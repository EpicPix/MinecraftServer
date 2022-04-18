namespace MinecraftServer.Packets;

public class StatusPongClient : Packet<StatusPingPacketData>
{

    PacketType Packet.Type => PacketType.Status;
    PacketSide Packet.Side => PacketSide.Server;
    uint Packet.Id => 1;

    public async Task<StatusPingPacketData> ReadPacket(NetworkConnection stream)
    {
        return new StatusPingPacketData(await stream.ReadULong());
    }

    public async Task WritePacket(NetworkConnection stream, StatusPingPacketData data)
    {
        await stream.WriteULong(data.Payload);
    }
}