namespace MinecraftServer.Packets;

public class StatusPingClient : Packet<StatusPingPacketData>
{

    PacketType Packet.Type => PacketType.Status;
    PacketSide Packet.Side => PacketSide.Client;
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

public class StatusPingPacketData : PacketData
{
    public ulong Payload { get; }

    public StatusPingPacketData(ulong payload)
    {
        Payload = payload;
    }
}