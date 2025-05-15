#if NAS && !NET_20
namespace Priority_Queue
{
    public class StablePriorityQueueNode : FastPriorityQueueNode
    {
        /// <summary>
        /// Represents the order the node was inserted in
        /// </summary>
        public long InsertionIndex { get; set; }
    }
}
#endif