using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TFSFacilitator
{
    public class BuildLauncher
    {
        private const string apiVersion = "2.1";
        private const string area = "build";
        private const string resource = "builds";


        // VERB https://{instance}[/{collection}[/{team-project}]/_apis[/{area}]/{resource}?api-version={version}
        private string tfsUri =
            "{protocol}://{instance}/{collection}/{team-project}/_apis/{area}/{resource}?api-version={version}";

        private bool bUseHttps = false;
        private string instance = "server:port/tfs";
        private string sourceBranch;

        private string collection;
        private string teamProject;

        private string userName = null;
        private string passWord = null;

        public BuildLauncher(string userName = null, string passWord = null)
        {
            SetVarsInTFSContext();
        }

        public BuildLauncher(string serverName, string serverPort, bool bUseHttps = false)
        {
            instance = instance.Replace("server", serverName);
            instance = instance.Replace("port", serverPort);
        }

        public BuildLauncher(string serverName, string serverPort, bool bUseHttps = false, string userName = null, string passWord = null) : 
            this(serverName, serverPort, bUseHttps)
        {
            this.userName = userName;
            this.passWord = passWord;
        }

        public BuildLauncher(string serverName, string serverPort, string collection, string teamProject,  bool bUseHttps = false, string userName = null, string passWord = null) : 
            this(serverName, serverPort, bUseHttps, userName,passWord)
        {
            this.collection = collection;
            this.teamProject = teamProject;
        }

        private void SetVarsInTFSContext()
        {
            // Given http://localhost:8080/tfs/DefaultCollection/Test

            // SYSTEM_TEAMFOUNDATIONCOLLECTIONURI
            //http://sith:8080/tfs/DefaultCollection/
            string tempUri = System.Environment.GetEnvironmentVariable("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI");
            if (!string.IsNullOrEmpty(tempUri))
            {
                tfsUri = tfsUri.Replace("{ protocol}://{instance}/{collection})", tempUri);
            }

            // SYSTEM_TEAMPROJECT
            // Test
            string teamProject = System.Environment.GetEnvironmentVariable("SYSTEM_TEAMPROJECT");
            if (!string.IsNullOrEmpty(teamProject))
            {
                tfsUri = tfsUri.Replace("{team-project}", teamProject);
            }

            // BUILD_SOURCEBRANCH
            // refs/heads/master
            //BUILD_SOURCEBRANCHNAME
            // master
            sourceBranch = System.Environment.GetEnvironmentVariable("BUILD_SOURCEBRANCH");
            
        }


        public string GetRequestUri(string collection, string project, string version = apiVersion)
        {
            if (string.IsNullOrEmpty(instance) || string.IsNullOrEmpty(collection) || string.IsNullOrEmpty(project)
                || string.IsNullOrEmpty(resource)) throw new Exception("Missing TFS Request Parameters");

            tfsUri = tfsUri.Replace("{protocol}", bUseHttps ? "https" : "http");
            tfsUri = tfsUri.Replace("{instance}", instance);
            tfsUri = tfsUri.Replace("{collection}", collection);
            tfsUri = tfsUri.Replace("{team-project}", project);
            tfsUri = tfsUri.Replace("{area}", area);
            tfsUri = tfsUri.Replace("{resource}", resource);
            tfsUri = tfsUri.Replace("{version}", version);

            return tfsUri;
        }


        private string GetQueueBuildRequestUri()
        {
            
            if (!tfsUri.Contains("{protocol}://{instance}/{collection}"))
            {
                return GetRequestUri(collection, teamProject);
            }
            else
            {
                tfsUri = tfsUri.Replace("{area}", area);
                tfsUri = tfsUri.Replace("{resource}", resource);
                tfsUri = tfsUri.Replace("{version}", apiVersion);
                return tfsUri;
            }
           
        }
    

        public async Task QueueBuild(string buildId)
        {
            dynamic obj = new
            {
                definition = new
                {
                    id = buildId,
                    type = "build"
                }                
            };
            

            if (!string.IsNullOrEmpty(sourceBranch))
            {
                obj = new
                {
                    obj,
                    sourceBranch = sourceBranch
                    //"sourceBranch": "refs/heads/master"
                };
            }

            System.Web.Script.Serialization.JavaScriptSerializer js =
                new System.Web.Script.Serialization.JavaScriptSerializer();
            var httpArgsContent = new StringContent(js.Serialize(obj), Encoding.UTF8, "application/json");

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(passWord))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Basic",
                        Convert.ToBase64String(Encoding.ASCII.GetBytes($"{this.userName}:{this.passWord}")));

                using (HttpResponseMessage response = client.PostAsync(GetQueueBuildRequestUri(), httpArgsContent).Result)
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);
                }
            }
        }


        internal class ResponseCodes
        {
            private string GetResponseCodeDetails(string code)
            {

                switch (code)
                {
                    case "200":
                        return "Success, and there is a response body.";
                    case "201":
                        return
                            "Success, when creating resources.Some APIs return 200 when successfully creating a resource.Look at the docs for the API you're using to be sure.";
                    case "204":
                        return
                            "Success, and there is no response body.For example, you'll get this when you delete a resource.";
                    case "400":
                        return "The parameters in the URL or in the request body aren't valid.";
                    case "403":
                        return "The authenticated user doesn't have permission to perform the operation.";
                    case "404":
                        return
                            "The resource doesn't exist, or the authenticated user doesn't have permission to see that it exists.";
                    case "409":
                        return
                            "There's a conflict between the request and the state of the data on the server. For example, if you attempt to submit a pull request and there is already a pull request for the commits, the response code is 409.";
                }

                return string.Empty;
            }
        }
    }
}
