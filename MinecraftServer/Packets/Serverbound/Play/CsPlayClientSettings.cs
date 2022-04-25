using MinecraftServer.Networking;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound.Play;

public class CsPlayClientSettings : Packet<CsPlayClientSettings, CsPlayClientSettingsPacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0x05;

    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        var locale = await stream.ReadStringAsync(16);
        var viewDistance = await stream.ReadByteAsync();
        var chatMode = (CsPlayClientSettingsPacketData.ChatModeEnum) await stream.ReadByteAsync();
        var chatColors = await stream.ReadBoolAsync();
        var displayedSkinParts = (CsPlayClientSettingsPacketData.SkinPartsFlags) await stream.ReadByteAsync();
        var mainHand = (CsPlayClientSettingsPacketData.MainHandEnum) await stream.ReadByteAsync();
        var enableTextFiltering = await stream.ReadBoolAsync();
        var allowServerListings = await stream.ReadBoolAsync();
        
        return new CsPlayClientSettingsPacketData(locale, viewDistance, chatMode, chatColors, displayedSkinParts, mainHand, enableTextFiltering, allowServerListings);
    }

    public override ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        throw new NotImplementedException();
    }
}