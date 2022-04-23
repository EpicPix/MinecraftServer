using MinecraftServer.Nbt;
using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Play;

public class ScPlayChunkDataAndUpdateLight : Packet<ScPlayChunkDataAndUpdateLight, ScPlayChunkDataAndUpdateLightPacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0x22;

    public override ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        throw new NotImplementedException();
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData pData)
    {
        var data = Of(pData);
        
        await stream.WriteInt(data.ChunkX);
        await stream.WriteInt(data.ChunkZ);

        var arr = new NbtTagLongArray();
        // 7 bits per xz
        for (uint i = 0; i < 37; i++)
        {
            arr.Add(0);
        }
        
        await new NbtTagRoot()
            .Set("MOTION_BLOCKING", arr)
            .Write(stream);

        await using var ms = new MemoryStream();
        await using var s = new StreamAdapter(ms);
        for (uint i = 0; i < 16; i++)
        {
            await s.WriteUShort(16 * 16 * 8); // block count

            await s.WriteUByte(8); // palette bits
            await s.WriteVarInt(2); // palette length
            await s.WriteVarInt(1); // element[0]
            await s.WriteVarInt(2); // element[1]
            
            await s.WriteVarInt(16 * 16 * 2); // block data length (in longs)
            for (var y = 0; y < 16; y++)
            {
                for (var z = 0; z < 16; z++)
                {
                    for (var x = 0; x < 16; x++)
                    {
                        await s.WriteUByte((byte) (z >= 8 ? 0 : 1));
                    }
                }
            }

            await s.WriteUByte(0);
            await s.WriteVarInt(1);
            await s.WriteVarInt(0);
        }
        await stream.WriteVarInt((int) ms.Length);
        await stream.WriteBytes(ms.ToArray());
        
        await stream.WriteVarInt(0);

        await stream.WriteBool(true);
        await stream.WriteVarInt(0);
        await stream.WriteVarInt(0);
        await stream.WriteVarInt(0);
        await stream.WriteVarInt(0);
        
        await stream.WriteVarInt(0);
        await stream.WriteVarInt(0);

    }
}