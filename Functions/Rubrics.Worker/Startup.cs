using ItOps.Extensions.Logging;
using ITOps.SendEmail;
using ITOps.SendEmail.Contracts;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NServiceBus;
using Rubrics.Worker;
using Rubrics.Worker.Main;
using Rubrics.Worker.Main.Settings;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(Startup))]
[assembly: NServiceBusTriggerFunction("%ENDPOINT_NAME%", TriggerFunctionName = "rubrics-worker")]

namespace Rubrics.Worker
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            Activity.ForceDefaultIdFormat = true;

            var appSettings = AppSettingsProvider.GetAppSettings(builder);
            var sendGridSettings = new SendGridSettingsProvider(appSettings.SendGridApiKey);
            var sendGridEmailSender = new SendGridEmailSender(sendGridSettings);
            var logger = LogManager.CreateLogger(nameof(Startup));

            try
            {
                Run(builder, appSettings, sendGridEmailSender, logger);
            }
            catch (Exception e)
            {
                LogInitializationError(e, logger, sendGridEmailSender,
                        appSettings.ToEmailTech, appSettings.ENDPOINT_NAME)
                    .GetAwaiter().GetResult();

                throw;
            }
#if DEBUG
            Console.Title = "RubricsWorker";
#endif
        }

        private static void Run(IFunctionsHostBuilder builder, AppSettings appSettings, SendGridEmailSender emailSender,
            ILogger logger)
        {
            Bootstrapper.Init(builder.Services, appSettings, out var documentStore, logger);
            builder.UseNServiceBus(() => NServicebusConfiguration.ConfigEndPoint(appSettings, documentStore, builder.Services));
        }

        private static async Task LogInitializationError(Exception e, ILogger logger, ISendEmail sendGridEmailSender,
            string toEmailOnError, string endpointName)
        {
            try
            {
                await sendGridEmailSender
                    .SendEmail(toEmailOnError, $"{endpointName}@aicpa.org", GetEmailSubject(), GetEmailBody(e))
                    .ConfigureAwait(false);
            }
            catch (Exception sendGridException)
            {
                logger.LogError(sendGridException, "Failed to send Email.");
            }

            logger.LogCritical(e, $"Failed to initialize {endpointName}.");

            string GetEmailSubject() => $"[EXTERNAL] Failed to initialize {endpointName}.";
            string GetEmailBody(Exception e) => $"Exception Message: {e.Message}" +
                                                "\r\n\r\n" +
                                                $"Exception: {e}";
        }
    }
}