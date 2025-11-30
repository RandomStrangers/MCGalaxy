#if NAS && TEN_BIT_BLOCKS
using System;
using System.Collections.Generic;
namespace NotAwesomeSurvival
{
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
#endif