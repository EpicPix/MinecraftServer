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
        for (uint i = 0; i < 16; i++)
        {
            await s.WriteUnsignedShortAsync(16 * 16 * 8); // block count

            if (i <= 3) // (16 * 3)   48 up to 63
            {
                await s.WriteByteAsync(8); // palette bits
                await s.WriteVarIntAsync(4); // palette length
                await s.WriteVarIntAsync(9); // element[0], grass block
                await s.WriteVarIntAsync(10); // element[1], dirt
                await s.WriteVarIntAsync(1); // element[2], stone
                await s.WriteVarIntAsync(33); // element[3], bedrock

                await s.WriteVarIntAsync(16 * 16 * 2); // block data length (in longs)
                for (var y = 0; y < 16; y++)
                {
                    var actualY = i * 16 + y;
                    
                    for (var z = 0; z < 16; z++)
                    {
                        for (var x = 0; x < 16; x++)
                        {
                            if (actualY == 63)
                            {
                                await s.WriteByteAsync(0);
                            }else if (actualY > 60)
                            {
                                await s.WriteByteAsync(1);
                            }else if (actualY == 0)
                            {
                                await s.WriteByteAsync(3);
                            } else
                            {
                                await s.WriteByteAsync(2);
                            }
                        }
                    }
                }
            } else
            {
                await s.WriteByteAsync(0); // palette bits
                await s.WriteVarIntAsync(0); // block
                await s.WriteVarIntAsync(0); // block data length
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