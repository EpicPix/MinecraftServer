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
        
        await stream.WriteIntAsync(data.ChunkX);
        await stream.WriteIntAsync(data.ChunkZ);

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
        for (var i = 0; i < data.Chunk.SectionsLength; i++)
        {
            var section = data.Chunk[i];
            await s.WriteUnsignedShortAsync(16 * 16 * 16); // block count, a lot and i don't feel like adding a counter for checking amount of blocks in a ChunkSection

            var palette = section.GetPalette();
            if (palette.Length == 0)
            {
                await s.WriteByteAsync(0);
                await s.WriteByteAsync(0);
                await s.WriteByteAsync(0);
            } else
            {
                await s.WriteByteAsync(8);
                var paletteLength = 0;
                foreach (var id in palette)
                {
                    paletteLength += Utils.GetVarIntLength(id);
                }
                await s.WriteVarIntAsync(paletteLength);
                foreach (var id in palette)
                {
                    await s.WriteVarIntAsync(id);
                }
                await s.WriteVarIntAsync(16 * 16 * 2); // block data length (in longs)
                for (byte y = 0; y <= 15; y++)
                {
                    for (byte z = 0; z <= 15; z++)
                    {
                        for (byte x = 0; x <= 15; x++)
                        {
                            await s.WriteByteAsync(section.GetBlockIdToPalette(section.GetBlockId(x, y, z)));
                        }
                    }
                }
            }

            await s.WriteByteAsync(0);
            await s.WriteVarIntAsync(1);
            await s.WriteVarIntAsync(0);
        }
        await stream.WriteVarIntAsync((int) ms.Length);
        await stream.WriteBytesAsync(ms.ToArray());
        
        await stream.WriteVarIntAsync(0);

        await stream.WriteBoolAsync(true);
        await stream.WriteVarIntAsync(0);
        await stream.WriteVarIntAsync(0);
        await stream.WriteVarIntAsync(0);
        await stream.WriteVarIntAsync(0);
        
        await stream.WriteVarIntAsync(0);
        await stream.WriteVarIntAsync(0);

    }
}