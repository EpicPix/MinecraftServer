using System;

namespace MinecraftServer.SourceGenerators.Events
{
    /// <summary>
    /// Attribute used to define an Event Bus
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public class EventBusAttribute : Attribute
    {
        public EventBuses Bus { get; }

        public EventBusAttribute(EventBuses bus)
        {
            Bus = bus;
        }
    }
}