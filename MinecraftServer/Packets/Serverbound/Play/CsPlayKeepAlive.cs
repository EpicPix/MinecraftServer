﻿using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Serverbound.Play;

public class CsPlayKeepAlive : Packet<CsPlayKeepAlive, ScPlayKeepAlivePacketData>
{
    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0x0F;
    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        var packet = new ScPlayKeepAlivePacketData();
        packet.KeepAliveId = (long)await stream.ReadULong();
        return packet;
    }

    public override ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        return stream.WriteULong((ulong)Of(data).KeepAliveId);
    }
}