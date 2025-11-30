#if NAS && TEN_BIT_BLOCKS
using System;
using System.Collections;
using System.Collections.Generic;
namespace NotAwesomeSurvival
{
    public class GenericPriorityQueue<TItem, TPriority> : IFixedSizePriorityQueue<TItem, TPriority>
        where TItem : GenericPriorityQueueNode<TPriority>
        where TPriority : IComparable<TPriority>
    {
        public int _numNodes;
        public TItem[] _nodes;
        public long _numNodesEverEnqueued;
        public Comparison<TPriority> _comparer;
        public GenericPriorityQueue(int maxNodes, Comparison<TPriority> comparer)
        {
            if (maxNodes <= 0)
            {
                throw new InvalidOperationException("New queue size cannot be smaller than 1");
            }
            _numNodes = 0;
            _nodes = new TItem[maxNodes + 1];
            _numNodesEverEnqueued = 0;
            _comparer = comparer;
        }
        public int Count
        {
            get
            {
                return _numNodes;
            }
        }
        public int MaxSize
        {
            get
            {
                return _nodes.Length - 1;
            }
        }
        public void Clear()
        {
            Array.Clear(_nodes, 1, _numNodes);
            _numNodes = 0;
        }
        public bool Contains(TItem node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            if (node.Queue != null && !Equals(node.Queue))
            {
                throw new InvalidOperationException("node.Contains was called on a node from another queue.  Please call originalQueue.ResetNode() first");
            }
            if (node.QueueIndex < 0 || node.QueueIndex >= _nodes.Length)
            {
                throw new InvalidOperationException("node.QueueIndex has been corrupted. Did you change it manually?");
            }
            return _nodes[node.QueueIndex] == node;
        }
        public void Enqueue(TItem node, TPriority priority)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            if (_numNodes >= _nodes.Length - 1)
            {
                throw new InvalidOperationException("Queue is full - node cannot be added: " + node);
            }
            if (node.Queue != null && !Equals(node.Queue))
            {
                throw new InvalidOperationException("node.Enqueue was called on a node from another queue.  Please call originalQueue.ResetNode() first");
            }
            if (Contains(node))
            {
                throw new InvalidOperationException("Node is already enqueued: " + node);
            }
            node.Queue = this;
            node.Priority = priority;
            _numNodes++;
            _nodes[_numNodes] = node;
            node.QueueIndex = _numNodes;
            node.InsertionIndex = _numNodesEverEnqueued++;
            CascadeUp(node);
        }
        public void CascadeUp(TItem node)
        {
            int parent;
            if (node.QueueIndex > 1)
            {
                parent = node.QueueIndex >> 1;
                TItem parentNode = _nodes[parent];
                if (HasHigherPriority(parentNode, node))
                {
                    return;
                }
                _nodes[node.QueueIndex] = parentNode;
                parentNode.QueueIndex = node.QueueIndex;
                node.QueueIndex = parent;
            }
            else
            {
                return;
            }
            while (parent > 1)
            {
                parent >>= 1;
                TItem parentNode = _nodes[parent];
                if (HasHigherPriority(parentNode, node))
                {
                    break;
                }
                _nodes[node.QueueIndex] = parentNode;
                parentNode.QueueIndex = node.QueueIndex;
                node.QueueIndex = parent;
            }
            _nodes[node.QueueIndex] = node;
        }
        public void CascadeDown(TItem node)
        {
            int finalQueueIndex = node.QueueIndex,
                childLeftIndex = 2 * finalQueueIndex;
            if (childLeftIndex > _numNodes)
            {
                return;
            }
            int childRightIndex = childLeftIndex + 1;
            TItem childLeft = _nodes[childLeftIndex];
            if (HasHigherPriority(childLeft, node))
            {
                if (childRightIndex > _numNodes)
                {
                    node.QueueIndex = childLeftIndex;
                    childLeft.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = childLeft;
                    _nodes[childLeftIndex] = node;
                    return;
                }
                TItem childRight = _nodes[childRightIndex];
                if (HasHigherPriority(childLeft, childRight))
                {
                    childLeft.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = childLeft;
                    finalQueueIndex = childLeftIndex;
                }
                else
                {
                    childRight.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = childRight;
                    finalQueueIndex = childRightIndex;
                }
            }
            else if (childRightIndex > _numNodes)
            {
                return;
            }
            else
            {
                TItem childRight = _nodes[childRightIndex];
                if (HasHigherPriority(childRight, node))
                {
                    childRight.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = childRight;
                    finalQueueIndex = childRightIndex;
                }
                else
                {
                    return;
                }
            }
            while (true)
            {
                childLeftIndex = 2 * finalQueueIndex;
                if (childLeftIndex > _numNodes)
                {
                    node.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = node;
                    break;
                }
                childRightIndex = childLeftIndex + 1;
                childLeft = _nodes[childLeftIndex];
                if (HasHigherPriority(childLeft, node))
                {
                    if (childRightIndex > _numNodes)
                    {
                        node.QueueIndex = childLeftIndex;
                        childLeft.QueueIndex = finalQueueIndex;
                        _nodes[finalQueueIndex] = childLeft;
                        _nodes[childLeftIndex] = node;
                        break;
                    }
                    TItem childRight = _nodes[childRightIndex];
                    if (HasHigherPriority(childLeft, childRight))
                    {
                        childLeft.QueueIndex = finalQueueIndex;
                        _nodes[finalQueueIndex] = childLeft;
                        finalQueueIndex = childLeftIndex;
                    }
                    else
                    {
                        childRight.QueueIndex = finalQueueIndex;
                        _nodes[finalQueueIndex] = childRight;
                        finalQueueIndex = childRightIndex;
                    }
                }
                else if (childRightIndex > _numNodes)
                {
                    node.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = node;
                    break;
                }
                else
                {
                    TItem childRight = _nodes[childRightIndex];
                    if (HasHigherPriority(childRight, node))
                    {
                        childRight.QueueIndex = finalQueueIndex;
                        _nodes[finalQueueIndex] = childRight;
                        finalQueueIndex = childRightIndex;
                    }
                    else
                    {
                        node.QueueIndex = finalQueueIndex;
                        _nodes[finalQueueIndex] = node;
                        break;
                    }
                }
            }
        }
        public bool HasHigherPriority(TItem higher, TItem lower)
        {
            int cmp = _comparer(higher.Priority, lower.Priority);
            return cmp < 0 || (cmp == 0 && higher.InsertionIndex < lower.InsertionIndex);
        }
        public TItem Dequeue()
        {
            if (_numNodes <= 0)
            {
                throw new InvalidOperationException("Cannot call Dequeue() on an empty queue");
            }
            if (!IsValidQueue())
            {
                throw new InvalidOperationException("Queue has been corrupted (Did you update a node priority manually instead of calling UpdatePriority()?" +
                                                    "Or add the same node to two different queues?)");
            }
            TItem returnMe = _nodes[1];
            if (_numNodes == 1)
            {
                _nodes[1] = null;
                _numNodes = 0;
                return returnMe;
            }
            TItem formerLastNode = _nodes[_numNodes];
            _nodes[1] = formerLastNode;
            formerLastNode.QueueIndex = 1;
            _nodes[_numNodes] = null;
            _numNodes--;
            CascadeDown(formerLastNode);
            return returnMe;
        }
        public void Resize(int maxNodes)
        {
            if (maxNodes <= 0)
            {
                throw new InvalidOperationException("Queue size cannot be smaller than 1");
            }
            if (maxNodes < _numNodes)
            {
                throw new InvalidOperationException("Called Resize(" + maxNodes + "), but current queue contains " + _numNodes + " nodes");
            }
            TItem[] newArray = new TItem[maxNodes + 1];
            int highestIndexToCopy = Math.Min(maxNodes, _numNodes);
            Array.Copy(_nodes, newArray, highestIndexToCopy + 1);
            _nodes = newArray;
        }
        public TItem First
        {
            get
            {
                if (_numNodes <= 0)
                {
                    throw new InvalidOperationException("Cannot call .First on an empty queue");
                }
                return _nodes[1];
            }
        }
        public void UpdatePriority(TItem node, TPriority priority)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            if (node.Queue != null && !Equals(node.Queue))
            {
                throw new InvalidOperationException("node.UpdatePriority was called on a node from another queue");
            }
            if (!Contains(node))
            {
                throw new InvalidOperationException("Cannot call UpdatePriority() on a node which is not enqueued: " + node);
            }
            node.Priority = priority;
            OnNodeUpdated(node);
        }
        public void OnNodeUpdated(TItem node)
        {
            int parentIndex = node.QueueIndex >> 1;
            if (parentIndex > 0 && HasHigherPriority(node, _nodes[parentIndex]))
            {
                CascadeUp(node);
            }
            else
            {
                CascadeDown(node);
            }
        }
        public void Remove(TItem node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            if (node.Queue != null && !Equals(node.Queue))
            {
                throw new InvalidOperationException("node.Remove was called on a node from another queue");
            }
            if (!Contains(node))
            {
                throw new InvalidOperationException("Cannot call Remove() on a node which is not enqueued: " + node);
            }
            if (node.QueueIndex == _numNodes)
            {
                _nodes[_numNodes] = null;
                _numNodes--;
                return;
            }
            TItem formerLastNode = _nodes[_numNodes];
            _nodes[node.QueueIndex] = formerLastNode;
            formerLastNode.QueueIndex = node.QueueIndex;
            _nodes[_numNodes] = null;
            _numNodes--;
            OnNodeUpdated(formerLastNode);
        }
        public void ResetNode(TItem node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            if (node.Queue != null && !Equals(node.Queue))
            {
                throw new InvalidOperationException("node.ResetNode was called on a node from another queue");
            }
            if (Contains(node))
            {
                throw new InvalidOperationException("node.ResetNode was called on a node that is still in the queue");
            }
            node.Queue = null;
            node.QueueIndex = 0;
        }
        public IEnumerator<TItem> GetEnumerator()
        {
            for (int i = 1; i <= _numNodes; i++)
            {
                yield return _nodes[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public bool IsValidQueue()
        {
            for (int i = 1; i < _nodes.Length; i++)
            {
                if (_nodes[i] != null)
                {
                    int childLeftIndex = 2 * i;
                    if (childLeftIndex < _nodes.Length && _nodes[childLeftIndex] != null && HasHigherPriority(_nodes[childLeftIndex], _nodes[i]))
                    {
                        return false;
                    }
                    int childRightIndex = childLeftIndex + 1;
                    if (childRightIndex < _nodes.Length && _nodes[childRightIndex] != null && HasHigherPriority(_nodes[childRightIndex], _nodes[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
#endif