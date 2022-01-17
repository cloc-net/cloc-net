using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cloc.Tests
{
    [TestClass]
    public class CheckerTests
    {
        [TestMethod]
        public void Count_Empty()
        {
            using (var checker = new Checker())
            {
                var files = checker.Count(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "TestData", "Empty"));

                Assert.AreEqual(0, files.Count);
            }
        }

        [TestMethod]
        public void Count_ClocTest()
        {
            using (var checker = new Checker())
            { 
                var files = checker.Count(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "TestData", "Cloc-Test"));

                Assert.AreEqual(4, files.Count);

                var moreTeapotsRenderer = files.Where(f => f.Path.EndsWith("MoreTeapotsRenderer.cpp")).FirstOrDefault();
                Assert.AreNotEqual(null, moreTeapotsRenderer);
                Assert.AreEqual("C++", moreTeapotsRenderer.Language);
                Assert.AreEqual(76, moreTeapotsRenderer.Blank);
                Assert.AreEqual(365, moreTeapotsRenderer.Code);
                Assert.AreEqual(114, moreTeapotsRenderer.Comment);
            }
        }

        [TestMethod]
        public void Count_ClocTest_Exclude()
        {
            using (var checker = new Checker())
            {
                var files = checker.Count(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "TestData", "Cloc-Test"), "cc");

                Assert.AreEqual(3, files.Count);
            }
        }

        [TestMethod]
        public void Count_ClocTest_Array()
        {
            using (var checker = new Checker())
            {
                var files = checker.Count(new String[] { Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "TestData", "Cloc-Test", "cc"), Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "TestData", "Cloc-Test", "ee") });

                Assert.AreEqual(2, files.Count);
            }
        }

        [TestMethod]
        public void Count_ClocTest_Array_Exclude()
        {
            using (var checker = new Checker())
            {
                var files = checker.Count(new String[] { Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "TestData", "Cloc-Test", "cc"), Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "TestData", "Cloc-Test", "ee") }, "*.c");

                Assert.AreEqual(2, files.Count);
            }
        }
    }
}
