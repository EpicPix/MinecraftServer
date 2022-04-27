using System;

namespace MinecraftServer.SourceGenerators.Events
{
    /// <summary>
    /// Attribute used for source generation
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class EventHandlerAttribute : Attribute
    {
        public EventBuses Bus { get; }
        public string HandledData { get; }
        public int Priority { get; }
        public bool IsAsync { get; }

        public EventHandlerAttribute(EventBuses bus, Type handledData, int priority, bool isAsync = false)
        {
            Bus = bus;
            HandledData = handledData.FullName;
            Priority = priority;
            IsAsync = isAsync;
        }

        internal EventHandlerAttribute(EventBuses bus, string handledData, int priority, bool isAsync = false)
        {
            Bus = bus;
            HandledData = handledData;
            Priority = priority;
            IsAsync = isAsync;
        }
    }
}