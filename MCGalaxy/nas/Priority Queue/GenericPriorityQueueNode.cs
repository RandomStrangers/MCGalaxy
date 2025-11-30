#if NAS && TEN_BIT_BLOCKS
namespace NotAwesomeSurvival
{
    public class GenericPriorityQueueNode<TPriority>
    {
        public TPriority Priority { get; set; }
        public int QueueIndex { get; set; }
        public long InsertionIndex { get; set; }
        public object Queue { get; set; }
    }
}
#endif