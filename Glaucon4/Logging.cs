using Serilog;
using System.Diagnostics;

using Serilog.Formatting.Json;

namespace Terwiel.Glaucon
{

    public partial class Glaucon
    {

        public static void CreateLog()
        {

            if (Param.KeepLog)
            {
                Log.Logger =
                    new LoggerConfiguration()
                    .WriteTo.File(new JsonFormatter(), Param.LogFilename)
                    // set default minimum level
                    .MinimumLevel.Information()
                    .CreateLogger();
                Log.Logger.Information("=========== START Program run ===============");
            }
        }

        [Conditional("LOG")]
        public static void LgIn(string fname)
        {
            if (Param.KeepLog)
            {
                Log.Information($"enter {fname}");

                Debug.WriteLine($"enter {fname}");
            }
        }

        [Conditional("LOG")]
        public static void LgOut(string fname)
        {
            if (Param.KeepLog)
            {
                Log.Information($"exit {fname}");
                Debug.WriteLine($"exit {fname}");
            }
        }

        [Conditional("LOG")]
        public static void Lg(string text)
        {
            if (Param.KeepLog)
            {
                Log.Information(text);
                Debug.WriteLine(text);
            }
        }

    }
}
