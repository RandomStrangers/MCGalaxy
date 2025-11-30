#if NAS && TEN_BIT_BLOCKS
using System;
using System.Collections;
using System.Collections.Generic;
namespace NotAwesomeSurvival
{
    public class SimplePriorityQueue<TItem, TPriority> : IPriorityQueue<TItem, TPriority>
        where TPriority : IComparable<TPriority>
    {
        public class SimpleNode : GenericPriorityQueueNode<TPriority>
        {
            public TItem Data { get; set; }
            public SimpleNode(TItem data)
            {
                Data = data;
            }
        }
        private readonly GenericPriorityQueue<SimpleNode, TPriority> _queue;
        private readonly Dictionary<TItem, IList<SimpleNode>> _itemToNodesCache;
        private readonly IList<SimpleNode> _nullNodesCache;
        public SimplePriorityQueue() : this(Comparer<TPriority>.Default, EqualityComparer<TItem>.Default) { }
        public SimplePriorityQueue(IComparer<TPriority> priorityComparer, IEqualityComparer<TItem> itemEquality) : this(priorityComparer.Compare, itemEquality) { }
        public SimplePriorityQueue(Comparison<TPriority> priorityComparer, IEqualityComparer<TItem> itemEquality)
        {
            _queue = new(10, priorityComparer);
            _itemToNodesCache = new(itemEquality);
            _nullNodesCache = new List<SimpleNode>();
        }
        public SimpleNode GetExistingNode(TItem item)
        {
            if (item == null)
            {
                return _nullNodesCache.Count > 0 ? _nullNodesCache[0] : null;
            }
            if (!_itemToNodesCache.TryGetValue(item, out IList<SimpleNode> nodes))
            {
                return null;
            }
            return nodes[0];
        }
        public void RemoveFromNodeCache(SimpleNode node)
        {
            if (node.Data == null)
            {
                _nullNodesCache.Remove(node);
                return;
            }
            if (!_itemToNodesCache.TryGetValue(node.Data, out IList<SimpleNode> nodes))
            {
                return;
            }
            nodes.Remove(node);
            if (nodes.Count == 0)
            {
                _itemToNodesCache.Remove(node.Data);
            }
        }
        public int Count
        {
            get
            {
                return _queue.Count;
            }
        }
        public TItem First
        {
            get
            {
                if (_queue.Count <= 0)
                {
                    throw new InvalidOperationException("Cannot call .First on an empty queue");
                }
                return _queue.First.Data;
            }
        }
        public void Clear()
        {
            _queue.Clear();
            _itemToNodesCache.Clear();
            _nullNodesCache.Clear();
        }
        public bool Contains(TItem item)
        {
            return item == null ? _nullNodesCache.Count > 0 : _itemToNodesCache.ContainsKey(item);
        }
        public TItem Dequeue()
        {
            if (_queue.Count <= 0)
            {
                throw new InvalidOperationException("Cannot call Dequeue() on an empty queue");
            }
            SimpleNode node = _queue.Dequeue();
            RemoveFromNodeCache(node);
            return node.Data;
        }
        public SimpleNode EnqueueNoLockOrCache(TItem item, TPriority priority)
        {
            SimpleNode node = new(item);
            if (_queue.Count == _queue.MaxSize)
            {
                _queue.Resize(_queue.MaxSize * 2 + 1);
            }
            _queue.Enqueue(node, priority);
            return node;
        }
        public void Enqueue(TItem item, TPriority priority)
        {
            IList<SimpleNode> nodes;
            if (item == null)
            {
                nodes = _nullNodesCache;
            }
            else if (!_itemToNodesCache.TryGetValue(item, out nodes))
            {
                nodes = new List<SimpleNode>();
                _itemToNodesCache[item] = nodes;
            }
            SimpleNode node = EnqueueNoLockOrCache(item, priority);
            nodes.Add(node);
        }
        public void Remove(TItem item)
        {
            SimpleNode removeMe;
            IList<SimpleNode> nodes;
            if (item == null)
            {
                if (_nullNodesCache.Count == 0)
                {
                    throw new InvalidOperationException("Cannot call Remove() on a node which is not enqueued: " + item);
                }
                removeMe = _nullNodesCache[0];
                nodes = _nullNodesCache;
            }
            else
            {
                if (!_itemToNodesCache.TryGetValue(item, out nodes))
                {
                    throw new InvalidOperationException("Cannot call Remove() on a node which is not enqueued: " + item);
                }
                removeMe = nodes[0];
                if (nodes.Count == 1)
                {
                    _itemToNodesCache.Remove(item);
                }
            }
            _queue.Remove(removeMe);
            nodes.Remove(removeMe);
        }
        public void UpdatePriority(TItem item, TPriority priority)
        {
            SimpleNode updateMe = GetExistingNode(item) ?? throw new InvalidOperationException("Cannot call UpdatePriority() on a node which is not enqueued: " + item);
            _queue.UpdatePriority(updateMe, priority);
        }
        public IEnumerator<TItem> GetEnumerator()
        {
            List<TItem> queueData = new();
            foreach (SimpleNode node in _queue)
            {
                queueData.Add(node.Data);
            }
            return queueData.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
#endif