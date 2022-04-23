namespace MinecraftServer.Networking;

public class StreamAdapter : DataAdapter
{
    public StreamAdapter(Stream bidirectionStream, long readBound = -1, CancellationToken ct = default) : base(ct)
    {
        ReadStream = bidirectionStream;
        WriteStream = bidirectionStream;
        if(readBound != -1) RemainingLength = readBound;
    }

    public Stream ReadStream { get; }
    public Stream WriteStream { get; }
    private long _readBytes;
    private long _writtenBytes;
    private bool _endOfStream = false;
    public override long BytesRead => _readBytes;
    public override long BytesWritten => _writtenBytes;
    public override bool EndOfPhysicalStream => _endOfStream;

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

    public override async ValueTask<int> ReadAsync(Memory<byte> buf, CancellationToken ct = default)
    {
        if (RemainingLength == 0) return 0;
        var len = await ReadStream.ReadAsync(buf.Slice(0, (int)Math.Min(RemainingLength, buf.Length)), ct);
        RemainingLength -= len;
        _readBytes += len;
        if(RemainingLength != 0 && len == 0) _endOfStream = true;
        return len;
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buf, CancellationToken ct = default)
    {
        _writtenBytes += buf.Length;
        return WriteStream.WriteAsync(buf, ct);
    }
}