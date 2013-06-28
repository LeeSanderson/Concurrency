using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace Concurrency.Tests
{
    /// <summary>
    /// Test <see cref="ReadCopyUpdateList{T}"/>
    /// </summary>
    [TestFixture]
    public class TestReadCopyUpdateList : TestBase
    {
        /// <summary>
        /// Test getting the <see cref="ReadCopyUpdateList{T}.First"/> property
        /// </summary>
        [Test]
        public void TestFirst()
        {
            // Arrange
            ReadCopyUpdateList<int> list = new ReadCopyUpdateList<int>();

            // Assume
            Assert.IsNull(list.First);

            // Act
            list.AddFirst(1);

            // Assert
            Assert.IsNotNull(list.First);
            Assert.AreEqual(1, list.First.Value);
        }

        /// <summary>
        /// Test getting the <see cref="ReadCopyUpdateList{T}.Last"/> property
        /// </summary>
        [Test]
        public void TestLast()
        {
            // Arrange
            ReadCopyUpdateList<int> list = new ReadCopyUpdateList<int>();

            // Assume
            Assert.IsNull(list.Last);

            // Act
            ReadCopyUpdateListNode<int> last = list.AddFirst(1); // Add in reverse order
            list.AddFirst(0);

            // Assert
            Assert.IsNotNull(list.Last);
            Assert.AreEqual(last, list.Last);
            Assert.AreEqual(1, list.Last.Value);
        }

        /// <summary>
        /// Test the <see cref="ReadCopyUpdateList{T}.AddLast"/> method
        /// </summary>
        [Test]
        public void TestAddLast()
        {
            // Arrange
            ReadCopyUpdateList<int> list = new ReadCopyUpdateList<int>();

            // Act
            ReadCopyUpdateListNode<int> first = list.AddLast(0);
            ReadCopyUpdateListNode<int> node = list.AddLast(1);

            // Assert
            Assert.IsNotNull(node);
            Assert.AreEqual(1, node.Value);
            Assert.IsNotNull(first);
            Assert.AreEqual(node, first.Next);
            Assert.AreEqual(node, list.Last);
            Assert.IsTrue(list.Contains(1));
        }

        /// <summary>
        /// Test the <see cref="ReadCopyUpdateList{T}.AddFirst"/> method
        /// </summary>
        [Test]
        public void TestAddFirst()
        {
            // Arrange
            ReadCopyUpdateList<int> list = new ReadCopyUpdateList<int>();

            // Act
            ReadCopyUpdateListNode<int> last = list.AddFirst(0); // Will be last when next node added
            ReadCopyUpdateListNode<int> node = list.AddFirst(1);

            // Assert
            Assert.IsNotNull(node);
            Assert.AreEqual(1, node.Value);
            Assert.IsNotNull(last);
            Assert.AreEqual(node, list.First);
            Assert.AreEqual(last, node.Next);
            Assert.IsTrue(list.Contains(0));
            Assert.IsTrue(list.Contains(1));
        }

        /// <summary>
        /// Test the <see cref="ReadCopyUpdateList{T}.AddAfter"/> method
        /// </summary>
        [Test]
        public void TestAddAfter()
        {
            // Arrange
            ReadCopyUpdateList<int> list = new ReadCopyUpdateList<int>();

            // Assume
            AssertThrow<ArgumentNullException>(() => list.AddAfter(null, 4));

            // Act
            ReadCopyUpdateListNode<int> last = list.AddFirst(0); // Will be last when next node added
            ReadCopyUpdateListNode<int> first = list.AddFirst(1);
            ReadCopyUpdateListNode<int> newLast = list.AddAfter(last, 2);
            ReadCopyUpdateListNode<int> mid = list.AddAfter(first, 3);

            // Assert
            Assert.AreEqual(first, list.First);
            Assert.AreEqual(mid, first.Next);
            Assert.AreEqual(last, mid.Next);
            Assert.AreEqual(newLast, last.Next);            
        }

        /// <summary>
        /// Test the <see cref="ReadCopyUpdateList{T}.Remove(T)"/> method
        /// </summary>
        [Test]
        public void TestRemoveValue()
        {
            // Arrange
            ReadCopyUpdateList<int> list = new ReadCopyUpdateList<int>();

            // Act
            list.AddValues(0, 1, 2, 3, 4, 5);
            bool removed3 = list.Remove(3);
            bool removed3Again = list.Remove(3);
            bool removed72 = list.Remove(72);

            // Assert
            Assert.IsTrue(removed3);
            Assert.IsFalse(removed3Again);
            Assert.IsFalse(removed72);
        }

        /// <summary>
        /// Test the <see cref="ReadCopyUpdateList{T}.Remove(ReadCopyUpdateListNode{T})"/> method
        /// </summary>
        [Test]
        public void TestRemoveNode()
        {
            // Arrange
            ReadCopyUpdateList<int> list = new ReadCopyUpdateList<int>();

            // Assume
            AssertThrow<ArgumentNullException>(() => list.Remove(null));

            // Act
            bool removeFromEmptyList = list.Remove(new ReadCopyUpdateListNode<int>(0));
            ReadCopyUpdateListNode<int> node0 = list.AddFirst(0);
            ReadCopyUpdateListNode<int> node1 = list.AddAfter(node0, 1);
            ReadCopyUpdateListNode<int> node2 = list.AddAfter(node1, 2);
            ReadCopyUpdateListNode<int> node3 = list.AddAfter(node2, 3);
            ReadCopyUpdateListNode<int> node4 = list.AddAfter(node3, 4);
            list.AddAfter(node4, 5);
            
            bool removed3 = list.Remove(node3);
            bool removed3Again = list.Remove(node3);
            bool removed72 = list.Remove(new ReadCopyUpdateListNode<int>(72));
            bool removed0 = list.Remove(node0);

            // Assert
            Assert.IsFalse(removeFromEmptyList);
            Assert.IsTrue(removed3);
            Assert.IsFalse(removed3Again);
            Assert.IsFalse(removed72);
            Assert.IsTrue(removed0);
        }

        /// <summary>
        /// Test the <see cref="ReadCopyUpdateList{T}.Clear()"/> method
        /// </summary>
        [Test]
        public void TestClear()
        {
            // Arrange
            ReadCopyUpdateList<int> list = new ReadCopyUpdateList<int>();

            // Act
            list.AddValues(0, 1, 2, 3, 4);
            bool contains3 = list.Contains(3);
            list.Clear();
            bool contains3AfterClear = list.Contains(3);

            // Assert
            Assert.IsTrue(contains3);
            Assert.IsFalse(contains3AfterClear);
        }

        /// <summary>
        /// Test the <see cref="ReadCopyUpdateList{T}.AddRange"/> method
        /// </summary>
        [Test]
        public void TestAddRange()
        {
            // Arrange
            ReadCopyUpdateList<int> list = new ReadCopyUpdateList<int>();
            int[] range = new[] {0, 1, 2};

            // Assume
            AssertThrow<ArgumentNullException>(() => list.AddRange(null));

            // Act
            list.AddRange(range);
            bool containsRange = list.Contains(0) && list.Contains(1) && list.Contains(2);
           
            // Assert
            Assert.IsTrue(containsRange);            
        }

        /// <summary>
        /// Test the <see cref="ReadCopyUpdateList{T}.FindFirst"/> method
        /// </summary>
        [Test]
        public void TestFindFirst()
        {
            // Arrange
            ReadCopyUpdateList<int> list = new ReadCopyUpdateList<int>();

            // Assume
            AssertThrow<ArgumentNullException>(() => list.FindFirst(null));

            // Act
            list.AddValues(0, 1, 2, 3, 4, 3, 5);
            ReadCopyUpdateListNode<int> first3 = list.FindFirst((n) => n == 3);
            bool removedFirst3 = list.Remove(3);
            ReadCopyUpdateListNode<int> second3 = list.FindFirst((n) => n == 3);
            ReadCopyUpdateListNode<int> first72 = list.FindFirst((n) => n == 72);

            // Assert            
            Assert.IsNotNull(first3);
            Assert.AreEqual(3, first3.Value); 
            Assert.IsNotNull(first3.Next);
            Assert.AreEqual(4, first3.Next.Value);
            Assert.IsTrue(removedFirst3);
            Assert.IsNotNull(second3);
            Assert.AreEqual(3, second3.Value);
            Assert.IsNotNull(second3.Next);
            Assert.AreEqual(5, second3.Next.Value);
            Assert.IsNull(first72);
        }

        /// <summary>
        /// Test the <see cref="IEnumerator{T}"/> interface supported by <see cref="ReadCopyUpdateList{T}"/>
        /// </summary>
        [Test]
        public void TestTypedEnumeration()
        {
            // Arrange
            ReadCopyUpdateList<int> list = new ReadCopyUpdateList<int>();
            list.AddValues(0, 1, 2, 3, 4);
            IEnumerator<int> enumerator = list.GetEnumerator();

            // Act
            enumerator.Reset();
            int count = 0;
            int current;
            AssertThrow<InvalidOperationException>(() => current = enumerator.Current); // Can't access current before move next
            while (enumerator.MoveNext())
            {
                current = enumerator.Current;
                Assert.AreEqual(count, current);
                count++;
            }
            AssertThrow<InvalidOperationException>(() => current = enumerator.Current); // Can't access current after end of enum
            AssertThrow<InvalidOperationException>(() => enumerator.MoveNext());
                        
            // Assert
            Assert.AreEqual(5, count);
        }

        /// <summary>
        /// Test disposal of the <see cref="IEnumerator{T}"/> interface supported by <see cref="ReadCopyUpdateList{T}"/>
        /// </summary>
        [Test]
        public void TestDisposeTypedEnumeration()
        {
            // Arrange
            ReadCopyUpdateList<int> list = new ReadCopyUpdateList<int>();
            list.AddValues(0, 1, 2, 3, 4);
            int count = 0;

            using (IEnumerator<int> enumerator = list.GetEnumerator())
            {

                // Act
                enumerator.Reset();                
                while (enumerator.MoveNext())
                {
                    int current = enumerator.Current;
                    Assert.AreEqual(count, current);
                    count++;
                }             
            }

            // Assert
            Assert.AreEqual(5, count);
        }


        /// <summary>
        /// Test the <see cref="IEnumerator"/> interface supported by <see cref="ReadCopyUpdateList{T}"/>
        /// </summary>
        [Test]
        public void TestObjectEnumeration()
        {
            // Arrange
            ReadCopyUpdateList<int> list = new ReadCopyUpdateList<int>();
            IEnumerable listEnum = list;
            list.AddValues(0, 1, 2, 3, 4);
            IEnumerator enumerator = listEnum.GetEnumerator();

            // Act
            enumerator.Reset();
            int count = 0;
            object current;
            AssertThrow<InvalidOperationException>(() => current = enumerator.Current); // Can't access current before move next
            while (enumerator.MoveNext())
            {
                current = enumerator.Current;
                Assert.AreEqual(count, current);
                count++;
            }
            AssertThrow<InvalidOperationException>(() => current = enumerator.Current); // Can't access current after end of enum
            AssertThrow<InvalidOperationException>(() => enumerator.MoveNext());

            // Assert
            Assert.AreEqual(5, count);
        }

    }
}
