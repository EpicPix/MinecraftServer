using System.Text.Json;
using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Play;

public class ScPlayPlayerInfo : Packet<ScPlayPlayerInfo, ScPlayPlayerInfoPacketData>
{
    
    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0x36;

    public override ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        throw new NotImplementedException();
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData pData)
    {
        var data = Of(pData);

        await stream.WriteVarInt((int) data.Action);
        await stream.WriteVarInt(data.Actions.Count);
        foreach (var action in data.Actions)
        {
            await stream.WriteUUID(action.Uuid);
            if (data.Action == ScPlayPlayerInfoPacketData.UpdateAction.AddPlayer)
            {
                var a = (ScPlayPlayerInfoPacketData.AddPlayerAction) action;

                await stream.WriteString(a.Username, 16);
                if (a.Profile != null)
                {
                    await stream.WriteVarInt(a.Profile.properties.Count);
                    foreach (var property in a.Profile.properties)
                    {
                        await stream.WriteString(property.name, 32767);
                        await stream.WriteString(property.value, 32767);
                        await stream.WriteBool(property.signature != null); // is the property signed
                        if (property.signature != null)
                        {
                            await stream.WriteString(property.signature, 32767);
                        }
                    }
                } else
                {
                    await stream.WriteVarInt(0);
                }

                await stream.WriteVarInt(a.Gamemode);
                await stream.WriteVarInt(a.Ping);
                await stream.WriteBool(a.DisplayName != null);
                if (a.DisplayName != null)
                {
                    await using var mem = new MemoryStream();
                    await JsonSerializer.SerializeAsync(mem, a.DisplayName, SerializationContext.Default.ChatComponent);
                    await stream.WriteBytesLen(mem.ToArray(), ushort.MaxValue);
                }
            }
            else if (data.Action == ScPlayPlayerInfoPacketData.UpdateAction.UpdateGamemode)
            {
                await stream.WriteVarInt(((ScPlayPlayerInfoPacketData.UpdateGamemodeAction) action).Gamemode);
            }
            else if (data.Action == ScPlayPlayerInfoPacketData.UpdateAction.UpdateLatency)
            {
                await stream.WriteVarInt(((ScPlayPlayerInfoPacketData.UpdateLatencyAction) action).Ping);
            }
            else if (data.Action == ScPlayPlayerInfoPacketData.UpdateAction.UpdateDisplayName)
            {
                var a = (ScPlayPlayerInfoPacketData.UpdateDisplayNameAction) action;
                
                await stream.WriteBool(a.DisplayName != null);
                if (a.DisplayName != null)
                {
                    await using var mem = new MemoryStream();
                    await JsonSerializer.SerializeAsync(mem, a.DisplayName, SerializationContext.Default.ChatComponent);
                    await stream.WriteBytesLen(mem.ToArray(), ushort.MaxValue);
                }
            }
        }
    }
}