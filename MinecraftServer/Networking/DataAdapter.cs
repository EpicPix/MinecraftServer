using System.Data;
using System.Text;

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
        return ReadAsync(new ArraySegment<byte>(buffer, offset, count)).Result;
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
        WriteBytes(new ArraySegment<byte>(buffer, offset, count)).GetAwaiter().GetResult();
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

    public ValueTask WriteBool(bool value)
    {
        using var tbuf = IoBuffer.Allocate(1);
        tbuf[0] = (byte)(value ? 1 : 0);
        return WriteBytes(tbuf);
    }

    public async ValueTask<bool> ReadBool()
    {
        using var tbuf = await ReadBytes(1);
        return tbuf[0] != 0;
    }


    public async ValueTask WriteUByte(byte value)
    {
        _singleByteWrite[0] = value;
        await WriteBytes(_singleByteWrite);
    }

    public async ValueTask<byte> ReadUByte()
    {
        await ReadAsync(_singleByteRead);
        return _singleByteRead[0];
    }

    public async ValueTask<ushort> ReadUShort()
    {
        using var tbuf = await ReadBytes(2);
        return (ushort) (tbuf[0] << 8 | tbuf[1]);
    }

    public async ValueTask WriteUShort(ushort value)
    {
        using var tbuf = WriteAllocator.Allocate(2);
        tbuf[0] = (byte)(value >> 8);
        tbuf[1] = (byte)value;
        await WriteBytes(tbuf);
    }
    
    public async ValueTask<int> ReadInt()
    {
        using var tbuf = await ReadBytes(4);
        return tbuf[0] << 24 | tbuf[1] << 16 | tbuf[2] << 8 | tbuf[3];
    }

    public ValueTask WriteInt(int value)
    {
        using var tbuf = IoBuffer.Allocate(4);
        tbuf[0] = (byte) (value >> 24);
        tbuf[1] = (byte) (value >> 16);
        tbuf[2] = (byte) (value >> 8);
        tbuf[3] = (byte) value;
        return WriteBytes(tbuf);
    }

    public async ValueTask<float> ReadFloat()
    {
        return BitConverter.Int32BitsToSingle(await ReadInt());
    }

    public ValueTask WriteFloat(float value)
    {
        return WriteInt(BitConverter.SingleToInt32Bits(value));
    }

    public async ValueTask<double> ReadDouble()
    {
        return BitConverter.UInt64BitsToDouble(await ReadULong());
    }

    public ValueTask WriteDouble(double value)
    {
        return WriteULong(BitConverter.DoubleToUInt64Bits(value));
    }

    public async ValueTask<Guid> ReadUUID()
    {
        using var tbuf = await ReadBytes(16);
        return new Guid(tbuf.Data);
    }

    public ValueTask WriteUUID(Guid uuid)
    {
        using var tbuf = WriteAllocator.Allocate(16);
        uuid.TryWriteBytes(tbuf.Data);
        return WriteBytes(tbuf);
    }

    public async ValueTask<ulong> ReadULong()
    {
        using var tbuf = await ReadBytes(8);
        return (ulong) tbuf[0] << 56 | 
               (ulong) tbuf[1] << 48 | 
               (ulong) tbuf[2] << 40 | 
               (ulong) tbuf[3] << 32 | 
               (ulong) tbuf[4] << 24 | 
               (ulong) tbuf[5] << 16 | 
               (ulong) tbuf[6] << 8 | 
               tbuf[7];
    }

    public ValueTask WriteULong(ulong value)
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
        return WriteBytes(tbuf);
    }
    
    public async ValueTask WriteBytesLen(Memory<byte> bytes, uint max)
    {
        if (bytes.Length > max)
        {
            throw new ConstraintException($"bytes length {bytes.Length} > {max}");
        }
        await WriteVarInt(bytes.Length);
        await WriteBytes(bytes);
    }

    public ValueTask ReadBytes(ArraySegment<byte> toRead)
    {
        return Utils.FillBytes(toRead, this, _ct);
    }
    
    public ValueTask ReadBytes(IoBuffer toRead)
    {
        return ReadBytes(toRead.Data);
    }

    public async ValueTask<IoBuffer> ReadBytes(int toRead)
    {
        // do not do "using" on this one. we don't want to dispose the returned buffer yet
        var buf = IoBuffer.Allocate(toRead);
        await ReadBytes(buf);
        return buf;
    }

    public async ValueTask<string> ReadString(ushort max)
    {
        var length = await ReadVarInt();
        if (length > max)
        {
            throw new ConstraintException($"string length {length} > {max}");
        }

        using var bytes = await ReadBytes(length);
        return Encoding.UTF8.GetString(bytes.Data);
    }

    public async ValueTask WriteString(string str, ushort max)
    {
        if (str.Length > max)
        {
            throw new ConstraintException($"string length {str.Length} > {max}");
        }

        await WriteVarInt(str.Length);
        using var buf = WriteAllocator.Allocate(Encoding.UTF8.GetByteCount(str));
        Encoding.UTF8.GetBytes(str, buf.Data);
        await WriteBytes(buf.Data);
    }
    
    public async ValueTask WriteStringShort(string str)
    { 
        await WriteUShort((ushort)str.Length);
        using var buf = WriteAllocator.Allocate(Encoding.UTF8.GetByteCount(str));
        Encoding.UTF8.GetBytes(str, buf.Data);
        await WriteBytes(buf.Data);
    }

    public async ValueTask Skip(int bytes)
    {
        int rem = bytes;
        while (rem > 0 && !_ct.IsCancellationRequested && !EndOfPhysicalStream)
        {
            var res = await ReadAsync(new ArraySegment<byte>(_drainBytes, 0, Math.Min(4096, rem)), _ct);
            rem -= res;
            if (res == 0) break;
        }
    }
    
    public async ValueTask<int> ReadVarInt()
    {
        int value = 0;
        int position = 0;
        byte currentByte;

        while (!_ct.IsCancellationRequested && !EndOfPhysicalStream)
        {
            currentByte = await ReadUByte();
            value |= (currentByte & 0x7F) << position;

            if ((currentByte & 0x80) == 0) break;
            
            position += 7;
            if (position >= 32) throw new ArgumentException("VarInt is too big");
        }
        return value;
    }

    public async ValueTask WriteVarInt(int value)
    {
        using var buf = WriteAllocator.Allocate(5);
        var length = Utils.WriteVarInt(value, buf.Data);
        await WriteBytes(buf.Data.Slice(0, length));
    }

    public ValueTask WriteBytes(Memory<byte> bytes)
    {
        return WriteAsync(bytes, _ct);
    }
    
    public ValueTask WriteBytes(IoBuffer bytes)
    {
        return WriteAsync(bytes.Data, _ct);
    }
}