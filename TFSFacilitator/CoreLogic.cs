using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TFSFacilitator
{
    class CoreLogic
    {
        // Have to wire the TFS build stop to this?
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();


        protected async void OnStart()
        {
            try
            {
                Console.WriteLine("Starting service Polling");
                await Poll();
            }
            catch (TaskCanceledException exception)
            {
                Console.WriteLine("Exception: {0}", exception);
            }
        }

        protected void OnStop()
        {
            Console.WriteLine("In OnStop");
            
        }
        
        /// <summary>
        /// Main Entry launch point
        /// </summary>
        public void Launch()
        {
            // TODO: Launch all TFS builds here
            //bool ret = Task.Run(() => );
            
            // Start up the polling to determine when the initiated
            // builds are completed
            OnStart();
        }

        private async Task Poll()
        {
            CancellationToken cancellation = _cts.Token;

            TimeSpan[] intervals =
            {
                TimeSpan.FromMinutes(0),
                TimeSpan.FromMinutes(1)
            };
            var index = 0;
            Console.WriteLine("Core logic loop started...");
            while (true)
            {
                await Task.Delay(intervals[index], cancellation);
                Console.WriteLine("Interval fired");
                
                try
                {
                
                    //bool ret = await Task.Run(() => );
                    //if (.HasStopped)
                    //{
                    //    break;
                    //}

                    if (index == 0)
                        index = 1;

                    Console.WriteLine("Interval fired");
                }
                catch
                {
                    // rerun on exception based on retry count
                   // index = 0;
                }

                if (cancellation.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }
}
