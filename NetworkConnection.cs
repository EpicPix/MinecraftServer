using System.Data;
using System.Net.Sockets;
using System.Text;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace MinecraftServer;

public class NetworkConnection
{

    private readonly AsyncBinaryReader _reader;
    private readonly AsyncBinaryWriter _writer;
    
    public NetworkConnection(MemoryStream stream)
    {
        _reader = new AsyncBinaryReader(stream);
        _writer = new AsyncBinaryWriter(stream);
    }
    
    public NetworkConnection(Socket socket)
    {
        var stream = new NetworkStream(socket);
        _reader = new AsyncBinaryReader(stream);
        _writer = new AsyncBinaryWriter(stream);
    }

    public async Task WriteUByte(byte value)
    {
        await _writer.WriteAsync(value);
    }

    public async Task<byte> ReadUByte()
    {
        return await _reader.ReadByteAsync();
    }

    public async Task<ushort> ReadUShort()
    {
        var high = (ushort) await ReadUByte();
        var low = (ushort) await ReadUByte();
        return (ushort) (high << 8 | low);
    }

    public async Task WriteUShort(ushort value)
    {
        await WriteUByte((byte) (value >> 8));
        await WriteUByte((byte) value);
    }

    public async Task<ulong> ReadULong()
    {
        return (ulong) await ReadUByte() << 56 | 
               (ulong) await ReadUByte() << 48 | 
               (ulong) await ReadUByte() << 40 | 
               (ulong) await ReadUByte() << 32 | 
               (ulong) await ReadUByte() << 24 | 
               (ulong) await ReadUByte() << 16 | 
               (ulong) await ReadUByte() << 8 | 
               await ReadUByte();
    }

    public async Task WriteULong(ulong value)
    {
        await WriteUByte((byte) (value >> 56));
        await WriteUByte((byte) (value >> 48));
        await WriteUByte((byte) (value >> 40));
        await WriteUByte((byte) (value >> 32));
        await WriteUByte((byte) (value >> 24));
        await WriteUByte((byte) (value >> 16));
        await WriteUByte((byte) (value >> 8));
        await WriteUByte((byte) value);
    }

    public async Task<string> ReadString(ushort max)
    {
        var length = await ReadVarInt();
        if (length > max)
        {
            throw new ConstraintException($"string length {length} > {max}");
        }

        return Encoding.UTF8.GetString(await _reader.ReadBytesAsync(length));
    }

    public async Task WriteString(string str, ushort max)
    {
        if (str.Length > max)
        {
            throw new ConstraintException($"string length {str.Length} > {max}");
        }

        await WriteVarInt(str.Length);
        await _writer.WriteAsync(Encoding.UTF8.GetBytes(str));
    }
    
    public async Task<int> ReadVarInt()
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

    public async Task WriteVarInt(int value)
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

    public async Task WriteBytes(byte[] bytes)
    {
        await _writer.WriteAsync(bytes);
    }

    public async Task Flush()
    {
        await _writer.FlushAsync();
    }

    public int GetVarIntLength(int value)
    {
        int len = 0;
        while (true)
        {
            len++;
            if ((value & ~0x7F) == 0)
            {
                return len;
            }
            
            value >>= 7;
        }
    }

}