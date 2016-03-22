using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TFSFacilitator;

namespace TFSFacilitatorUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void GetRequestUri()
        {
            BuildLauncher buildLauncher = new BuildLauncher("localhost", "8080");

            string expectedUri = "http://localhost:8080/tfs/DefaultCollection/Test/_apis/build/builds?api-version=2.1";

            var actualUri = buildLauncher.GetRequestUri("DefaultCollection", "Test");
            Assert.AreEqual(expectedUri,actualUri);

        }
    }
}
