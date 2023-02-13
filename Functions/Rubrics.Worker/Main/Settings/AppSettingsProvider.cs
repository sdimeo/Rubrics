using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Rubrics.Worker.Main.Settings
{
    public static class AppSettingsProvider
    {
        public static AppSettings GetAppSettings(IFunctionsHostBuilder builder)
        {
            var executionContextOptions = builder.Services.BuildServiceProvider()
                .GetService<IOptions<ExecutionContextOptions>>().Value;
            var appSettings = GetAppSettings(executionContextOptions.AppDirectory);
            return appSettings;
        }

        private static AppSettings GetAppSettings(string appDirectory)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(appDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            return builder.Build().Get<AppSettings>();
        }
    }
}