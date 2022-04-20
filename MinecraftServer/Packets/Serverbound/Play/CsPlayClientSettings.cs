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
        var locale = await stream.ReadString(16);
        var viewDistance = await stream.ReadUByte();
        var chatMode = (CsPlayClientSettingsPacketData.ChatModeEnum) await stream.ReadUByte();
        var chatColors = await stream.ReadBool();
        var displayedSkinParts = (CsPlayClientSettingsPacketData.SkinPartsFlags) await stream.ReadUByte();
        var mainHand = (CsPlayClientSettingsPacketData.MainHandEnum) await stream.ReadUByte();
        var enableTextFiltering = await stream.ReadBool();
        var allowServerListings = await stream.ReadBool();
        
        return new CsPlayClientSettingsPacketData(locale, viewDistance, chatMode, chatColors, displayedSkinParts, mainHand, enableTextFiltering, allowServerListings);
    }

    public override ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        throw new NotImplementedException();
    }
}