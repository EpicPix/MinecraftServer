using System.IO.Compression;

namespace MinecraftServer.Networking;

public class DecompressionAdapter : DataAdapter
{
    private DeflateStream _stream;
    public DecompressionAdapter(DataAdapter baseAdapter)
    {
        _stream = new DeflateStream(baseAdapter, CompressionMode.Compress);
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
        throw new NotImplementedException();
    }
}