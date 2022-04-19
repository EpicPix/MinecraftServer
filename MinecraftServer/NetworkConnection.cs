using System.ComponentModel.Design;
using System.Data;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using MinecraftServer.Packets;

namespace MinecraftServer;

public class NetworkConnection : IDisposable
{
    private Stream _readStream;
    private Stream _writeStream;

    public PacketType CurrentState = PacketType.Handshake;
    public bool Connected = true;
    public string? Username = null;
    public byte[] VerifyToken;
    public byte[] EncryptionKey;
    public bool IsCompressed = false;
    private byte[] _singleByteRead = new byte[1];
    private byte[] _singleByteWrite = new byte[1];
    public GameProfile? PlayerProfile;

    public NetworkConnection(Stream client)
    {
        _readStream = client;
        _writeStream = client;
    }

    public void Encrypt()
    {
        var aes = Aes.Create();
        aes.Mode = CipherMode.CFB;
        aes.Key = EncryptionKey;
        aes.IV = EncryptionKey;
        _readStream = new CryptoStream(_readStream, aes.CreateEncryptor(), CryptoStreamMode.Read);
        _writeStream = new CryptoStream(_writeStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
    }
    
    public ValueTask WriteBool(bool value)
    {
        using var tbuf = PooledArray.Allocate(1);
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
        await _writeStream.WriteAsync(_singleByteWrite);
    }

    public async ValueTask<byte> ReadUByte()
    {
        await _readStream.ReadAsync(_singleByteRead);
        return _singleByteRead[0];
    }

    public async ValueTask<ushort> ReadUShort()
    {
        using var tbuf = await ReadBytes(2);
        return (ushort) (tbuf[0] << 8 | tbuf[1]);
    }

    public async ValueTask WriteUShort(ushort value)
    {
        using var tbuf = PooledArray.Allocate(2);
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
        using var tbuf = PooledArray.Allocate(4);
        tbuf[0] = (byte) (value >> 24);
        tbuf[1] = (byte) (value >> 16);
        tbuf[2] = (byte) (value >> 8);
        tbuf[3] = (byte) value;
        return WriteBytes(tbuf);
    }

    public ValueTask WriteFloat(float value)
    {
        return WriteInt(BitConverter.SingleToInt32Bits(value));
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
        using var tbuf = PooledArray.Allocate(16);
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
        using var tbuf = PooledArray.Allocate(8);
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
        await _writeStream.WriteAsync(bytes);
    }

    public async ValueTask ReadBytes(ArraySegment<byte> toRead)
    {
        int remLen = toRead.Count;
        int pos = 0;
        while (remLen > 0)
        {
            int read = await _readStream.ReadAsync(toRead.Slice(pos, remLen));
            pos += read;
            remLen -= read;
        }
    }
    
    public ValueTask ReadBytes(PooledArray toRead)
    {
        return ReadBytes(toRead.Data);
    }

    public async ValueTask<PooledArray> ReadBytes(int toRead)
    {
        var buf = PooledArray.Allocate(toRead);
        await ReadBytes(buf.Data);
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
        using var buf = PooledArray.Allocate(Encoding.UTF8.GetByteCount(str));
        Encoding.UTF8.GetBytes(str, buf.Data);
        await _writeStream.WriteAsync(buf.Data);
    }
    
    public async ValueTask WriteStringShort(string str)
    { 
        await WriteUShort((ushort)str.Length);
        using var buf = PooledArray.Allocate(Encoding.UTF8.GetByteCount(str));
        Encoding.UTF8.GetBytes(str, buf.Data);
        await _writeStream.WriteAsync(buf.Data);
    }
    
    public async ValueTask<int> ReadVarInt()
    {
        int value = 0;
        int position = 0;
        byte currentByte;

        while (true)
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
        while (true)
        {
            if ((value & ~0x7F) == 0)
            {
                await WriteUByte((byte) value);
                return;
            }
            
            await WriteUByte((byte) ((value & 0x7F) | 0x80));
            value >>= 7;
        }
    }

    public async ValueTask WriteBytes(Memory<byte> bytes)
    {
        await _writeStream.WriteAsync(bytes);
    }
    
    public async ValueTask WriteBytes(PooledArray bytes)
    {
        await _writeStream.WriteAsync(bytes.Data);
    }
    
    public void Dispose()
    {
        _readStream.Dispose();
        _writeStream.Dispose();
    }
}