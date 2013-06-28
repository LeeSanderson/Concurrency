using System;
using System.Collections;
using System.Collections.Generic;

namespace Concurrency
{
    /// <summary>
    /// A linked list that implement Read-Copy-Update semantics in order to allow lock
    /// free synchronisation between multiple readers and a single writer.
    /// 
    /// <see cref="http://en.wikipedia.org/wiki/Read-copy-update"/>
    /// </summary>
    public class ReadCopyUpdateList<T> : IEnumerable<T>
    {
        private ReadCopyUpdateListNode<T> head;

        /// <summary>
        /// Get the first node in the list (or null if list is empty).
        /// </summary>
        public ReadCopyUpdateListNode<T> First
        {
            get { return head; }
        }

        /// <summary>
        /// Get the last node in the list (or null if list is empty).
        /// </summary>
        public ReadCopyUpdateListNode<T> Last
        {
            get
            {
                if (head == null)
                    return null;
                
                ReadCopyUpdateListNode<T> local = head;
                while (local.next != null)
                    local = local.next;
                return local;
            }
        } 

        /// <summary>
        /// Adds a value to the end of the list.
        /// Performance of <see cref="AddLast"/> is O(n).
        /// <see cref="AddLast"/> is not thread-safe for writing as we only expect a single writer/updater but is
        /// thead-safe for reading.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The value that was just added</returns>
        public ReadCopyUpdateListNode<T> AddLast(T value)
        {
            ReadCopyUpdateListNode<T> newNode = new ReadCopyUpdateListNode<T>(value);
            ReadCopyUpdateListNode<T> local = Last;
            if (local == null)
                head = newNode;
            else
                local.next = newNode;
            
            return newNode;
        }

        /// <summary>
        /// Add a value to the beginning of the list.
        /// Performance of <see cref="AddFirst"/> is O(1).
        /// <see cref="AddFirst"/> is not thread-safe for writing as we only expect a single writer/updater but is
        /// thead-safe for reading.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The value that was just added</returns>
        public ReadCopyUpdateListNode<T> AddFirst(T value)
        {
            ReadCopyUpdateListNode<T> newNode = new ReadCopyUpdateListNode<T>(value);
            ReadCopyUpdateListNode<T> local = head;
            if (local == null) // List is currently empty
            {
                head = newNode;
            }
            else
            {
                newNode.next = head;
                head = newNode;
            }
            return newNode; // Publish new node.
        }

        /// <summary>
        /// Add a value after a given node.        
        /// Performance of <see cref="AddAfter"/> is O(1).
        /// <see cref="AddAfter"/> is not thread-safe for writing as we only expect a single writer/updater but is
        /// thead-safe for reading.
        /// 
        /// <example>
        /// Given two node [a] and [c] where [c] is after [a]. We want to add value "b" after [a].
        /// We therefore:
        /// 
        /// 1. Create [b] node and point to [c]  
        ///
        ///        [a]----->[c]
        ///            [b]---^
        /// 
        /// 2. Repoint [a] to point to [b]
        /// 
        ///        [a]       [c]
        ///         |-->[b]---^
        ///</example>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ReadCopyUpdateListNode<T> AddAfter(ReadCopyUpdateListNode<T> node, T value)
        {
            if (node == null) throw new ArgumentNullException("node");
            ReadCopyUpdateListNode<T> newNode = new ReadCopyUpdateListNode<T>(value, node.next); // Create "b"
            node.next = newNode; // Repoint [a]
            return newNode;
        }

        /// <summary>
        /// Add a range of values.
        /// </summary>
        /// <param name="range"></param>
        public void AddRange(IEnumerable<T> range)
        {
            if (range == null) throw new ArgumentNullException("range");
            ReadCopyUpdateListNode<T> last = Last;
            foreach (T value in range)
            {
                last = (last == null) ? AddFirst(value) : AddAfter(last, value);
            }
        }

        /// <summary>
        /// Add values
        /// </summary>
        /// <param name="values"></param>
        public void AddValues(params T[] values)
        {
            if (values != null && values.Length > 0)
                AddRange(values);
        }

        /// <summary>
        /// Clear the list
        /// Performance of <see cref="Clear"/> is O(1).
        /// </summary>
        public void Clear()
        {
            // Just reset the "head" to null. Existing reader will continue to work as normal.
            head = null;
        }

        /// <summary>
        /// Check if a value exists in the list
        /// Performance of <see cref="Contains"/> is O(n).
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(T value)
        {
            return Find(value) != null;
        }

        /// <summary>
        /// Find the node with the value
        /// Performance of <see cref="Find"/> is O(n).
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ReadCopyUpdateListNode<T> Find(T value)
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            ReadCopyUpdateListNode<T> local = head;
            while (local != null)
            {
                if (comparer.Equals(local.Value, value))
                    return local;
                local = local.next;
            }
            return null; // Not found
        } 

