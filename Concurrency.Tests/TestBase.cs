using System;
using NUnit.Framework;

namespace Concurrency.Tests
{
    /// <summary>
    /// Base class for tests - includes some assertions for common testing scenarios that should be included in NUnit but aren't :(
    /// </summary>
    public class TestBase
    {
        /// <summary>
        /// Perform an action that expects an exception of a given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <param name="action"></param>
        public static void AssertThrow<T>(Action action)
            where T : Exception
        {
            try
            {
                action();
                Assert.Fail("Expected exception of type {0}", typeof(T).FullName);
            }
            catch (T)
            {
                // Success
            }
        }

    }
}
