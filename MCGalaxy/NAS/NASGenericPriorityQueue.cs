using System;
using System.Collections;
using System.Collections.Generic;
namespace MCGalaxy
{
    public class NASPriorityQueue<NASTItem, NASTPriority> : INASFixedSizePriorityQueue<NASTItem, NASTPriority>
        where NASTItem : NASPriorityQueueNode<NASTPriority>
        where NASTPriority : IComparable<NASTPriority>
    {
        public int _numNodes;
        public NASTItem[] _nodes;
        public long _numNodesEverEnqueued;
        public Comparison<NASTPriority> _comparer;
        public NASPriorityQueue(int maxNodes, Comparison<NASTPriority> comparer)
        {
            switch (maxNodes)
            {
                case <= 0:
                    throw new NASQueueException("New queue size cannot be smaller than 1");
                default:
                    _numNodes = 0;
                    _nodes = new NASTItem[maxNodes + 1];
                    _numNodesEverEnqueued = 0;
                    _comparer = comparer;
                    break;
            }
        }
        public int Count => _numNodes;
        public int MaxSize => _nodes.Length - 1;
        public void Clear()
        {
            Array.Clear(_nodes, 1, _numNodes);
            _numNodes = 0;
        }
        public bool Contains(NASTItem node) => node == null
                ? throw new ArgumentNullException("node")
                : node.Queue != null && !Equals(node.Queue)
                ? throw new NASQueueException("node.Contains was called on a node from another queue.  Please call originalQueue.ResetNode() first")
                : node.QueueIndex < 0 || node.QueueIndex >= _nodes.Length
                ? throw new NASQueueException("node.QueueIndex has been corrupted. Did you change it manually?")
                : _nodes[node.QueueIndex] == node;
        public void Enqueue(NASTItem node, NASTPriority priority)
        {
            switch (node)
            {
                case null:
                    throw new ArgumentNullException("node");
                default:
                    if (_numNodes >= _nodes.Length - 1)
                        throw new NASQueueException("Queue is full - node cannot be added: " + node);
                    else if (node.Queue != null && !Equals(node.Queue))
                        throw new NASQueueException("node.Enqueue was called on a node from another queue.  Please call originalQueue.ResetNode() first");
                    else if (Contains(node))
                        throw new NASQueueException("Node is already enqueued: " + node);
                    else
                    {
                        node.Queue = this;
                        node.Priority = priority;
                        _numNodes++;
                        _nodes[_numNodes] = node;
                        node.QueueIndex = _numNodes;
                        node.InsertionIndex = _numNodesEverEnqueued++;
                        CascadeUp(node);
                    }
                    break;
            }
        }
        public void CascadeUp(NASTItem node)
        {
            int parent;
            switch (node.QueueIndex)
            {
                case > 1:
                    {
                        parent = node.QueueIndex >> 1;
                        NASTItem parentNode = _nodes[parent];
                        if (HasHigherPriority(parentNode, node))
                            return;
                        _nodes[node.QueueIndex] = parentNode;
                        parentNode.QueueIndex = node.QueueIndex;
                        node.QueueIndex = parent;
                        break;
                    }
                default:
                    return;
            }
            while (parent > 1)
            {
                parent >>= 1;
                NASTItem parentNode = _nodes[parent];
                if (HasHigherPriority(parentNode, node))
                    break;
                _nodes[node.QueueIndex] = parentNode;
                parentNode.QueueIndex = node.QueueIndex;
                node.QueueIndex = parent;
            }
            _nodes[node.QueueIndex] = node;
        }
        public void CascadeDown(NASTItem node)
        {
            int finalQueueIndex = node.QueueIndex,
                childLeftIndex = 2 * finalQueueIndex;
            if (childLeftIndex > _numNodes)
                return;
            int childRightIndex = childLeftIndex + 1;
            NASTItem childLeft = _nodes[childLeftIndex];
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
                NASTItem childRight = _nodes[childRightIndex];
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
                return;
            else
            {
                NASTItem childRight = _nodes[childRightIndex];
                if (HasHigherPriority(childRight, node))
                {
                    childRight.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = childRight;
                    finalQueueIndex = childRightIndex;
                }
                else
                    return;
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
                    NASTItem childRight = _nodes[childRightIndex];
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
                    NASTItem childRight = _nodes[childRightIndex];
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
        public bool HasHigherPriority(NASTItem higher, NASTItem lower) => _comparer(higher.Priority, lower.Priority) < 0 || (_comparer(higher.Priority, lower.Priority) == 0 && higher.InsertionIndex < lower.InsertionIndex);
        public NASTItem Dequeue()
        {
            switch (_numNodes)
            {
                case <= 0:
                    throw new NASQueueException("Cannot call Dequeue() on an empty queue");
                default:
                    if (!IsValidQueue())
                        throw new NASQueueException("Queue has been corrupted (Did you update a node priority manually instead of calling UpdatePriority()?" +
                            "Or add the same node to two different queues?)");
                    else
                    {
                        NASTItem returnMe = _nodes[1];
                        if (_numNodes == 1)
                        {
                            _nodes[1] = null;
                            _numNodes = 0;
                            return returnMe;
                        }
                        NASTItem formerLastNode = _nodes[_numNodes];
                        _nodes[1] = formerLastNode;
                        formerLastNode.QueueIndex = 1;
                        _nodes[_numNodes] = null;
                        _numNodes--;
                        CascadeDown(formerLastNode);
                        return returnMe;
                    }
            }
        }
        public void Resize(int maxNodes)
        {
            switch (maxNodes)
            {
                case <= 0:
                    throw new NASQueueException("Queue size cannot be smaller than 1");
                default:
                    if (maxNodes < _numNodes)
                        throw new NASQueueException("Called Resize(" + maxNodes + "), but current queue contains " + _numNodes + " nodes");
                    else
                    {
                        NASTItem[] newArray = new NASTItem[maxNodes + 1];
                        int highestIndexToCopy = Math.Min(maxNodes, _numNodes);
                        Array.Copy(_nodes, newArray, highestIndexToCopy + 1);
                        _nodes = newArray;
                    }
                    break;
            }
        }
        public NASTItem First => _numNodes <= 0 ? throw new("Cannot call .First on an empty queue") : _nodes[1];
        public void UpdatePriority(NASTItem node, NASTPriority priority)
        {
            switch (node)
            {
                case null:
                    throw new ArgumentNullException("node");
                default:
                    if (node.Queue != null && !Equals(node.Queue))
                        throw new NASQueueException("node.UpdatePriority was called on a node from another queue");
                    else if (!Contains(node))
                        throw new NASQueueException("Cannot call UpdatePriority() on a node which is not enqueued: " + node);
                    else
                    {
                        node.Priority = priority;
                        OnNodeUpdated(node);
                    }
                    break;
            }
        }
        public void OnNodeUpdated(NASTItem node)
        {
            int parentIndex = node.QueueIndex >> 1;
            switch (parentIndex)
            {
                case > 0 when HasHigherPriority(node, _nodes[parentIndex]):
                    CascadeUp(node);
                    break;
                default:
                    CascadeDown(node);
                    break;
            }
        }
        public void Remove(NASTItem node)
        {
            switch (node)
            {
                case null:
                    throw new ArgumentNullException("node");
                default:
                    if (node.Queue != null && !Equals(node.Queue))
                        throw new NASQueueException("node.Remove was called on a node from another queue");
                    else if (!Contains(node))
                        throw new NASQueueException("Cannot call Remove() on a node which is not enqueued: " + node);
                    else
                    {
                        if (node.QueueIndex == _numNodes)
                        {
                            _nodes[_numNodes] = null;
                            _numNodes--;
                            return;
                        }
                        NASTItem formerLastNode = _nodes[_numNodes];
                        _nodes[node.QueueIndex] = formerLastNode;
                        formerLastNode.QueueIndex = node.QueueIndex;
                        _nodes[_numNodes] = null;
                        _numNodes--;
                        OnNodeUpdated(formerLastNode);
                    }
                    break;
            }
        }
        public void ResetNode(NASTItem node)
        {
            switch (node)
            {
                case null:
                    throw new ArgumentNullException("node");
                default:
                    if (node.Queue != null && !Equals(node.Queue))
                        throw new NASQueueException("node.ResetNode was called on a node from another queue");
                    else if (Contains(node))
                        throw new NASQueueException("node.ResetNode was called on a node that is still in the queue");
                    else
                    {
                        node.Queue = null;
                        node.QueueIndex = 0;
                    }
                    break;
            }
        }
        public IEnumerator<NASTItem> GetEnumerator()
        {
            for (int i = 1; i <= _numNodes; i++)
                yield return _nodes[i];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public bool IsValidQueue()
        {
            for (int i = 1; i < _nodes.Length; i++)
                if (_nodes[i] != null)
                {
                    int childLeftIndex = 2 * i;
                    if (childLeftIndex < _nodes.Length && _nodes[childLeftIndex] != null && HasHigherPriority(_nodes[childLeftIndex], _nodes[i]))
                        return false;
                    int childRightIndex = childLeftIndex + 1;
                    if (childRightIndex < _nodes.Length && _nodes[childRightIndex] != null && HasHigherPriority(_nodes[childRightIndex], _nodes[i]))
                        return false;
                }
            return true;
        }
    }
}