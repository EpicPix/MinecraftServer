using System.Data;
using System.Text;
using MinecraftServer.Data;
using MinecraftServer.Types;

namespace MinecraftServer.Networking;

public abstract class DataAdapter : Stream
{
    private CancellationToken _ct;
    public DataAdapter(CancellationToken token)
    {
        _ct = token;
    }
    private static byte[] _drainBytes = new byte[4096];
    public override int Read(byte[] buffer, int offset, int count)
    {
        return ReadAsync(new ArraySegment<byte>(buffer, offset, count), _ct).Result;
    }

    public virtual long BytesRead => throw new NotImplementedException();
    public virtual long BytesWritten => throw new NotImplementedException();
    public virtual bool EndOfPhysicalStream => throw new NotImplementedException();
    public abstract override ValueTask<int> ReadAsync(Memory<byte> buf, CancellationToken ct = default);
    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException();
    }

    protected long RemainingLength = long.MaxValue;

    public override void SetLength(long value)
    {
        RemainingLength = value;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        WriteBytesAsync(new ArraySegment<byte>(buffer, offset, count)).GetAwaiter().GetResult();
    }

    private readonly IoBuffer.IAllocator _readAllocator = new IoBuffer.PooledArrayAllocator();
    private readonly IoBuffer.IAllocator _writeAllocator = new IoBuffer.PooledArrayAllocator();
    protected virtual IoBuffer.IAllocator ReadAllocator => _readAllocator;
    protected virtual IoBuffer.IAllocator WriteAllocator => _readAllocator;
    private byte[] _singleByteRead = new byte[1];
    private byte[] _singleByteWrite = new byte[1];
    public abstract override ValueTask WriteAsync(ReadOnlyMemory<byte> buf, CancellationToken ct = default);
    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => RemainingLength;
    public override long Position { get; set; }

    public ValueTask WriteBoolAsync(bool value)
    {
        using var tbuf = IoBuffer.Allocate(1);
        tbuf[0] = (byte)(value ? 1 : 0);
        return WriteBytesAsync(tbuf);
    }

    public async ValueTask<bool> ReadBoolAsync()
    {
        using var tbuf = await ReadBytesAsync(1);
        return tbuf[0] != 0;
    }


    public ValueTask WriteByteAsync(byte value)
    {
        _singleByteWrite[0] = value;
        return WriteBytesAsync(_singleByteWrite);
    }

    public async ValueTask<byte> ReadByteAsync()
    {
        await ReadAsync(_singleByteRead, _ct);
        return _singleByteRead[0];
    }

    public ValueTask WriteAngleAsync(Angle angle)
    {
        _singleByteWrite[0] = angle.Value;
        return WriteBytesAsync(_singleByteWrite);
    }

    public async ValueTask<Angle> ReadAngleAsync()
    {
        await ReadAsync(_singleByteRead, _ct);
        return new Angle(_singleByteRead[0]);
    }

    public async ValueTask<ushort> ReadUnsignedShortAsync()
    {
        using var tbuf = await ReadBytesAsync(2);
        return (ushort) (tbuf[0] << 8 | tbuf[1]);
    }

    public async ValueTask WriteUnsignedShortAsync(ushort value)
    {
        using var tbuf = WriteAllocator.Allocate(2);
        tbuf[0] = (byte)(value >> 8);
        tbuf[1] = (byte)value;
        await WriteBytesAsync(tbuf);
    }
    
    public async ValueTask<int> ReadIntAsync()
    {
        using var tbuf = await ReadBytesAsync(4);
        return tbuf[0] << 24 | tbuf[1] << 16 | tbuf[2] << 8 | tbuf[3];
    }

    public ValueTask WriteIntAsync(int value)
    {
        using var tbuf = IoBuffer.Allocate(4);
        tbuf[0] = (byte) (value >> 24);
        tbuf[1] = (byte) (value >> 16);
        tbuf[2] = (byte) (value >> 8);
        tbuf[3] = (byte) value;
        return WriteBytesAsync(tbuf);
    }

    public async ValueTask<float> ReadFloatAsync()
    {
        return BitConverter.Int32BitsToSingle(await ReadIntAsync());
    }

    public ValueTask WriteFloatAsync(float value)
    {
        return WriteIntAsync(BitConverter.SingleToInt32Bits(value));
    }

    public async ValueTask<double> ReadDoubleAsync()
    {
        return BitConverter.UInt64BitsToDouble(await ReadUnsignedLongAsync());
    }

    public ValueTask WriteDoubleAsync(double value)
    {
        return WriteUnsignedLongAsync(BitConverter.DoubleToUInt64Bits(value));
    }

    public async ValueTask<Guid> ReadUuidAsync()
    {
        using var tbuf = await ReadBytesAsync(16);
        return new Guid(tbuf.Data);
    }

    public ValueTask WriteUuidAsync(Guid uuid)
    {
        using var tbuf = WriteAllocator.Allocate(16);
        uuid.TryWriteBytes(tbuf.Data);
        return WriteBytesAsync(tbuf);
    }

    public async ValueTask<ulong> ReadUnsignedLongAsync()
    {
        using var tbuf = await ReadBytesAsync(8);
        return (ulong) tbuf[0] << 56 | 
               (ulong) tbuf[1] << 48 | 
               (ulong) tbuf[2] << 40 | 
               (ulong) tbuf[3] << 32 | 
               (ulong) tbuf[4] << 24 | 
               (ulong) tbuf[5] << 16 | 
               (ulong) tbuf[6] << 8 | 
               tbuf[7];
    }

    public ValueTask WriteUnsignedLongAsync(ulong value)
    {
        using var tbuf = WriteAllocator.Allocate(8);
        tbuf[0] = (byte) (value >> 56);
        tbuf[1] = (byte) (value >> 48);
        tbuf[2] = (byte) (value >> 40);
        tbuf[3] = (byte) (value >> 32);
        tbuf[4] = (byte) (value >> 24);
        tbuf[5] = (byte) (value >> 16);
        tbuf[6] = (byte) (value >> 8);
        tbuf[7] = (byte) value;
        return WriteBytesAsync(tbuf);
    }

    public ValueTask WritePositionAsync(Position position)
    {
        return WriteUnsignedLongAsync((ulong) position.ToLong());
    }
    
    public async ValueTask WriteBytesLenAsync(Memory<byte> bytes, uint max)
    {
        if (bytes.Length > max)
        {
            throw new ConstraintException($"bytes length {bytes.Length} > {max}");
        }
        await WriteVarIntAsync(bytes.Length);
        await WriteBytesAsync(bytes);
    }

    public ValueTask ReadBytesAsync(ArraySegment<byte> toRead)
    {
        return Utils.FillBytes(toRead, this, _ct);
    }
    
    public ValueTask ReadBytesAsync(IoBuffer toRead)
    {
        return ReadBytesAsync(toRead.Data);
    }

    public async ValueTask<IoBuffer> ReadBytesAsync(int toRead)
    {
        // do not do "using" on this one. we don't want to dispose the returned buffer yet
        var buf = IoBuffer.Allocate(toRead);
        await ReadBytesAsync(buf);
        return buf;
    }

    public async ValueTask<string> ReadStringAsync(ushort max)
    {
        var length = await ReadVarIntAsync();
        if (length > max)
        {
            throw new ConstraintException($"string length {length} > {max}");
        }

        using var bytes = await ReadBytesAsync(length);
        return Encoding.UTF8.GetString(bytes.Data);
    }

    public async ValueTask WriteStringAsync(string str, ushort max)
    {
        if (str.Length > max)
        {
            throw new ConstraintException($"string length {str.Length} > {max}");
        }

        await WriteVarIntAsync(str.Length);
        using var buf = WriteAllocator.Allocate(Encoding.UTF8.GetByteCount(str));
        Encoding.UTF8.GetBytes(str, buf.Data);
        await WriteBytesAsync(buf.Data);
    }
    
    public async ValueTask WriteStringShortAsync(string str)
    { 
        await WriteUnsignedShortAsync((ushort)str.Length);
        using var buf = WriteAllocator.Allocate(Encoding.UTF8.GetByteCount(str));
        Encoding.UTF8.GetBytes(str, buf.Data);
        await WriteBytesAsync(buf.Data);
    }

    public async ValueTask SkipAsync(int bytes)
    {
        int rem = bytes;
        while (rem > 0 && !_ct.IsCancellationRequested && !EndOfPhysicalStream)
        {
            var res = await ReadAsync(new ArraySegment<byte>(_drainBytes, 0, Math.Min(4096, rem)), _ct);
            rem -= res;
            if (res == 0) break;
        }
    }
    
    public async ValueTask<int> ReadVarIntAsync()
    {
        int value = 0;
        int position = 0;
        byte currentByte;

        while (!_ct.IsCancellationRequested && !EndOfPhysicalStream)
        {
            currentByte = await ReadByteAsync();
            value |= (currentByte & 0x7F) << position;

            if ((currentByte & 0x80) == 0) break;
            
            position += 7;
            if (position >= 32) throw new ArgumentException("VarInt is too big");
        }
        return value;
    }

    public async ValueTask WriteVarIntAsync(int value)
    {
        using var buf = WriteAllocator.Allocate(5);
        var length = Utils.WriteVarInt(value, buf.Data);
        await WriteBytesAsync(buf.Data.Slice(0, length));
    }

    public ValueTask WriteBytesAsync(Memory<byte> bytes)
    {
        return WriteAsync(bytes, _ct);
    }
    
    public ValueTask WriteBytesAsync(IoBuffer bytes)
    {
        return WriteAsync(bytes.Data, _ct);
    }
}