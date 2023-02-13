using ItOps.Extensions.Logging;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Rubrics.Worker.Main
{
    public class PostStartupActions
    {
        public PostStartupActions(ILoggerFactory loggerFactory)
        {
            LogManager.InitFactory(loggerFactory);
        }

        [FunctionName("TimerFunctionForLoggingSetup")]
        public Task Run([TimerTrigger("0 0 1 1 *", RunOnStartup = true)] TimerInfo myTimer, ILogger logger)
        {
            logger.LogInformation("Timer Function for Logging setup called!");
            return Task.CompletedTask;
        }
    }
}