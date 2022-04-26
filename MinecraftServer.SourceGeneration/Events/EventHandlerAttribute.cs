using System;

namespace MinecraftServer.SourceGeneration.Events
{
    /// <summary>
    /// Attribute used for source generation
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class EventHandlerAttribute : Attribute
    {
        public int BusId { get; }
        public string HandledData { get; }
        public int Priority { get; }

        public EventHandlerAttribute(int busId, Type handledData, int priority)
        {
            BusId = busId;
            HandledData = handledData.FullName;
            Priority = priority;
        }

        internal EventHandlerAttribute(int busId, string handledData, int priority)
        {
            BusId = busId;
            HandledData = handledData;
            Priority = priority;
        }
    }
}