using MinecraftServer.Packets;

namespace MinecraftServer.Networking;

public class NetworkConnection : DataAdapter, IDisposable
{
    private Stream _readStream;
    private Stream _writeStream;

    public PacketType CurrentState = PacketType.Handshake;
    public bool Connected = true;
    public string? Username = null;
    public byte[] VerifyToken;
    public byte[] EncryptionKey;
    public bool IsCompressed = false;
    public GameProfile? PlayerProfile;
    private Stack<DataAdapter> _transformerStack = new();

    public NetworkConnection(Stream client)
    {
        _transformerStack.Push(new StreamAdapter(client));
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
    
    public void Dispose()
    {
        _readStream.Dispose();
        _writeStream.Dispose();
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