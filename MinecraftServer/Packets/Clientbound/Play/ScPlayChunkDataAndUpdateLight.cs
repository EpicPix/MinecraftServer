using MinecraftServer.Nbt;
using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Play;

public class ScPlayChunkDataAndUpdateLight : Packet<ScPlayChunkDataAndUpdateLight, PacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0x22;

    public override ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        throw new NotImplementedException();
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        await stream.WriteInt(0); // Chunk X
        await stream.WriteInt(0); // Chunk Z

        var arr = new NbtTagLongArray();
        for (uint i = 0; i < 32; i++) arr.Add(32);
        
        await new NbtTagRoot()
            .Set("MOTION_BLOCKING", arr)
            .Write(stream);

        await stream.WriteVarInt(128);
        for (uint i = 0; i < 16; i++)
        {
            await stream.WriteUShort(16 * 16 * 16); // block count

            await stream.WriteUByte(0);
            await stream.WriteVarInt(0x12);
            await stream.WriteVarInt(0);

            await stream.WriteUByte(0);
            await stream.WriteVarInt(1);
            await stream.WriteVarInt(0);
        }
        
        await stream.WriteVarInt(0);

        await stream.WriteBool(false);
        await stream.WriteVarInt(0);
        await stream.WriteVarInt(0);
        await stream.WriteVarInt(0);
        await stream.WriteVarInt(0);
        
        await stream.WriteVarInt(0);
        await stream.WriteVarInt(0);

    }
}