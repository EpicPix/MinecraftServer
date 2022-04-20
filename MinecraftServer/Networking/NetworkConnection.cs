using MinecraftServer.Packets;
using MinecraftServer.Packets.Clientbound.Data;
using MinecraftServer.Packets.Clientbound.Play;

namespace MinecraftServer.Networking;

public class NetworkConnection : DataAdapter, IDisposable
{
    public PacketType CurrentState { get; private set; } = PacketType.Handshake;
    public bool Connected = true;
    public string? Username = null;
    public byte[] VerifyToken;
    public byte[] EncryptionKey;
    public bool IsCompressed = false;
    public GameProfile? Profile { get; internal set; }
    private Stack<DataAdapter> _transformerStack = new();
    public PlayerPacketQueue PacketQueue { get; }
    public DateTime LastKeepAlive = DateTime.MinValue;
    public DateTime LastKeepAliveValue = DateTime.MinValue;

    public NetworkConnection(Stream client)
    {
        _transformerStack.Push(new StreamAdapter(client));
        PacketQueue = new PlayerPacketQueue();
    }

    public void Disconnect()
    {
        throw new NotImplementedException();
    }

    public void ChangeState(PacketType newState)
    {
        if (newState <= CurrentState)
        {
            throw new InvalidOperationException(
                $"The state transition is not allowed, from {CurrentState} to {newState}");
        }

        if (newState is PacketType.Play)
        {
            Task.Run(SendKeepAlive);  
        }

        CurrentState = newState;
    }
    
    private async Task SendKeepAlive()
    {
        await Task.Delay(5000);
        try
        {
            while (Connected)
            {
                if (LastKeepAlive != DateTime.MinValue)
                {
                    if (DateTime.UtcNow - LastKeepAlive > TimeSpan.FromSeconds(30))
                    {
                        Disconnect();
                    }
                }
                else
                {
                    var time = DateTime.UtcNow;
                    LastKeepAliveValue = time;
                    await ScPlayKeepAlive.Send(new ScPlayKeepAlivePacketData()
                    {
                        KeepAliveId = time.Ticks
                    }, this);
                }
                await Task.Delay(5000);
            }
        }
        catch(Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void Encrypt()
    {
        AddTransformer((x) => 
            new EncryptionAdapter(x, EncryptionKey));
    }

    public void PopTransformer()
    {
        _transformerStack.Pop();
    }

    public void AddTransformer(Func<DataAdapter, DataAdapter> transform)
    {
        _transformerStack.Push(transform(_transformerStack.Peek()));
    }
    
    public new void Dispose()
    {
        _transformerStack.Peek().Dispose();
    }

    public override void Flush()
    {
        _transformerStack.Peek().Flush();
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buf, CancellationToken ct = default)
    {
        return _transformerStack.Peek().ReadAsync(buf, ct);
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buf, CancellationToken ct = default)
    {
        return _transformerStack.Peek().WriteAsync(buf, ct);
    }
}