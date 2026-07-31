using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AtomReaderNet.Tests
{
    [TestClass]
    public class BufferSizeTests
    {
        [TestMethod]
        public void ZeroBufferSizeThrows()
        {
            using var reader = new AtomReader("hello");
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => reader.BufferSize = 0);
        }

        [TestMethod]
        public void NegativeBufferSizeThrows()
        {
            using var reader = new AtomReader("hello");
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => reader.BufferSize = -1);
        }

        [TestMethod]
        public void TooLargeBufferSizeThrows()
        {
            using var reader = new AtomReader("hello");
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => reader.BufferSize = (128 * 1024 * 1024) + 1);
        }
    }
}
