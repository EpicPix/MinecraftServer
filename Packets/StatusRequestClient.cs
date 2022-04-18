namespace MinecraftServer.Packets;

public class StatusRequestClient : Packet<PacketData>
{

    PacketType Packet.Type => PacketType.Status;
    PacketSide Packet.Side => PacketSide.Client;
    uint Packet.Id => 0;

    public Task<PacketData> ReadPacket(NetworkConnection stream)
    {
        return Task.FromResult(new PacketData());
    }

    public Task WritePacket(NetworkConnection stream, PacketData data)
    {
        return Task.CompletedTask;
    }
}