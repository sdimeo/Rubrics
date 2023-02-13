namespace Rubrics.Worker.Main.Settings
{
    public class AppSettings
    {
        public string AzureWebJobsServiceBus { get; set; }
        public string AzureWebJobsStorage { get; set; }

        public string DatabaseName { get; set; }
        public string DatabaseUrl { get; set; }

        public string ENDPOINT_NAME { get; set; }
        public string EnvironmentName { get; set; }

        public string RubricsCertificateBlobContainer { get; set; }
        public string RubricsCertificateFileName { get; set; }
        public string RubricsCertificatePassword { get; set; }

        public string NServicebusLicense { get; set; }

        public string SendGridApiKey { get; set; }
        public string ToEmail { get; set; }
        public string ToEmailTech { get; set; }

        
        public int MaxConcurrency { get; set; } = 0;
    }
}
