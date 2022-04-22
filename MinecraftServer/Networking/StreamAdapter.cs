namespace MinecraftServer.Networking;

public class StreamAdapter : DataAdapter
{
    public StreamAdapter(Stream readStream, Stream writeStream, CancellationToken ct = default) : base(ct)
    {
        ReadStream = readStream;
        WriteStream = writeStream;
    }
    public StreamAdapter(Stream bidirectionStream, CancellationToken ct = default) : base(ct)
    {
        ReadStream = bidirectionStream;
        WriteStream = bidirectionStream;
    }

    public Stream ReadStream { get; }
    public Stream WriteStream { get; }

    public override async ValueTask DisposeAsync()
    {
        await ReadStream.DisposeAsync();
        await WriteStream.DisposeAsync();
    }

    public override void Flush()
    {
        WriteStream.Flush();
    }

    public override void Close()
    {
        ReadStream.Close();
        WriteStream.Close();
        base.Close();
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buf, CancellationToken ct = default)
    {
        return ReadStream.ReadAsync(buf, ct);
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buf, CancellationToken ct = default)
    {
        return WriteStream.WriteAsync(buf, ct);
    }
}