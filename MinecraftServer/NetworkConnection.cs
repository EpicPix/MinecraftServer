using System.ComponentModel.Design;
using System.Data;
using System.Net.Sockets;
using System.Text;
using MinecraftServer.Packets;

namespace MinecraftServer;

public class NetworkConnection : IDisposable
{

    public Socket? Socket { get; }
    public BinaryReader? Reader { get; set; }
    public BinaryWriter? Writer { get; }

    public PacketType CurrentState = PacketType.Handshake;
    public bool Connected = true;
    public string? Username = null;

    public UncompletedPacket? CurrentPacket { get; set; }

    public NetworkConnection(Socket? socket, BinaryReader? reader, BinaryWriter? writer)
    {
        Socket = socket;
        Reader = reader;
        Writer = writer;
    }

    public void ReadPacket()
    {
        if (Socket == null) throw new NullReferenceException("Socket is null");
        
        if (CurrentPacket != null) {
            var packet = (UncompletedPacket) CurrentPacket;
            if (packet.Offset == packet.Data.Length)
            {
                throw new InvalidOperationException("Tried to read packet that is already completed");
            }
            var required = (int) (packet.Data.Length - packet.Offset);
            var read = Math.Min(Socket.Available, required);
            if (read != 0)
            {
                var readAmount = Socket.Receive(packet.Data, (int) packet.Offset, read, 0);
                packet.Offset += (uint) readAmount;
                CurrentPacket = packet;
            }
            return;
        }
        throw new NullReferenceException("Tried to read packet while there is no packet in progress");
    }

    public void WriteUByte(byte value)
    {
        Span<byte> r = stackalloc byte[1];
        r[0] = value;
        if (Writer != null) 
            Writer.Write(r);
        else 
            Socket?.Send(r);
    }

    public byte ReadUByte()
    {
        Span<byte> r = stackalloc byte[1];
        if (Reader != null)
            Reader.Read(r);
        else
            Socket?.Receive(r);

        return r[0];
    }

    public ushort ReadUShort()
    {
        Span<byte> r = stackalloc byte[2];
        if (Reader != null)
            Reader.Read(r);
        else
            Socket?.Receive(r);
        
        return (ushort) (r[0] << 8 | r[1]);
    }

    public void WriteUShort(ushort value)
    {
        Span<byte> r = stackalloc byte[2];
        r[0] = (byte) (value >> 8);
        r[1] = (byte) value;
        if (Writer != null) 
            Writer.Write(r);
        else 
            Socket?.Send(r);
    }

    public Guid ReadUUID()
    {
        Span<byte> r = stackalloc byte[16];
        if (Reader != null)
            Reader.Read(r);
        else
            Socket?.Receive(r);
        
        return new Guid(r);
    }

    public void WriteUUID(Guid uuid)
    {
        Span<byte> r = stackalloc byte[16];
        uuid.TryWriteBytes(r);
        
        if (Writer != null) 
            Writer.Write(r);
        else 
            Socket?.Send(r);
    }

    public ulong ReadULong()
    {
        Span<byte> r = stackalloc byte[8];
        if (Reader != null)
            Reader.Read(r);
        else
            Socket?.Receive(r);
        
        return (ulong) r[0] << 56 |
               (ulong) r[1] << 48 |
               (ulong) r[2] << 40 |
               (ulong) r[3] << 32 |
               (ulong) r[4] << 24 |
               (ulong) r[5] << 16 |
               (ulong) r[6] << 8 |
               r[7];
    }

    public void WriteULong(ulong value)
    {
        Span<byte> r = stackalloc byte[8];
        r[0] = (byte) (value >> 56);
        r[1] = (byte) (value >> 48);
        r[2] = (byte) (value >> 40);
        r[3] = (byte) (value >> 32);
        r[4] = (byte) (value >> 24);
        r[5] = (byte) (value >> 16);
        r[6] = (byte) (value >> 8);
        r[7] = (byte) value;
        if (Writer != null) 
            Writer.Write(r);
        else 
            Socket?.Send(r);
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

        int numRead = -1;
        if (Reader != null)
        {
            numRead = Reader.Read(bytes);
        }else if (Socket != null)
        {
            numRead = Socket.Receive(bytes, 0);
        }
        
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
        Writer?.Write(bytes);
    }

    public void WriteString(string str, uint max)
    {
        if (str.Length > max)
        {
            throw new ConstraintException($"string length {str.Length} > {max}");
        }

        WriteVarInt(str.Length);
        Writer?.Write(Encoding.UTF8.GetBytes(str));
    }
    
    public int ReadVarIntBytes(out int val)
    {
        if (Socket == null) throw new NotSupportedException("Socket must be set");
        
        int read = 0;
        int position = 0;
        val = 0;

        Span<byte> r = stackalloc byte[1];
        
        while (true)
        {
            if (Socket.Available <= 0)
            {
                break;
            }
            int readAmount = Socket.Receive(r);
            if (readAmount == 0)
            {
                break;
            }
            read++;
            val |= (r[0] & 0x7F) << position;

            if ((r[0] & 0x80) == 0) break;
            
            position += 7;
            if (position >= 32) throw new ArgumentException("VarInt is too big");
        }
        return read;
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
        if (Writer != null)
        {
            Writer.Write(bytes);
        } else if(Socket != null)
        {
            Socket.Send(bytes);
        }
    }

    public void Close()
    {
        if (Socket != null)
        {
            Socket.Close();
            return;
        }
        
        throw new NullReferenceException("Tried to close a null socket");
    }

    public void Dispose()
    {
        Reader?.Dispose();
        Writer?.Dispose();
    }
}