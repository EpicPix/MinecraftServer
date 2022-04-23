using System.Net.Sockets;
using System.Security.Cryptography;
using MinecraftServer.Packets;
using MinecraftServer.Packets.Clientbound.Data;
using MinecraftServer.Packets.Clientbound.Login;
using MinecraftServer.Packets.Clientbound.Play;

namespace MinecraftServer.Networking;

public class NetworkConnection : DataAdapter
{
    public PacketType CurrentState { get; private set; } = PacketType.Handshake;
    public string? Username = null;
    public Guid Uuid;
    public byte[] VerifyToken;
    public byte[] EncryptionKey;
    public bool IsCompressed = false;
    public GameProfile? Profile { get; internal set; }
    private Stack<DataAdapter> _transformerStack = new();
    public PlayerPacketQueue PacketQueue { get; }
    public DateTime LastKeepAliveSend = DateTime.MinValue;
    public DateTime LastKeepAlive = DateTime.MinValue;
    public long LastKeepAliveValue;

    public double PlayerX = 0;
    public double PlayerY = 0;
    public double PlayerZ = 0;

    private int _latency;
    public int Latency {
        get => _latency;
        set {
            Console.WriteLine($"Ping for {Username} set to {value}ms");
            _latency = value;
        }
    }

    public CancellationToken ConnectionState { get; }
    public bool Connected => !ConnectionState.IsCancellationRequested;
    private CancellationTokenSource _stateSource;
    public bool HasMoreToRead { get; private set; } = true;

    public Socket Socket => ((NetworkStream) ((StreamAdapter) _transformerStack.ToArray()[_transformerStack.Count - 1]).ReadStream).Socket;

    public NetworkConnection(Server server, Stream client, CancellationToken shutdownToken = default) : base(shutdownToken)
    {
        _transformerStack.Push(new StreamAdapter(client));
        PacketQueue = new PlayerPacketQueue(server);
        _stateSource = CancellationTokenSource.CreateLinkedTokenSource(shutdownToken);
        ConnectionState = _stateSource.Token;
    }

    public async ValueTask Disconnect(string reason = "", bool remote = false)
    {
        if (remote)
        {
            CloseConnection(this);
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
            Task.Run(async () =>
            {
                await Task.Delay(5000);
                // make sure to close the connection after 5 second timeout
                if (Connected)
                {
                    CloseConnection(this);
                }
            });
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
            Task.Run(SendKeepAlive, ConnectionState);
        }

        CurrentState = newState;
    }
    
    private async void SendKeepAlive()
    {
        await Task.Delay(10000, ConnectionState);
        try
        {
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
                
                var randomId = (long) RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue) << 32 | (uint) RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);
                LastKeepAliveValue = randomId;
                await ScPlayKeepAlive.SendAsync(new ScPlayKeepAlivePacketData(randomId), this, connection => {
                    LastKeepAliveSend = DateTime.UtcNow;
                    return ValueTask.CompletedTask;
                });
                await Task.Delay(10000, ConnectionState);
            }
        }
        catch(Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void Encrypt()
    {
        AddTransformer((x, ct) => 
            new EncryptionAdapter(x, EncryptionKey, ct));
    }

    public void PopTransformer()
    {
        _transformerStack.Pop();
    }

    public void AddTransformer(Func<DataAdapter, CancellationToken, DataAdapter> transform)
    {
        _transformerStack.Push(transform(_transformerStack.Peek(), ConnectionState));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _stateSource.Dispose();
            PacketQueue.Stop();
            _transformerStack.Peek().Dispose();
        }

        base.Dispose(disposing);
    }

    public override void Flush()
    {
        _transformerStack.Peek().Flush();
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buf, CancellationToken ct = default)
    {
        if (!HasMoreToRead)
        {
            await Task.Delay(100); // prevent high cpu usage from waiting for all data to be written
            return 0;
        }
        int res = await _transformerStack.Peek().ReadAsync(buf, ct);
        if (res == 0)
        {
            HasMoreToRead = false;
        }

        return res;
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buf, CancellationToken ct = default)
    {
        return _transformerStack.Peek().WriteAsync(buf, ct);
    }
}