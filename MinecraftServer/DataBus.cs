// using MinecraftServer.SourceGeneration.Events;

using MinecraftServer.Packets.Serverbound.Handshake;
using MinecraftServer.SourceGeneration.Events;

namespace MinecraftServer;

[EventBus(1)]
public partial class DataBus : EventBus
{
    [EventHandler(1, typeof(CsHandshake), 100)]
    public static ValueTask thing(CsHandshake data, DataBus val)
    {
        return ValueTask.CompletedTask;
    }

    // public static async ValueTask PostEventAsync<T>(T data, DataBus instance) {
    //
    //     switch (data)
    //     {
    //         case CsHandshake c:
    //             if(instance.ShouldContinue()) await MinecraftServer.DataBus.thing(c, instance);
    //             if(instance.ShouldContinue()) await MinecraftServer.DataBus.thing(c, instance);
    //             if(instance.ShouldContinue()) await MinecraftServer.DataBus.thing(c, instance);
    //             if(instance.ShouldContinue()) await MinecraftServer.DataBus.thing(c, instance);
    //             break;
    //         case int d:
    //             return MinecraftServer.DataBus.thing(d, instance);
    //             break;
    //         default:
    //             throw new InvalidOperationException(
    //                 "The specified data cannot be posted. No registered event handler is able to handle that type of data!");
    //     }
    // }
}