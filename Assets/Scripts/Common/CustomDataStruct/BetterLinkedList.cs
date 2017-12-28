using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;

/// <summary>
/// 说明：无GC版链表：对LinkedList地再次封装，托管所有LinkedListNode，用缓存来避免GC
/// 
/// by wsh @ 2017-06-15
/// </summary>

namespace CustomDataStruct
{
    public sealed class BetterLinkedList<T> : IEnumerable
    {
        LinkedList<BetterLinkedListNodeData<T>> mLinkedList;

        public BetterLinkedList()
        {
            mLinkedList = new LinkedList<BetterLinkedListNodeData<T>>();
        }

        public int Count
        {
            get
            {
                return mLinkedList.Count;
            }
        }

        public BetterLinkedListNode<T> First
        {
            get
            {
                if (mLinkedList.First == null)
                {
                    return null;
                }

                return mLinkedList.First.Value.Holder;
            }
        }

        public BetterLinkedListNode<T> Last
        {
            get
            {
                if (mLinkedList.Last == null)
                {
                    return null;
                }

                return mLinkedList.Last.Value.Holder;
            }
        }

        public void AddAfter(BetterLinkedListNode<T> node, BetterLinkedListNode<T> newNode)
        {
            mLinkedList.AddAfter(node.Node, newNode.Node);
        }

        public BetterLinkedListNode<T> AddAfter(BetterLinkedListNode<T> node, T value)
        {
            BetterLinkedListNode<T> newNode = BetterLinkedListNode<T>.Get();
            newNode.InitInfo(this, value);
            mLinkedList.AddAfter(node.Node, newNode.Node);
            return newNode;
        }

        public void AddBefore(BetterLinkedListNode<T> node, BetterLinkedListNode<T> newNode)
        {
            mLinkedList.AddBefore(node.Node, newNode.Node);
        }

        public BetterLinkedListNode<T> AddBefore(BetterLinkedListNode<T> node, T value)
        {
            BetterLinkedListNode<T> newNode = BetterLinkedListNode<T>.Get();
            newNode.InitInfo(this, value);
            mLinkedList.AddBefore(node.Node, newNode.Node);
            return newNode;
        }

        public BetterLinkedListNode<T> AddFirst(T value)
        {
            BetterLinkedListNode<T> newNode = BetterLinkedListNode<T>.Get();
            newNode.InitInfo(this, value);
            mLinkedList.AddFirst(newNode.Node);
            return newNode;
        }

        public void AddFirst(BetterLinkedListNode<T> node)
        {
            mLinkedList.AddFirst(node.Node);
        }

        public void AddLast(BetterLinkedListNode<T> node)
        {
            mLinkedList.AddLast(node.Node);
        }

        public BetterLinkedListNode<T> AddLast(T value)
        {
            BetterLinkedListNode<T> newNode = BetterLinkedListNode<T>.Get();
            newNode.InitInfo(this, value);
            mLinkedList.AddLast(newNode.Node);
            return newNode;
        }

        public void Clear()
        {
            BetterLinkedListNode<T> node = First;
            while (node != null)
            {
                node.Release();
                node = node.Next;
            }
            mLinkedList.Clear();
        }

        public bool Contains(T value)
        {
            return Find(value) != null;
        }

        public void CopyTo(T[] array, int index)
        {
            BetterLinkedListNode<T> node = First;
            while (node != null && index < Count)
            {
                array[index++] = node.Value;
                node = node.Next;
            }
        }

        public BetterLinkedListNode<T> Find(T value)
        {
            BetterLinkedListNode<T> node = First;
            EqualityComparer<T> comp = EqualityComparer<T>.Default;
            while (node != null)
            {
                if (comp.Equals(node.Value, value))
                {
                    return node;
                }

                node = node.Next;
            }
            
            return null;
        }

        public BetterLinkedListNode<T> FindLast(T value)
        {
            BetterLinkedListNode<T> node = Last;
            EqualityComparer<T> comp = EqualityComparer<T>.Default;
            while (node != null)
            {
                if (comp.Equals(node.Value, value))
                {
                    return node;
                }

                node = node.Previous;
            }

            return null;
        }
        
        IEnumerator IEnumerable.GetEnumerator() { return new NodeEnumerator(First); }
        public NodeEnumerator GetEnumerator() { return new NodeEnumerator(First); }

        public struct NodeEnumerator : IEnumerator
        {
            private readonly BetterLinkedListNode<T> head;
            private BetterLinkedListNode<T> node;
            internal NodeEnumerator(BetterLinkedListNode<T> node)
            {
                this.head = node;
                this.node = null;
            }
            void IEnumerator.Reset() { node = null; }
            public object Current { get { return node; } }
            public bool MoveNext()
            {
                if (node == null) node = head;
                else node = node.Next;
                return node != null;
            }
        }

        public void Remove(BetterLinkedListNode<T> node)
        {
            if (node != null)
            {
                mLinkedList.Remove(node.Node);
                node.Release();
            }
        }

        public bool Remove(T value)
        {
            BetterLinkedListNode<T> node = Find(value);
            Remove(node);
            return node != null;
        }

        public void RemoveFirst()
        {
            Remove(First);
        }

        public void RemoveLast()
        {
            Remove(Last);
        }
    }
}
