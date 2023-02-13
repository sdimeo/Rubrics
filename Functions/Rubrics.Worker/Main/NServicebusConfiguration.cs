using ItOps.Extensions.Logging;
using ItOps.NServicebusConfiguration.Common;
using ItOps.NServicebusWithRavenDb;
using ITOps.SendEmail.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NServiceBus;
using NServiceBus.Faults;
using Raven.Client.Documents;
using Rubrics.Worker.Main.Settings;
using System;
using System.Threading.Tasks;

namespace Rubrics.Worker.Main
{
    public class NServicebusConfiguration
    {
        public static ServiceBusTriggeredEndpointConfiguration ConfigEndPoint(AppSettings appSettings, DocumentStore documentStore, IServiceCollection services)
        {
            var licenseAndMonitoringDetails = GetLicensingAndMonitoringDetails(appSettings);

            var builder = new NServicebusWithRavenDbConfigurationBuilder(
                appSettings.AzureWebJobsServiceBus, appSettings.AzureWebJobsStorage, appSettings.EnvironmentName, appSettings.ENDPOINT_NAME,
                documentStore, licenseAndMonitoringDetails,
                (message) => OnError(message, appSettings, services),
                appSettings.MaxConcurrency);

            var endpointConfiguration = builder.Build();

            endpointConfiguration.AdvancedConfiguration.ExecuteTheseHandlersFirst(HandlerOrderSpecification.GetHandlerOrder());

            return endpointConfiguration;
        }

        private static LicenseAndMonitoringDetails GetLicensingAndMonitoringDetails(AppSettings appSettings)
        {
            return new LicenseAndMonitoringDetails(appSettings.NServicebusLicense, string.Empty, string.Empty);
        }

        private static async Task OnError(FailedMessage failedMessage, AppSettings appSettings, IServiceCollection services)
        {
            var emailSender = services.BuildServiceProvider().GetRequiredService<ISendEmail>();
            var logger = LogManager.CreateLogger("NServicebusErrorLogger");

            var fromEmail = $"{appSettings.ENDPOINT_NAME}@aicpa.org";
            var messageType = failedMessage.Headers["NServiceBus.EnclosedMessageTypes"];
            var emailSubject = $"Message moved to the error queue in {appSettings.ENDPOINT_NAME}";
            var emailBody = $"MessageType:{Environment.NewLine}{messageType}{Environment.NewLine}{Environment.NewLine}" +
                            $"Exception Message:{Environment.NewLine}{failedMessage.Exception.Message}{Environment.NewLine}{Environment.NewLine}" +
                            $"Exception:{Environment.NewLine}{failedMessage.Exception}{Environment.NewLine}{Environment.NewLine}" +
                            $"Message Id:{Environment.NewLine}{failedMessage.MessageId}";

            await emailSender.SendEmail(appSettings.ToEmailTech, fromEmail, emailSubject, emailBody).ConfigureAwait(false);

            logger.LogError($"MessageId:{Environment.NewLine}{failedMessage.MessageId}");
            logger.LogError($"Exception:{Environment.NewLine}{failedMessage.Exception}");
        }
    }
}