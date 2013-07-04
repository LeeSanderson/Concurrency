using System.Diagnostics;
using System.Threading;
using Microsoft.Concurrency.TestTools.UnitTesting;
using NUnit.Framework;

namespace Concurrency.Chess
{
    /// <summary>
    /// This class contains sample concurrency unit tests with both nunit and chess test attributes.
    /// All concurrency unit tests should follow the pattern defined here.
    /// 
    /// The nunit attributes allow the tests to be run in VS (via Resharper).
    /// 
    /// The chess test attributes allow the test to be run via the Microsoft Concurrency Unit testing tool (mcut.exe) 
    /// (see the Concurrency.build nant script for details).
    /// 
    /// WARNING: Do not include setup/teardown methods - all tests MUST be self contained. 
    /// (Although the setup/teardown methods may be called by nunit they are ignored by chess.)
    /// </summary>
    [TestFixture]
    public class TestConcurrencyFramework
    {
        private static object lock1 = new object();
        private static object lock2 = new object();

        /// <summary>
        /// Test dead lock by using inconsistent lock ordering.
        /// This test may work in nunit or it may fail depending on the order in which the threads execute.
        /// Under chess this test will always fail with a "deadlock" error (if the Ignore attributes are commented out).
        /// </summary>
        [Microsoft.Concurrency.TestTools.UnitTesting.Ignore]
        [NUnit.Framework.Ignore("May cause nunit to hang")]
        [Test]
        [ScheduleTestMethod]
        public void TestDeadLockCausedByInconsistentLockOrdering()
        {
            lock1 = new object();
            lock2 = new object();

            Thread t = new Thread(() =>
            {
                // In child thread lock2 then lock1
                lock (lock2)
                {
                    lock (lock1)
                    {
                        Debug.WriteLine("Child thread successful - aquired lock2 then lock1");
                    }
                }
            });
            t.Start();

            // In parent thread lock1 then lock2
            lock (lock1)
            {
                lock (lock2)
                {
                    Debug.WriteLine("Parent thread successful - aquired lock1 then lock2");
                }
            }
            t.Join();
        }

    }
}
