using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace TFSFacilitator
{
    public class BuildLauncher
    {
        // Const defines
        private const string ApiVersion = "2.1";
        private const string Area = "build";
        private const string Resource = "builds";

        // The format of the build API
        // VERB https://{instance}[/{collection}[/{team-project}]/_apis[/{area}]/{resource}?api-version={version}
        private string tfsUri = "{protocol}://{instance}/{collection}/{team-project}/_apis/{area}/{resource}?api-version={version}";

        private bool bUseHttps = false;
        private readonly string instance = "server:port/tfs";
        private string sourceBranch;

        private readonly string collection;
        private string teamProject;

        private readonly string userName = null;
        private readonly string passWord = null;

        internal string TfsUri => this.tfsUri;

        /// <summary>
        /// Construct a BuildLauncher object.  This object is responsible for initiating other TFS builds via the TFS REST API.
        /// This is the most simlistic version of the constructor that will infer project, collection, project, etc.
        /// This version would be best suited to be initiated from a running TFS context.
        /// "{protocol}://{SERVERNAME}:{SERVERPORT}/tfs/{COLLECTION}/{TEAM-PROJECT}/_apis/{AREA}"
        /// </summary>
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        public BuildLauncher(string userName = null, string passWord = null)
        {
            SetVarsInTFSContext();
        }

        /// <summary>
        /// Construct a BuildLauncher object.  This object is responsible for initiating other TFS builds via the TFS REST API.
        /// This is a minimal version of the constructor that will infer user name, project, collection, etc.
        /// This version would be best suited to be initiated from a running TFS context.
        /// "{protocol}://{SERVERNAME}:{SERVERPORT}/tfs/{COLLECTION}/{TEAM-PROJECT}/_apis/{AREA}"
        /// </summary>
        /// <param name="serverName">The actual server name of the target TFS server.  So for "http://mytfsserver:8080/tfs/", the server name is mytfsserver.</param>
        /// <param name="serverPort">The actual server port of the target TFS server.  So for "http://mytfsserver:8080/tfs/", the server port is 8080.</param>
        /// <param name="bUseHttps">Controls if http or https is used in the TFS URI.</param>
        public BuildLauncher(string serverName, string serverPort, bool bUseHttps = false)
        {
            instance = instance.Replace("server", serverName);
            instance = instance.Replace("port", serverPort);
        }

        /// <summary>
        /// Construct a BuildLauncher object.  This object is responsible for initiating other TFS builds via the TFS REST API.
        /// This is a minimal version of the constructor that will infer project, collection, etc.
        /// This version would be best suited to be initiated from a running TFS context.
        /// "{protocol}://{SERVERNAME}:{SERVERPORT}/tfs/{COLLECTION}/{TEAM-PROJECT}/_apis/{AREA}"
        /// </summary>
        /// <param name="serverName">The actual server name of the target TFS server.  So for "http://mytfsserver:8080/tfs/", the server name is mytfsserver.</param>
        /// <param name="serverPort">The actual server port of the target TFS server.  So for "http://mytfsserver:8080/tfs/", the server port is 8080.</param>
        /// <param name="bUseHttps">Controls if http or https is used in the TFS URI.</param>
        /// <param name="userName">The TFS username to use.</param>
        /// <param name="passWord">The TFS password to use.</param>
        public BuildLauncher(string serverName, string serverPort, bool bUseHttps = false, string userName = null, string passWord = null) : 
            this(serverName, serverPort, bUseHttps)
        {
            this.userName = userName;
            this.passWord = passWord;
        }

        /// <summary>
        /// Construct a BuildLauncher object.  This object is responsible for initiating other TFS builds via the TFS REST API.
        /// This is a extended version of the constructor that allows all parameters to be defined.
        /// This version would be best suited to be initiated stand-alone.
        /// "{protocol}://{SERVERNAME}:{SERVERPORT}/tfs/{COLLECTION}/{TEAM-PROJECT}/_apis/{AREA}"
        /// </summary>
        /// <param name="serverName">The actual server name of the target TFS server.  So for "http://mytfsserver:8080/tfs/", the server name is mytfsserver.</param>
        /// <param name="serverPort">The actual server port of the target TFS server.  So for "http://mytfsserver:8080/tfs/", the server port is 8080.</param>
        /// <param name="collection">Specify the TFS Collection of the target build.</param>
        /// <param name="teamProject">Specify the TFS project of the target build </param>
        /// <param name="bUseHttps">Controls if http or https is used in the TFS URI.</param>
        /// <param name="userName">The TFS username to use.</param>
        /// <param name="passWord">The TFS password to use.</param>
        public BuildLauncher(string serverName, string serverPort, string collection, string teamProject,  bool bUseHttps = false, string userName = null, string passWord = null) : 
            this(serverName, serverPort, bUseHttps, userName,passWord)
        {
            this.collection = collection;
            this.teamProject = teamProject;
        }


        /// <summary>
        /// Extracts the TFS environment variables that would be set during the context of running inside of TFS.
        /// </summary>
        internal void SetVarsInTFSContext()
        {
            // Given http://localhost:8080/tfs/DefaultCollection/Test

            // SYSTEM_TEAMFOUNDATIONCOLLECTIONURI
            //http://sith:8080/tfs/DefaultCollection/
            string tempUri = System.Environment.GetEnvironmentVariable("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI");
            if (!string.IsNullOrEmpty(tempUri))
            {
                tfsUri = tfsUri.Replace("{protocol}://{instance}/{collection}/", tempUri);
            }

            // SYSTEM_TEAMPROJECT
            // Test
            teamProject = System.Environment.GetEnvironmentVariable("SYSTEM_TEAMPROJECT");
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

        /// <summary>
        /// Build up the URI
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="project"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        internal string GetRequestUri(string collection, string project, string version = ApiVersion)
        {
            if (string.IsNullOrEmpty(instance) || string.IsNullOrEmpty(collection) || string.IsNullOrEmpty(project)
                || string.IsNullOrEmpty(Resource)) throw new Exception("Missing TFS Request Parameters");

            tfsUri = tfsUri.Replace("{protocol}", bUseHttps ? "https" : "http");
            tfsUri = tfsUri.Replace("{instance}", instance);
            tfsUri = tfsUri.Replace("{collection}", collection);
            tfsUri = tfsUri.Replace("{team-project}", project);
            tfsUri = tfsUri.Replace("{area}", Area);
            tfsUri = tfsUri.Replace("{resource}", Resource);
            tfsUri = tfsUri.Replace("{version}", version);

            return tfsUri;
        }

        /// <summary>
        /// Facilitate the construction of the Request URI
        /// </summary>
        /// <returns></returns>
        internal string GetQueueBuildRequestUri()
        {
            
            if (tfsUri.Contains("{protocol}://{instance}/{collection}"))
            {
                return GetRequestUri(collection, teamProject);
            }
            else
            {
                tfsUri = tfsUri.Replace("{area}", Area);
                tfsUri = tfsUri.Replace("{resource}", Resource);
                tfsUri = tfsUri.Replace("{version}", ApiVersion);
                return tfsUri;
            }
           
        }


        /// <summary>
        /// Queue up a TFS build for a specific build id.
        /// </summary>
        /// <param name="buildId">The TFS build id of that target build to invoke.</param>
        /// <returns></returns>
        public async Task<Response> QueueBuild(string buildId)
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

            // Covert the object over to JSON
            System.Web.Script.Serialization.JavaScriptSerializer js =
                new System.Web.Script.Serialization.JavaScriptSerializer();
            var httpArgsContent = new StringContent(js.Serialize(obj), Encoding.UTF8, "application/json");

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                // If we need Authentication
                if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(passWord))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Basic",
                        Convert.ToBase64String(Encoding.ASCII.GetBytes($"{this.userName}:{this.passWord}")));

                using (HttpResponseMessage response = client.PostAsync(GetQueueBuildRequestUri(), httpArgsContent).Result)
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        // Convert the object over to a friendly C# object
                        Response buildResponseObject = js.Deserialize<Response>(responseBody);
                        Console.WriteLine(responseBody);

                        return buildResponseObject;
                    }
                }
            }
            return null;
        }
    }
}
