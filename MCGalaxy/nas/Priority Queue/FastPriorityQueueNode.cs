#if NAS && !NET_20
namespace Priority_Queue
{
    public class FastPriorityQueueNode
    {
        /// <summary>
        /// The Priority to insert this node at.
        /// Cannot be manually edited - see queue.Enqueue() and queue.UpdatePriority() instead
        /// </summary>
        public float Priority { get; set; }

        /// <summary>
        /// Represents the current position in the queue
        /// </summary>
        public int QueueIndex { get; set; }

#if DEBUG
        /// <summary>
        /// The queue this node is tied to. Used only for debug builds.
        /// </summary>
        public object Queue { get; set; }
#endif
    }
}
#endif