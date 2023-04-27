using MinecraftServer.Nbt;
using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Play;

public class ScPlayJoinGame : Packet<ScPlayJoinGame, ScPlayJoinGamePacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0x28;

    public override ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        throw new NotImplementedException();
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        var packet = Of(data);
        
        await stream.WriteIntAsync(packet.EntityId);
        await stream.WriteBoolAsync(false);
        await stream.WriteByteAsync(1);
        await stream.WriteByteAsync(0xff);
        await stream.WriteVarIntAsync(1);
        await stream.WriteStringAsync("minecraft:overworld", ushort.MaxValue); // identifier

        await new NbtTagRoot()
            .Set("minecraft:dimension_type", 
                new NbtTagCompound()
                    .SetString("type", "minecraft:dimension_type")
                    .Set("value", 
                        new NbtTagList<NbtTagCompound>()
                            .Add(
                                new NbtTagCompound()
                                    .SetString("name", "minecraft:overworld")
                                    .SetInteger("id", 0)
                                    .Set("element",
                                    new NbtTagCompound()
                                            .SetInteger("monster_spawn_block_light_limit", 0)
                                            .SetInteger("monster_spawn_light_level", 0)
                                            .SetByte("piglin_safe", 0)
                                            .SetByte("natural", 1)
                                            .SetFloat("ambient_light", 1)
                                            .SetString("infiniburn", "#minecraft:infiniburn_overworld")
                                            .SetByte("respawn_anchor_works", 0)
                                            .SetByte("has_skylight", 1)
                                            .SetByte("bed_works", 1)
                                            .SetString("effects", "minecraft:overworld")
                                            .SetByte("has_raids", 0)
                                            .SetInteger("min_y", 0)
                                            .SetInteger("height", 256)
                                            .SetInteger("logical_height", 256)
                                            .SetDouble("coordinate_scale", 1)
                                            .SetByte("ultrawarm", 0)
                                            .SetByte("has_ceiling", 0)
                                    )
                            )
                    )
            )
            .Set("minecraft:worldgen/biome", 
                new NbtTagCompound()
                    .SetString("type", "minecraft:worldgen/biome")
                    .Set("value", 
                        new NbtTagList<NbtTagCompound>()
                            .Add(new NbtTagCompound()
                                .SetString("name", "minecraft:the_void")
                                .SetInteger("id", 0)
                                .Set("element", 
                                    new NbtTagCompound()
                                        .SetInteger("has_precipitation", 0)
                                        .SetFloat("temperature", 0)
                                        .SetFloat("downfall", 0.5f)
                                        .SetString("category", "none")
                                        .Set("effects",
                                            new NbtTagCompound()
                                                .SetInteger("sky_color", 0)
                                                .SetInteger("water_fog_color", 0)
                                                .SetInteger("fog_color", 0)
                                                .SetInteger("water_color", 0)
                                        )
                                )
                            )
                            .Add(new NbtTagCompound()
                                .SetString("name", "minecraft:plains")
                                .SetInteger("id", 1)
                                .Set("element", 
                                    new NbtTagCompound()
                                        .SetInteger("has_precipitation", 0)
                                        .SetFloat("temperature", 0)
                                        .SetFloat("downfall", 0.5f)
                                        .SetString("category", "none")
                                        .Set("effects",
                                            new NbtTagCompound()
                                                .SetInteger("sky_color", 0xff0000)
                                                .SetInteger("water_fog_color", 0x00ff00)
                                                .SetInteger("fog_color", 0x0000ff)
                                                .SetInteger("water_color", 0xff00ff)
                                        )
                                )
                            )
                    )
            )
            .Write(stream);
        
        await stream.WriteStringAsync("minecraft:overworld", ushort.MaxValue); // identifier
        await stream.WriteStringAsync("minecraft:overworld", ushort.MaxValue); // identifier
        await stream.WriteUnsignedLongAsync(0);
        await stream.WriteVarIntAsync(80);
        await stream.WriteVarIntAsync(16);
        await stream.WriteVarIntAsync(16);
        await stream.WriteBoolAsync(false);
        await stream.WriteBoolAsync(true);
        await stream.WriteBoolAsync(false);
        await stream.WriteBoolAsync(false);
        await stream.WriteBoolAsync(false);
    }
}