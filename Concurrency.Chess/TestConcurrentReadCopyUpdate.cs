using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Concurrency.TestTools.UnitTesting;
using Microsoft.Concurrency.TestTools.UnitTesting.Chess;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

//**********************************************************************************************
// Instruct Chess to instrument the System.dll assembly so we can catch data races in CLR classes 
// such as LinkedList{T}
//**********************************************************************************************
[assembly: ChessInstrumentAssembly("System")]
[assembly: ChessInstrumentAssembly("Concurrency")]

namespace Concurrency.Chess
{
    /// <summary>
    /// Test the <see cref="ReadCopyUpdateList{T}"/> in a concurrent environment.
    /// </summary>
    [TestFixture]
    public class TestConcurrentReadCopyUpdate
    {
        private static LinkedList<int> llist;
        private static LinkedListNode<int> lnode3; 
        private static ReadCopyUpdateList<int> rculist;
        private static ReadCopyUpdateListNode<int> rcunode3;

        /// <summary>
        /// Test we can read while updating.
        /// </summary>
        [Test]
        [DataRaceTestMethod]
        public void TestConcurrentReadAndUpdate()
        {
            rculist = new ReadCopyUpdateList<int>();
            rculist.AddValues(1, 2 ,3, 4);
            rcunode3 = rculist.Find(3);
            Assert.IsNotNull(rcunode3);

            Parallel.Invoke(
                () =>
                    {
                        // Reader thread
                        ReadCopyUpdateListNode<int> node = rculist.First;
                        int count = 0;
                        while (node != null)
                        {
                            node = node.Next;
                            count++;
                        }
                        Debug.WriteLine("Read completed - count was: " + count);
                    },
                () =>
                    {
                        // Updater thread
                        rculist.Remove(rcunode3);
                        Debug.WriteLine("Node 3 removed");                        
                    });            
        }

        /// <summary>
        /// Test we can read while updating using a standard linked list.
        /// 
        /// If ran under Chess (comment out the Ignore attribute) this test fails with a "DATARACE" error.
        /// This is because the "next" pointers in the <see cref="LinkedListNode{T}"/> class are not volitile
        /// and therefore updates made by one thread are not neccessarily immediately seen by other threads.
        /// 
        /// Compare this with the <see cref="TestConcurrentReadAndUpdate"/> test that uses <see cref="ReadCopyUpdateList{T}"/>.
        /// Both tests perform the same operation but <see cref="ReadCopyUpdateList{T}"/> does not suffer from the "DATARACE" error.
        /// </summary>
        [Microsoft.Concurrency.TestTools.UnitTesting.Ignore]
        [Test]
        [DataRaceTestMethod]
        public void TestConcurrentReadAndUpdateWithLinkedList()
        {
            llist = new LinkedList<int>(new[] {1, 2, 3, 4});
            lnode3 = llist.Find(3);
            Assert.IsNotNull(lnode3);

            Parallel.Invoke(
                () =>
                {
                    // Reader thread
                    LinkedListNode<int> node = llist.First;
                    int count = 0;
                    while (node != null)
                    {
                        node = node.Next;
                        count++;
                    }
                    Debug.WriteLine("Read completed - count was: " + count);
                },
                () =>
                {
                    // Updater thread
                    llist.Remove(lnode3);
                    Debug.WriteLine("Node 3 removed");
                });
        }

    }

}
