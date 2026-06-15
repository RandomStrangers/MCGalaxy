using System;
using System.Collections;
using System.Collections.Generic;
namespace MCGalaxy
{
    public class NASSimplePriorityQueue<NASTItem, NASTPriority> : NASIPriorityQueue<NASTItem, NASTPriority>
        where NASTPriority : IComparable<NASTPriority>
    {
        public class NASSimpleNode : NASPriorityQueueNode<NASTPriority>
        {
            public NASTItem Data { get; set; }
            public NASSimpleNode(NASTItem data) => Data = data;
        }
        readonly NASPriorityQueue<NASSimpleNode, NASTPriority> _queue;
        readonly Dictionary<NASTItem, List<NASSimpleNode>> _itemToNodesCache;
        readonly List<NASSimpleNode> _nullNodesCache;
        public NASSimplePriorityQueue() : this(Comparer<NASTPriority>.Default, EqualityComparer<NASTItem>.Default) { }
        public NASSimplePriorityQueue(IComparer<NASTPriority> priorityComparer, IEqualityComparer<NASTItem> itemEquality) : this(priorityComparer.Compare, itemEquality) { }
        public NASSimplePriorityQueue(Comparison<NASTPriority> priorityComparer, IEqualityComparer<NASTItem> itemEquality)
        {
            _queue = new(10, priorityComparer);
            _itemToNodesCache = new(itemEquality);
            _nullNodesCache = new();
        }
        public NASSimpleNode GetExistingNode(NASTItem item) => item == null
                ? _nullNodesCache.Count > 0 ? _nullNodesCache[0] : null
                : !_itemToNodesCache.TryGetValue(item, out List<NASSimpleNode> nodes) ? null : nodes[0];
        public void RemoveFromNodeCache(NASSimpleNode node)
        {
            if (node.Data == null)
            {
                _nullNodesCache.Remove(node);
                return;
            }
            if (!_itemToNodesCache.TryGetValue(node.Data, out List<NASSimpleNode> nodes))
                return;
            nodes.Remove(node);
            if (nodes.Count == 0)
                _itemToNodesCache.Remove(node.Data);
        }
        public int Count => _queue.Count;
        public NASTItem First => _queue.Count <= 0 ? throw new NASQueueException("Cannot call .First on an empty queue") : _queue.First.Data;
        public void Clear()
        {
            _queue.Clear();
            _itemToNodesCache.Clear();
            _nullNodesCache.Clear();
        }
        public bool Contains(NASTItem item) => item == null ? _nullNodesCache.Count > 0 : _itemToNodesCache.ContainsKey(item);
        public NASTItem Dequeue()
        {
            switch (_queue.Count)
            {
                case <= 0:
                    throw new NASQueueException("Cannot call Dequeue() on an empty queue");
                default:
                    {
                        NASSimpleNode node = _queue.Dequeue();
                        RemoveFromNodeCache(node);
                        return node.Data;
                    }
            }
        }
        public NASSimpleNode EnqueueNoLockOrCache(NASTItem item, NASTPriority priority)
        {
            NASSimpleNode node = new(item);
            if (_queue.Count == _queue.MaxSize)
                _queue.Resize(_queue.MaxSize * 2 + 1);
            _queue.Enqueue(node, priority);
            return node;
        }
        public void Enqueue(NASTItem item, NASTPriority priority)
        {
            List<NASSimpleNode> nodes;
            if (item == null)
                nodes = _nullNodesCache;
            else if (!_itemToNodesCache.TryGetValue(item, out nodes))
            {
                nodes = new();
                _itemToNodesCache[item] = nodes;
            }
            NASSimpleNode node = EnqueueNoLockOrCache(item, priority);
            nodes.Add(node);
        }
        public void Remove(NASTItem item)
        {
            NASSimpleNode removeMe;
            List<NASSimpleNode> nodes;
            if (item == null)
            {
                if (_nullNodesCache.Count == 0)
                    throw new NASQueueException("Cannot call Remove() on a node which is not enqueued: " + item);
                removeMe = _nullNodesCache[0];
                nodes = _nullNodesCache;
            }
            else
            {
                if (!_itemToNodesCache.TryGetValue(item, out nodes))
                    throw new NASQueueException("Cannot call Remove() on a node which is not enqueued: " + item);
                removeMe = nodes[0];
                if (nodes.Count == 1)
                    _itemToNodesCache.Remove(item);
            }
            _queue.Remove(removeMe);
            nodes.Remove(removeMe);
        }
        public void UpdatePriority(NASTItem item, NASTPriority priority) => _queue.UpdatePriority(GetExistingNode(item) ?? throw new NASQueueException("Cannot call UpdatePriority() on a node which is not enqueued: " + item), priority);
        public IEnumerator<NASTItem> GetEnumerator()
        {
            List<NASTItem> queueData = new();
            foreach (NASSimpleNode node in _queue)
                queueData.Add(node.Data);
            return queueData.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
