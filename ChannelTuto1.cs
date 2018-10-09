using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Serilog;

namespace NetCoreTuto.Tuto1 {
    /// <summary>
    /// https://github.com/stephentoub/corefxlab/blob/master/src/System.Threading.Tasks.Channels/README.md
    /// https://github.com/dotnet/corefx/tree/master/src/System.Threading.Channels
    /// </summary>
    public static class ChannelTuto1 {
        /// <summary>
        /// Test channel
        /// </summary>
        public static void Sample1 () {
            var chan = Channel.CreateBounded<int> (2);
            var w = chan.Writer;
            var r = chan.Reader;
            
            var producer = Task.Run(async () => {
                for (int i = 1; i <= 5; i++) {
                    await w.WriteAsync(i);
                    Log.Information ($"P {i}");
                    await Task.Delay(100);
                }
                w.Complete();
                Log.Information($"Finish producer");
            });
            
            var consumer = Task.Run (async () => {
                while (!r.Completion.IsCompleted) {
                    int i = await r.ReadAsync();
                    Log.Information ($"C {i} ({producer.IsCompleted})");    
                    await Task.Delay(250);
                }
                Log.Information($"Finish consumer");
            });
            
            Task.WaitAll (producer, consumer);
            
            Log.Information($"Enter to exit...");
            Console.ReadLine();
        }
        
        /// <summary>
        /// test Task.Factory.StartNew
        /// </summary>
        public static void Sample2() {
            var t = Task.Factory.StartNew(async ()=>{
               await Task.Delay(300);
               Console.WriteLine("hello");
            }); //t is Task<Task<VoidTaskResult>>
            var tt = t.Unwrap(); //tt is Task<VoidTaskResult>
            tt.Wait(); //we must to wait the unwrap task, otherwise t.Wait() will return immediatly
            Console.WriteLine($"Finished...");
            Task.Delay(500).Wait();
        }
        
        /// <summary>
        /// test Task<void> Run and forget
        /// https://channel9.msdn.com/Series/Three-Essential-Tips-for-Async/Tip-1-Async-void-is-for-top-level-event-handlers-only
        /// </summary>
        public static void Sample3() 
        {
            Action     f1 = async () => { Console.WriteLine("f1 begin"); await Task.Delay(300); Console.WriteLine("f1 end");  throw new Exception("Booomm"); };
            Func<Task> f2 = async () => { Console.WriteLine("f2 begin"); await Task.Delay(300); Console.WriteLine("f2 end"); };
            
            try {
                var t1 = Task.Run(f1); //t1 is Task<void> (t1 is a void-returning task)
                t1.Wait(); //it won't wait, but return immediately (Run and Forget)
                Console.WriteLine("t1 wait exit");
            }
            catch (Exception e) {
                Console.WriteLine("t1: "+e.Message);
            }
            
            // try {
            //     var t2 = Task.Run(f2); //t2 is Task<VoidTaskResult> (t2 is a task-returning task)
            //     t2.Wait(); //it will wait
            //     Console.WriteLine("t2 wait exit");
            // }
            // catch (Exception e) {
            //     Console.WriteLine("t2: "+e.Message);
            // }
            
            Task.Delay(1000).Wait();
        }
       
    }
}
