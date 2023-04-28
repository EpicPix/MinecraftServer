using System.Text.Json;
using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Play;

public class ScPlayPlayerInfo : Packet<ScPlayPlayerInfo, ScPlayPlayerInfoPacketData>
{
    
    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0x3A;

    public override ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        throw new NotImplementedException();
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData pData)
    {
        var data = Of(pData);

        await stream.WriteByteAsync((byte) data.Action);
        await stream.WriteVarIntAsync(data.Actions.Count);
        foreach (var action in data.Actions)
        {
            await stream.WriteUuidAsync(action.Uuid);
            if (data.Action == ScPlayPlayerInfoPacketData.UpdateAction.AddPlayer)
            {
                var a = (ScPlayPlayerInfoPacketData.AddPlayerAction) action;

                await stream.WriteStringAsync(a.Username, 16);
                if (a.Profile != null)
                {
                    await stream.WriteVarIntAsync(a.Profile.properties.Count);
                    foreach (var property in a.Profile.properties)
                    {
                        await stream.WriteStringAsync(property.name, 32767);
                        await stream.WriteStringAsync(property.value, 32767);
                        await stream.WriteBoolAsync(property.signature != null); // is the property signed
                        if (property.signature != null)
                        {
                            await stream.WriteStringAsync(property.signature, 32767);
                        }
                    }
                } else
                {
                    await stream.WriteVarIntAsync(0);
                }
            }
            else if (data.Action == ScPlayPlayerInfoPacketData.UpdateAction.UpdateListed)
            {
                await stream.WriteBoolAsync(((ScPlayPlayerInfoPacketData.UpdateListedAction) action).Listed);
            }
            else if (data.Action == ScPlayPlayerInfoPacketData.UpdateAction.UpdateGamemode)
            {
                await stream.WriteVarIntAsync(((ScPlayPlayerInfoPacketData.UpdateGamemodeAction) action).Gamemode);
            }
            else if (data.Action == ScPlayPlayerInfoPacketData.UpdateAction.UpdateLatency)
            {
                await stream.WriteVarIntAsync(((ScPlayPlayerInfoPacketData.UpdateLatencyAction) action).Ping);
            }
            else if (data.Action == ScPlayPlayerInfoPacketData.UpdateAction.UpdateDisplayName)
            {
                var a = (ScPlayPlayerInfoPacketData.UpdateDisplayNameAction) action;
                
                await stream.WriteBoolAsync(a.DisplayName != null);
                if (a.DisplayName != null)
                {
                    await using var mem = new MemoryStream();
                    await JsonSerializer.SerializeAsync(mem, a.DisplayName, SerializationContext.Default.ChatComponent);
                    await stream.WriteBytesLenAsync(mem.ToArray(), ushort.MaxValue);
                }
            }
        }
    }
}