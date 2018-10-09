using System;
using System.Threading.Tasks;
using Serilog;

namespace NetCoreTuto.Tuto1 {
    class Program {
        static void Main (string[] args) {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss,fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")  //write log to console, delete it if you don't want to
                .CreateLogger();
                
            //ChannelTuto1.Sample4();
            JsonTuto3.create_json_mixed();
        }
    }
}