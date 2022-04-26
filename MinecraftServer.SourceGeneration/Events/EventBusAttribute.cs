using System;

namespace MinecraftServer.SourceGeneration.Events
{
    /// <summary>
    /// Attribute used to define an Event Bus
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public class EventBusAttribute : Attribute
    {
        public int BusId { get; }

        public EventBusAttribute(int busId)
        {
            BusId = busId;
        }
    }
}