        /// <summary>
        /// Find the first node that matches the given <see cref="predicate"/> 
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public ReadCopyUpdateListNode<T> FindFirst(Predicate<T> predicate)
        {
            if (predicate == null) throw new ArgumentNullException("predicate");
            ReadCopyUpdateListNode<T> local = head;
            while (local != null)
            {
                if (predicate(local.Value))
                    return local;
                local = local.next;
            }
            return null; // Not found
        }

        /// <summary>
        /// Find and remove a node with the value
        /// Performance of <see cref="Remove(T)"/> is O(n).
        /// </summary>
        /// <param name="value"></param>
        /// <returns>
        /// True if the node was removed, else False.
        /// </returns>
        public bool Remove(T value)
        {
            ReadCopyUpdateListNode<T> node = Find(value);
            if (node != null)
            {
                return Remove(node);
            }
            return false;
        }

        /// <summary>
        /// Remove a node.
        /// Performance of <see cref="Remove(ReadCopyUpdateListNode{T})"/> is O(n) since we have to find the 
        /// previous node in order to rejig the pointers.
        /// 
        /// Note that unlike classic RCU lists we don't have to "wait for readers" to finish before disposing
        /// of unused nodes - instead we simple remove the node and leave it to the garbage collector to clean up.
        /// 
        /// <example>
        /// Given that we have three nodes [a]->[b]->[c] and we want to remove [b] we just need to
        /// repoint [a]->[c], [b]->[c] still exists for current readers but it is no longer in the list and will 
        /// be garbage collected once it is no longer refereneced by and other code.
        /// </example>
        /// </summary>
        /// <param name="node"></param>
        public bool Remove(ReadCopyUpdateListNode<T> node)
        {
            if (node == null) throw new ArgumentNullException("node");
            ReadCopyUpdateListNode<T> local = head;
            if (local == null) 
                return false; // List is empty

            ReadCopyUpdateListNode<T> prev = null;
            if (local == node)
            {
                // we are removing the first node.
                head = node.next;
                return true; // head node removed
            }
            // Else we are removing some interior node
            // Find the node in the list
            while (local != null)
            {
                if (local.next == node)
                {
                    prev = local;
                    break;
                }
                local = local.next;
            }
            if (prev == null)
                return false; // node not in the list

            // All we need to do now is point the previous node to the next node.
            // node will be cleaned up by the garbage collector since it is no longer 
            // referenced by the list.
            prev.next = node.next;
            return true; // interior node removed.
        }

        /// <summary>
        /// Implement <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new ReadCopyUpdateListEnumerator(head);
        }

        /// <summary>
        /// Implement <see cref="IEnumerable"/>
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region Internal enumerator class
        /// <summary>
        /// Implementation of <see cref="IEnumerator{T}"/> for <see cref="ReadCopyUpdateList{T}"/>
        /// </summary>
        private class ReadCopyUpdateListEnumerator : IEnumerator<T>
        {
            private readonly ReadCopyUpdateListNode<T> root;
            private ReadCopyUpdateListNode<T> current;

            public ReadCopyUpdateListEnumerator(ReadCopyUpdateListNode<T> head)
            {
                // Create a root node representing the base of the enumerator pointing to the head of the list.
                root = new ReadCopyUpdateListNode<T>(default(T), head); 
                current = root;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (current == null)
                {
                    throw new InvalidOperationException("Cannot move past last node");
                }

                current = current.Next;
                return (current != null);
            }

            public void Reset()
            {
                current = root;
            }

            public T Current
            {
                get
                {
                    if (current == null)
                        throw new InvalidOperationException("Cannot get current if moved past end of list");
                    if (current == root) 
                        throw new InvalidOperationException("Must call MoveNext() before accessing current");
                    return current.Value;
                }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }
        #endregion
    }

    /// <summary>
    /// Immutable list node.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReadCopyUpdateListNode<T>
    {
        private readonly T value;
        internal volatile ReadCopyUpdateListNode<T> next;

        /// <summary>
        /// Create a node with a <see cref="value"/> 
        /// </summary>
        /// <param name="value"></param>
        public ReadCopyUpdateListNode(T value)
        {
            this.value = value;
        }

        /// <summary>
        /// Create a node with a <see cref="value"/> pointing to another node.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="next"></param>
        public ReadCopyUpdateListNode(T value, ReadCopyUpdateListNode<T> next)
        {
            this.value = value;
            this.next = next;
        }

        /// <summary>
        /// Get the value associated with the node
        /// </summary>
        public T Value
        {
            get { return value; }
        }

        /// <summary>
        /// Get the next node in the list
        /// </summary>
        public ReadCopyUpdateListNode<T> Next
        {
            get { return next; }
        }
    }
}
