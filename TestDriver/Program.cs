using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TFSFacilitator;

namespace TestDriver
{
    class Program
    {
       
        static void Main(string[] args)
        {
            // Simple running to invoke the build queue
            // http://localhost:8080/tfs/DefaultCollection/Test/_build
            BuildLauncher buildLauncher = new BuildLauncher(
                "localhost",
                "8080",
                "DefaultCollection",
                "Test",
                false,
                "test",
                "T3stAcct123");

            try
            {
                Test(buildLauncher, "1");
            }
            catch (Exception ex)
            {
                
                Console.WriteLine("If you get unauthorized 401, make sure your server (IIS) has basic authentication on");
            }
            

            
        }

        public static async void Test(BuildLauncher buildLauncher, string id)
        {
            await buildLauncher.QueueBuild(id);
        }
    
    }
}
