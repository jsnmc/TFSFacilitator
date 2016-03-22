using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TFSFacilitator;

namespace TFSFacilitatorUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Validate_GetRequestUri_Success()
        {
            BuildLauncher buildLauncher = new BuildLauncher("localhost", "8080", false);

            string expectedUri = "http://localhost:8080/tfs/DefaultCollection/Test/_apis/build/builds?api-version=2.1";

            var actualUri = buildLauncher.GetRequestUri("DefaultCollection", "Test");
            Assert.AreEqual(expectedUri,actualUri);

        }

        [TestMethod]
        public void Validate_SetVarsInTFSContext_Success()
        {
            BuildLauncher buildLauncher = new BuildLauncher("localhost", "8080", false);
            // SET SYSTEM_TEAMFOUNDATIONCOLLECTIONURI
            // SET SYSTEM_TEAMPROJECT
            // SET BUILD_SOURCEBRANCH
            // This code path would normally be hit in the context of a TFS build - SYSTEM_TEAMFOUNDATIONCOLLECTIONURI tags the tfs and collection name on, therefore,
            // the code makes this assumption, but I don't really have a good way to determine if its there or not.
            System.Environment.SetEnvironmentVariable("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI", "http://mytfsserver:8081/tfs/DefaultCollection/", EnvironmentVariableTarget.Process);
            System.Environment.SetEnvironmentVariable("SYSTEM_TEAMPROJECT", "myTeamProject", EnvironmentVariableTarget.Process);
            System.Environment.SetEnvironmentVariable("BUILD_SOURCEBRANCH", "refs/heads/master", EnvironmentVariableTarget.Process);

            buildLauncher.SetVarsInTFSContext();

            string expectedUri = "http://mytfsserver:8081/tfs/DefaultCollection/myTeamProject/_apis/{area}/{resource}?api-version={version}";
            Assert.AreEqual(expectedUri, buildLauncher.TfsUri);
            


        }
    }
}
