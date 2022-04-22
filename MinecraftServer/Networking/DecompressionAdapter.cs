using System.IO.Compression;

namespace MinecraftServer.Networking;

public class DecompressionAdapter : DataAdapter
{
    private ZLibStream _stream;
    public DecompressionAdapter(DataAdapter baseAdapter, CancellationToken ct = default) : base(ct)
    {
        _stream = new ZLibStream(baseAdapter, CompressionMode.Decompress);
    }
    public override void Close()
    {
        _stream.Close();
        base.Close();
    }
    
    public override async ValueTask DisposeAsync()
    {
        await _stream.DisposeAsync();
    }

    public override void Flush()
    {
        _stream.Flush();    
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buf, CancellationToken ct = default)
    {
        return _stream.ReadAsync(buf, ct);
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buf, CancellationToken ct = default)
    {
        return _stream.WriteAsync(buf, ct);
    }
}