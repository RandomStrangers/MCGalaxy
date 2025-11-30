#if NAS && TEN_BIT_BLOCKS
using System;
namespace NotAwesomeSurvival
{
    public interface IFixedSizePriorityQueue<TItem, in TPriority> : IPriorityQueue<TItem, TPriority>
        where TPriority : IComparable<TPriority>
    {
        void Resize(int maxNodes);
        int MaxSize { get; }
        void ResetNode(TItem node);
    }
}
#endif