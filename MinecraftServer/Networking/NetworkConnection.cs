using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Security.Cryptography;
using MinecraftServer.Events;
using MinecraftServer.Packets;
using MinecraftServer.Packets.Clientbound.Data;
using MinecraftServer.Packets.Clientbound.Login;
using MinecraftServer.Packets.Clientbound.Play;

namespace MinecraftServer.Networking;

public class NetworkConnection : DataAdapter
{
    public PacketType CurrentState { get; private set; } = PacketType.Handshake;
    public string Username = null!;
    public Guid Uuid;
    public byte[] VerifyToken = null!;
    public byte[] EncryptionKey = null!;
    public bool IsCompressed = false;
    public GameProfile? Profile { get; internal set; }
    private Stack<DataAdapter> _readTransformerStack = new();
    private Stack<DataAdapter> _writeTransformerStack = new();
    public PlayerPacketQueue PacketQueue { get; }
    public DateTime LastKeepAliveSend = DateTime.MinValue;
    public DateTime LastKeepAlive = DateTime.MinValue;
    private readonly IoBuffer.IAllocator _readAllocator = new IoBuffer.SingleThreadAllocator(1 << 15);
    private readonly IoBuffer.IAllocator _writeAllocator = new IoBuffer.SingleThreadAllocator(1 << 15);
    protected override IoBuffer.IAllocator ReadAllocator => _readAllocator;
    protected override IoBuffer.IAllocator WriteAllocator => _writeAllocator;
    public long LastKeepAliveValue;
    public long RawBytesRead => _readTransformerStack.Last().BytesRead;
    public override bool EndOfPhysicalStream => _readTransformerStack.Last().EndOfPhysicalStream;
    public long RawBytesWritten => _readTransformerStack.Last().BytesWritten;

    public CancellationToken ConnectionState { get; }
    public bool Connected => !ConnectionState.IsCancellationRequested;
    private CancellationTokenSource _stateSource;

    public Player Player = null!;
    public ConcurrentDictionary<(int, int), bool> SentChunks = new ();
    public PacketEventBus EventBus;

    public NetworkConnection(Server server, Stream client, CancellationToken shutdownToken = default) : base(shutdownToken)
    {
        EventBus = new PacketEventBus(this, server);
        _readTransformerStack.Push(new StreamAdapter(client));
        _writeTransformerStack.Push(new StreamAdapter(client));
        PacketQueue = new PlayerPacketQueue(server, EventBus);
        _stateSource = CancellationTokenSource.CreateLinkedTokenSource(shutdownToken);
        ConnectionState = _stateSource.Token;
    }

    public async ValueTask Disconnect(string reason = "", bool remote = false)
    {
        if (remote)
        {
            await CloseConnection(this);
        }
        else
        {
            if (CurrentState == PacketType.Login)
            {
                await ScLoginDisconnect.SendAsync(new ScDisconnectPacketData(new ChatComponent(reason)), this, CloseConnection, true);
            }
            else if (CurrentState == PacketType.Play)
            {
                await ScPlayDisconnect.SendAsync(new ScDisconnectPacketData(new ChatComponent(reason)), this, CloseConnection, true);
            }
        }
    }

    private ValueTask CloseConnection(NetworkConnection conn)
    {
        _stateSource.Cancel();
        Dispose();
        return ValueTask.CompletedTask;
    }

    public void ChangeState(PacketType newState)
    {
        if (newState == CurrentState) return;
        
        if (newState < CurrentState)
        {
            throw new InvalidOperationException(
                $"The state transition is not allowed, from {CurrentState} to {newState}");
        }

        if (newState == PacketType.Play)
        {
            Task.Run(SendKeepAlive);
        }

        CurrentState = newState;
    }
    
    private async void SendKeepAlive()
    {
        try
        {
            await Task.Delay(10000, ConnectionState);
            while (Connected)
            {
                if (LastKeepAlive != DateTime.MinValue)
                {
                    if (DateTime.UtcNow - LastKeepAlive > TimeSpan.FromSeconds(30))
                    {
                        await Disconnect("Timed out");
                        break;
                    }
                }

                var randomId = (long)RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue) << 32 |
                               (uint)RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);
                LastKeepAliveValue = randomId;
                await ScPlayKeepAlive.SendAsync(new ScPlayKeepAlivePacketData(randomId), this, connection =>
                {
                    LastKeepAliveSend = DateTime.UtcNow;
                    return ValueTask.CompletedTask;
                });
                await Task.Delay(10000, ConnectionState);
            }
        }
        catch (TaskCanceledException) { }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void Encrypt()
    {
        var transform = (DataAdapter x, CancellationToken ct) =>
            new EncryptionAdapter(x, EncryptionKey, ct);
        AddTransformer(transform, true);
        AddTransformer(transform, false);
    }

    public void PopTransformer(bool isRead)
    {
        if (isRead)
        {
            _readTransformerStack.Pop();
        }
        else
        {
            _writeTransformerStack.Pop();
        }
    }

    public void AddTransformer(Func<DataAdapter, CancellationToken, DataAdapter> transform, bool isRead)
    {
        if (isRead)
        {
            _readTransformerStack.Push(transform(_readTransformerStack.Peek(), ConnectionState));
        }
        else
        {
            _writeTransformerStack.Push(transform(_writeTransformerStack.Peek(), ConnectionState));
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _stateSource.Dispose();
            PacketQueue.Stop();
            _readTransformerStack.Peek().Dispose();
            _writeTransformerStack.Peek().Dispose();
        }

        base.Dispose(disposing);
    }

    public override void Flush()
    {
        _writeTransformerStack.Peek().Flush();
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buf, CancellationToken ct = default)
    {
        if (EndOfPhysicalStream)
        {
            //await Task.Delay(100); // prevent high cpu usage from waiting for all data to be written
            return 0;
        }
        int res = await _readTransformerStack.Peek().ReadAsync(buf, ct);
        return res;
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buf, CancellationToken ct = default)
    {
        return _writeTransformerStack.Peek().WriteAsync(buf, ct);
    }
}