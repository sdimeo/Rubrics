using ItOps.Tracing.NServiceBusAppInsights;
using ITOps.RavenRepository.Core.PatchFramework;
using ITOps.SendEmail;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Rubrics.Domain.ActivityLogs;
using Rubrics.Domain.Assertions;
using Rubrics.Domain.Items;
using Rubrics.Domain.Keys;
using Rubrics.Domain.MeasurementOpportunities;
using Rubrics.Domain.MOSequences;
using Rubrics.Domain.RubricGuardian;
using Rubrics.Domain.RubricReviews;
using Rubrics.Domain.ScoreSpecs;
using Rubrics.Domain.Scoring.MeasurementOpportunities;
using Rubrics.Domain.TestCases;
using Rubrics.Domain.TestCases.TestCaseEvaluation;
using Rubrics.Handlers.Items;
using Rubrics.Handlers.Items.UseCases;
using Rubrics.Handlers.MOSequences.UseCases;
using Rubrics.Handlers.TestCases;
using Rubrics.Handlers.TestCases.UseCases;
using Rubrics.Infrastructure.CandRespRetrievers;
using Rubrics.Infrastructure.Evaluators;
using Rubrics.Infrastructure.Mapper.RubricJsonMapper;
using Rubrics.Infrastructure.Persistence.RavenDB;
using Rubrics.Infrastructure.Persistence.RavenDB.Configurations;
using Rubrics.Infrastructure.RawRespRetrievers.Factory;
using Rubrics.Infrastructure.RubricXmlMapping;
using Rubrics.Infrastructure.RubricXmlTransformation;
using Rubrics.Infrastructure.ScoreIdMapping;
using Rubrics.Infrastructure.Services;
using Rubrics.Infrastructure.Translator.ItemImport;
using Rubrics.ProcessManagers.ItemImport.Contracts;
using Rubrics.Worker.Main.ConfigurationProvider;
using Rubrics.Worker.Main.Settings;
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Rubrics.Worker.Main
{
    public class Bootstrapper
    {
        private static DocumentStore _documentStore;

        public static void Init(IServiceCollection services, AppSettings appSettings, out DocumentStore documentStore,
            ILogger logger)
        {
            var certificateProvider = new RavenCertificateProvider(appSettings, logger);
            var certificate = certificateProvider.RetrieveCertificate().GetAwaiter().GetResult();

            _documentStore = InitializeDocumentStore(certificate, appSettings.DatabaseUrl, appSettings.DatabaseName, logger);

            RegisterHandlers(services);
            RegisterDocumentStore(services, _documentStore);
            RegisterTelemetry(services);
            RegisterEmailProviders(services, appSettings);

            _documentStore.RunAllPatches("Rubrics.Infrastructure.Persistence.RavenDB", logger);
            AppDomain.CurrentDomain.ProcessExit += ProcessExit;
            documentStore = _documentStore;
        }

        private static void RegisterDocumentStore(IServiceCollection services, IDocumentStore documentStore)
        {
            services.AddSingleton<IDocumentStore>(documentStore);
        }

        private static void RegisterTelemetry(IServiceCollection services)
        {
            services.AddSingleton<ITelemetryModule, NServiceBusInsightsModule>();
        }

        private static void RegisterHandlers(IServiceCollection services)
        {
            services.AddTransient<RubricMapper>();
            services.AddTransient<RubricXmlMapper>();
            services.AddTransient<MCQCandidateResponseRetriever>();
            services.AddTransient<RawResponsesRetrieversFactory>();
            services.AddTransient<ItemTranslator>();
            services.AddTransient<ScoreIdTranslator>();
            services.AddTransient<TransformRubricXml>();

            services.AddTransient<MoSequenceUseCase>();
            services.AddTransient<RequireExecutionOfTestCasesUseCase>();
            services.AddTransient<AssignItemStatusUseCase>();

            services.AddTransient<TestCaseEvaluationService>();
            services.AddTransient<MOSequenceService>();

            services.AddTransient<DeleteTestCaseHandler>();
            services.AddTransient<ApproveRubricValidator>();

            services.AddTransient<IMeasurementOpportunityRepository, MeasurementOpportunityRavenDBRepositoryWithNServiceBus>();
            services.AddTransient<IKeyRepository, KeyRavenDBRepositoryWithNServiceBus>();
            services.AddTransient<IAssertionRepository, AssertionRavenDBRepositoryWithNServiceBus>();
            services.AddTransient<IActivityLogService, ActivityLogRavenDBService>();
            services.AddTransient<IRubricReviewRepository, RubricReviewRavenDBRepositoryWithNServiceBus>();
            services.AddTransient<IItemRepository, ItemRavenDBRepositoryWithNServiceBus>();
            services.AddTransient<ITestCaseRepository, TestCaseRavenDBRepositoryWithNServiceBus>();
            services.AddTransient<IProtectMos, RubricGuardianService>();
            services.AddTransient<IProtectKeys, RubricGuardianService>();
            services.AddTransient<IProtectAssertions, RubricGuardianService>();
            services.AddTransient<IProtectApprovals, RubricGuardianService>();
            services.AddTransient<IProtectTestCases, RubricGuardianService>();
            services.AddTransient<IMOSequenceRepository, MOSequenceRavenDBRepositoryWithNServiceBus>();
            services.AddTransient<IScoreSpecRepository, ScoreSpecRavenDBRepositoryWithNServiceBus>();
            services.AddTransient<IEvaluateMeasurementOpportunities, UbervatorEvaluator>();
            services.AddTransient<IProvideMigrationSetting, MigrationSettingsProvider>();
            services.AddTransient<ITranslateItemImportData, ItemImportTranslation>();
            services.AddTransient<IValidateItemImportData, ItemImportValidator>();
        }

        private static void RegisterEmailProviders(IServiceCollection services, AppSettings appSettings)
        {
            services.RegisterEmailProviders(appSettings.SendGridApiKey);
        }

        private static DocumentStore InitializeDocumentStore(X509Certificate2 cert, string databaseUrl, string databaseName, ILogger logger)
        {
            logger.LogInformation("Initializing the document store");

            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            var store = new DocumentStore
            {
                Urls = new[] { databaseUrl },
                Database = databaseName,
                Certificate = cert
            };

            store.ConfigureMaxNumberOfRequestsPerSession();
            store.ConfigureJsonContractResolver();
            store.ConfigureIdentityPropertyName();
            store.ConfigureTypeCollectionName();
            store.Initialize();
            store.InitializeSeedForMeasurementOpportunities();

            return store;
        }

        private static void ProcessExit(object sender, EventArgs e)
        {
            _documentStore.Dispose();
        }
    }
}