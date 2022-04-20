using System.IO.Compression;

namespace MinecraftServer.Networking;

public class CompressionAdapter : DataAdapter
{
    private ZLibStream _stream;
    public CompressionAdapter(DataAdapter baseAdapter)
    {
        _stream = new ZLibStream(baseAdapter, CompressionMode.Compress);
    }
    public override ValueTask<int> ReadAsync(Memory<byte> buf, CancellationToken ct = default)
    {
        throw new NotImplementedException();
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

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buf, CancellationToken ct = default)
    {
        return _stream.WriteAsync(buf, ct);
    }
}