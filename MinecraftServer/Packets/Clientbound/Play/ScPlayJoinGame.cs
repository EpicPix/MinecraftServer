using MinecraftServer.Nbt;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Play;

public class ScPlayJoinGame : Packet<ScPlayJoinGame, ScPlayJoinGamePacketData>
{

    public override PacketType Type => PacketType.Login;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0x26;

    public override PacketData ReadPacket(NetworkConnection stream)
    {
        throw new NotImplementedException();
    }

    public override void WritePacket(NetworkConnection stream, PacketData data)
    {
        stream.WriteInt(0);
        stream.WriteBool(false);
        stream.WriteUByte(1);
        stream.WriteUByte(1);
        stream.WriteVarInt(1);
        stream.WriteString("minecraft:overworld", ushort.MaxValue); // identifier

        new NbtTagRoot()
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
                                        .SetString("precipitation", "none")
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
                                        .SetString("precipitation", "none")
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
                    )
            )
            .Write(stream);
        
        new NbtTagRoot()
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
            .Write(stream);
        
        stream.WriteString("minecraft:overworld", ushort.MaxValue); // identifier
        stream.WriteULong(0);
        stream.WriteVarInt(80);
        stream.WriteVarInt(16);
        stream.WriteVarInt(16);
        stream.WriteBool(false);
        stream.WriteBool(true);
        stream.WriteBool(false);
        stream.WriteBool(false);
    }
}