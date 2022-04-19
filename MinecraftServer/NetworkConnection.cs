using System.ComponentModel.Design;
using System.Data;
using System.Net.Sockets;
using System.Text;
using MinecraftServer.Packets;

namespace MinecraftServer;

public class NetworkConnection : IDisposable
{

    private readonly Socket? _socket;
    private readonly BinaryReader _reader;
    private readonly BinaryWriter _writer;

    public PacketType CurrentState = PacketType.Handshake;
    public bool Connected = true;
    public string? Username = null;
    
    public bool CanRead => _reader.BaseStream.CanRead;

    public NetworkConnection(Stream stream)
    {
        _reader = new BinaryReader(stream);
        _writer = new BinaryWriter(stream);
    }
    
    public NetworkConnection(Socket socket)
    {
        _socket = socket;
        var stream = new NetworkStream(socket);
        _reader = new BinaryReader(stream);
        _writer = new BinaryWriter(stream);
    }

    public void WriteUByte(byte value)
    {
        _writer.Write(value);
    }

    public byte ReadUByte()
    {
        return _reader.ReadByte();
    }

    public ushort ReadUShort()
    {
        var high = (ushort) ReadUByte();
        var low = (ushort) ReadUByte();
        return (ushort) (high << 8 | low);
    }

    public void WriteUShort(ushort value)
    {
        WriteUByte((byte) (value >> 8));
        WriteUByte((byte) value);
    }

    public ulong ReadULong()
    {
        return (ulong) ReadUByte() << 56 | 
               (ulong) ReadUByte() << 48 | 
               (ulong) ReadUByte() << 40 | 
               (ulong) ReadUByte() << 32 | 
               (ulong) ReadUByte() << 24 | 
               (ulong) ReadUByte() << 16 | 
               (ulong) ReadUByte() << 8 | 
               ReadUByte();
    }

    public void WriteULong(ulong value)
    {
        WriteUByte((byte) (value >> 56));
        WriteUByte((byte) (value >> 48));
        WriteUByte((byte) (value >> 40));
        WriteUByte((byte) (value >> 32));
        WriteUByte((byte) (value >> 24));
        WriteUByte((byte) (value >> 16));
        WriteUByte((byte) (value >> 8));
        WriteUByte((byte) value);
    }

    public string ReadString(uint max)
    {
        var length = ReadVarInt();
        if (length > max)
        {
            throw new ConstraintException($"string length {length} > {max}");
        }

        Span<byte> byteBuf = stackalloc byte[length];
        ReadBytes(byteBuf);
        return Encoding.UTF8.GetString(byteBuf);
    }
    
    public void ReadBytes(Span<byte> bytes)
    {
        if (bytes.Length == 0)
        {
            return;
        }

        int count = bytes.Length;
        int numRead = 0;
        do
        {
            int n = _reader.Read(bytes.Slice(numRead, count));
            if (n == 0)
            {
                break;
            }

            numRead += n;
            count -= n;
        } while (count > 0);

        if (numRead != bytes.Length)
        {
            throw new CheckoutException($"The stream reached EOF before reading all of the expected bytes");
        }
    }
    
    public void WriteBytesLen(byte[] bytes, uint max)
    {
        if (bytes.Length > max)
        {
            throw new ConstraintException($"bytes length {bytes.Length} > {max}");
        }
        WriteVarInt(bytes.Length);
        _writer.Write(bytes);
    }

    public void WriteString(string str, uint max)
    {
        if (str.Length > max)
        {
            throw new ConstraintException($"string length {str.Length} > {max}");
        }

        WriteVarInt(str.Length);
        _writer.Write(Encoding.UTF8.GetBytes(str));
    }
    
    public int ReadVarInt()
    {
        int value = 0;
        int position = 0;
        byte currentByte;

        while (true)
        {
            currentByte = ReadUByte();
            value |= (currentByte & 0x7F) << position;

            if ((currentByte & 0x80) == 0) break;
            
            position += 7;
            if (position >= 32) throw new ArgumentException("VarInt is too big");
        }
        return value;
    }

    public void WriteVarInt(int value)
    {
        while (true)
        {
            if ((value & ~0x7F) == 0)
            {
                WriteUByte((byte) value);
                return;
            }
            
            WriteUByte((byte) ((value & 0x7F) | 0x80));
            value >>= 7;
        }
    }

    public void WriteBytes(ReadOnlySpan<byte> bytes)
    {
        _writer.Write(bytes);
    }

    public void Flush()
    {
        _writer.Flush();
    }

    public void Close()
    {
        if (_socket != null)
        {
            _socket.Close();
            return;
        }
        
        throw new NullReferenceException("Tried to close a null socket");
    }

    public void Dispose()
    {
        _reader.Dispose();
        _writer.Dispose();
    }
}