using System;
using System.Collections.Generic;
namespace MCGalaxy
{
    public class GenericPriorityQueueNode<TPriority>
    {
        public TPriority Priority { get; set; }
        public int QueueIndex { get; set; }
        public long InsertionIndex { get; set; }
        public object Queue { get; set; }
    }
    public interface IFixedSizePriorityQueue<TItem, in TPriority> : IPriorityQueue<TItem, TPriority>
        where TPriority : IComparable<TPriority>
    {
        void Resize(int maxNodes);
        int MaxSize { get; }
        void ResetNode(TItem node);
    }
    public interface IPriorityQueue<TItem, in TPriority> : IEnumerable<TItem>
        where TPriority : IComparable<TPriority>
    {
        void Enqueue(TItem node, TPriority priority);
        TItem Dequeue();
        void Clear();
        bool Contains(TItem node);
        void Remove(TItem node);
        void UpdatePriority(TItem node, TPriority priority);
        TItem First { get; }
        int Count { get; }
    }
}
