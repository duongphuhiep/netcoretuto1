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

        private static async Task ProduceRange (ChannelWriter<int> c, int count) {
            for (int i = 0; i < count; i++) {
                while (await c.WaitToWriteAsync ()) {
                    if (c.TryWrite (i)) {
                        Log.Information ("P " + i);
                        break;
                    }
                }
            }
            c.Complete ();
        }

        public static void Sample1 () {
            var chan = Channel.CreateBounded<int> (2);
            var w = chan.Writer;
            var r = chan.Reader;

            var producer = Task.Factory.StartNew (async () => {
                for (int i = 1; i <= 5; i++) {
                    await w.WriteAsync(i);
                    Log.Information ($"P {i}");
                    //Thread.Sleep(100);
                    await Task.Delay(100);
                }
                w.Complete();
                Log.Information("Finish producer");
            });

            var consumer = Task.Factory.StartNew (async () => {
                while (!r.Completion.IsCompleted) {
                    int i = await r.ReadAsync();
                    Log.Information ($"C {i}");    
                    //Thread.Sleep(250);
                    await Task.Delay(250);
                }
                Log.Information("Finish consumer");
            });
            
            // var x = Task.WhenAll(producer, consumer);
            // x.Wait();
            //while (!x.IsCompleted) {};
            
            //Task.WaitAll (producer, consumer);
            Log.Information("Enter to exit...");
            Console.ReadLine();
        }
    }
}

// P 1
// C 1
// P 2
// P 3
// C 2
// P 4
// P 5
// C 3
// C 4
// C 5