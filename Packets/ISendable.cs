using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftServer.Packets
{
    public interface ISendable<T>
    {
        public static abstract ValueTask SendPacket(T data, NetworkConnection network);
    }
}
