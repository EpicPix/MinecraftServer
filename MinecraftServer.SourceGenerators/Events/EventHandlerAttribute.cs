using System;

namespace MinecraftServer.SourceGenerators.Events
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
        public bool IsAsync { get; }

        public EventHandlerAttribute(int busId, Type handledData, int priority, bool isAsync = false)
        {
            BusId = busId;
            HandledData = handledData.FullName;
            Priority = priority;
            IsAsync = isAsync;
        }

        internal EventHandlerAttribute(int busId, string handledData, int priority, bool isAsync = false)
        {
            BusId = busId;
            HandledData = handledData;
            Priority = priority;
            IsAsync = isAsync;
        }
    }
}