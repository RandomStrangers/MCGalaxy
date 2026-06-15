using System;
using System.Collections.Generic;
namespace MCGalaxy
{
    public class NASPriorityQueueNode<NASTPriority>
    {
        public NASTPriority Priority { get; set; }
        public int QueueIndex { get; set; }
        public long InsertionIndex { get; set; }
        public object Queue { get; set; }
    }
    public interface NASIFixedSizePriorityQueue<NASTItem, in NASTPriority> : NASIPriorityQueue<NASTItem, NASTPriority>
        where NASTPriority : IComparable<NASTPriority>
    {
        void Resize(int maxNodes);
        int MaxSize { get; }
        void ResetNode(NASTItem node);
    }
    public interface NASIPriorityQueue<NASTItem, in NASTPriority> : IEnumerable<NASTItem>
        where NASTPriority : IComparable<NASTPriority>
    {
        void Enqueue(NASTItem node, NASTPriority priority);
        NASTItem Dequeue();
        void Clear();
        bool Contains(NASTItem node);
        void Remove(NASTItem node);
        void UpdatePriority(NASTItem node, NASTPriority priority);
        NASTItem First { get; }
        int Count { get; }
    }
}
