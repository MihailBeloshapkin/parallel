using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;

namespace HW3T1
{
    public class Tests
    {
        private MyThreadPool threadPool;
        
        [SetUp]
        public void SetUp()
        {
            threadPool = new MyThreadPool(Environment.ProcessorCount);
        }

        [Test]
        public void SimpleTest()
        {
            var task = threadPool.Submit<int>(() => 2 * 2);
            Assert.AreEqual(4, task.Result);
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void SimpleContinueWithTest()
        {
            threadPool = new MyThreadPool(1);
            var task = threadPool.Submit<int>(() => 2 * 2).ContinueWith(x => x * 3);
            Assert.AreEqual(12, task.Result);
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void NullFunctionTest()
        {
            Assert.Throws<ArgumentNullException>(() => threadPool.Submit<int>(null));
        }

        [Test]
        public void ContinueWorkAfterShutdownTest()
        {
            threadPool = new MyThreadPool(2);
            var task1 = threadPool.Submit(() => 5 * 7);
            threadPool.Shutdown();
            Assert.Throws<InvalidOperationException>(() => threadPool.Submit(() => 2 + 1));
        }

        [Test]
        public void CorrectNumberOfThreadsTest()
        {
            int countOfThreads = 0;
            Object locker = new object();
            threadPool = new MyThreadPool(Environment.ProcessorCount);
            for (int iter = 0; iter < 7; iter++)
            {
                threadPool.Submit(() =>
                {
                    lock (locker)
                    {
                        countOfThreads++;
                        Thread.Sleep(30);
                        return 1;
                    }
                });
            }
            Thread.Sleep(700);
            Assert.IsTrue(Environment.ProcessorCount <= countOfThreads);
        }
    }
}