#if NAS && TEN_BIT_BLOCKS
using System;
using System.Collections;
using System.Collections.Generic;
namespace Priority_Queue
{
    /// <summary>
    /// A simplified priority queue implementation.  Is stable, auto-resizes, and thread-safe, at the cost of being slightly slower than
    /// FastPriorityQueue
    /// Methods tagged as O(1) or O(log n) are assuming there are no duplicates.  Duplicates may increase the algorithmic complexity.
    /// </summary>
    /// <typeparam name="TItem">The type to enqueue</typeparam>
    /// <typeparam name="TPriority">The priority-type to use for nodes.  Must extend IComparable&lt;TPriority&gt;</typeparam>
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
        #region Constructors
        /// <summary>
        /// Instantiate a new Priority Queue
        /// </summary>
        public SimplePriorityQueue() : this(Comparer<TPriority>.Default, EqualityComparer<TItem>.Default) { }
        /// <summary>
        /// Instantiate a new Priority Queue
        /// </summary>
        /// <param name="priorityComparer">The comparer used to compare TPriority values.  Defaults to Comparer&lt;TPriority&gt;.default</param>
        /// <param name="itemEquality">The equality comparison function to use to compare TItem values</param>
        public SimplePriorityQueue(IComparer<TPriority> priorityComparer, IEqualityComparer<TItem> itemEquality) : this(priorityComparer.Compare, itemEquality) { }
        /// <summary>
        /// Instantiate a new Priority Queue
        /// </summary>
        /// <param name="priorityComparer">The comparison function to use to compare TPriority values</param>
        /// <param name="itemEquality">The equality comparison function to use to compare TItem values</param>
        public SimplePriorityQueue(Comparison<TPriority> priorityComparer, IEqualityComparer<TItem> itemEquality)
        {
            _queue = new GenericPriorityQueue<SimpleNode, TPriority>(10, priorityComparer);
            _itemToNodesCache = new Dictionary<TItem, IList<SimpleNode>>(itemEquality);
            _nullNodesCache = new List<SimpleNode>();
        }
        #endregion
        /// <summary>
        /// Given an item of type T, returns the existing SimpleNode in the queue
        /// </summary>
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
        /// <summary>
        /// Removes an item to the Node-cache to allow for many methods to be O(1) or O(log n) (assuming no duplicates)
        /// </summary>
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
        /// <summary>
        /// Returns the number of nodes in the queue.
        /// O(1)
        /// </summary>
        public int Count
        {
            get
            {
                lock (_queue)
                {
                    return _queue.Count;
                }
            }
        }
        /// <summary>
        /// Returns the head of the queue, without removing it (use Dequeue() for that).
        /// Throws an exception when the queue is empty.
        /// O(1)
        /// </summary>
        public TItem First
        {
            get
            {
                lock (_queue)
                {
                    if (_queue.Count <= 0)
                    {
                        throw new InvalidOperationException("Cannot call .First on an empty queue");
                    }
                    return _queue.First.Data;
                }
            }
        }
        /// <summary>
        /// Removes every node from the queue.
        /// O(n)
        /// </summary>
        public void Clear()
        {
            lock (_queue)
            {
                _queue.Clear();
                _itemToNodesCache.Clear();
                _nullNodesCache.Clear();
            }
        }
        /// <summary>
        /// Returns whether the given item is in the queue.
        /// O(1)
        /// </summary>
        public bool Contains(TItem item)
        {
            lock (_queue)
            {
                return item == null ? _nullNodesCache.Count > 0 : _itemToNodesCache.ContainsKey(item);
            }
        }
        /// <summary>
        /// Removes the head of the queue (node with minimum priority; ties are broken by order of insertion), and returns it.
        /// If queue is empty, throws an exception
        /// O(log n)
        /// </summary>
        public TItem Dequeue()
        {
            lock (_queue)
            {
                if (_queue.Count <= 0)
                {
                    throw new InvalidOperationException("Cannot call Dequeue() on an empty queue");
                }
                SimpleNode node = _queue.Dequeue();
                RemoveFromNodeCache(node);
                return node.Data;
            }
        }
        /// <summary>
        /// Enqueue the item with the given priority, without calling lock(_queue) or AddToNodeCache(node)
        /// </summary>
        /// <param name="item"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Enqueue a node to the priority queue.  Lower values are placed in front. Ties are broken by first-in-first-out.
        /// This queue automatically resizes itself, so there's no concern of the queue becoming 'full'.
        /// Duplicates and null-values are allowed.
        /// O(log n)
        /// </summary>
        public void Enqueue(TItem item, TPriority priority)
        {
            lock (_queue)
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
        }
        /// <summary>
        /// Removes an item from the queue.  The item does not need to be the head of the queue.  
        /// If the item is not in the queue, an exception is thrown.  If unsure, check Contains() first.
        /// If multiple copies of the item are enqueued, only the first one is removed. 
        /// O(log n)
        /// </summary>
        public void Remove(TItem item)
        {
            lock (_queue)
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
        }
        /// <summary>
        /// Call this method to change the priority of an item.
        /// Calling this method on a item not in the queue will throw an exception.
        /// If the item is enqueued multiple times, only the first one will be updated.
        /// (If your requirements are complex enough that you need to enqueue the same item multiple times <i>and</i> be able
        /// to update all of them, please wrap your items in a wrapper class so they can be distinguished).
        /// O(log n)
        /// </summary>
        public void UpdatePriority(TItem item, TPriority priority)
        {
            lock (_queue)
            {
                SimpleNode updateMe = GetExistingNode(item) ?? throw new InvalidOperationException("Cannot call UpdatePriority() on a node which is not enqueued: " + item);
                _queue.UpdatePriority(updateMe, priority);
            }
        }
        public IEnumerator<TItem> GetEnumerator()
        {
            List<TItem> queueData = new();
            lock (_queue)
            {
                //Copy to a separate list because we don't want to 'yield return' inside a lock
                foreach (SimpleNode node in _queue)
                {
                    queueData.Add(node.Data);
                }
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