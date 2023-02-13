using Microsoft.Extensions.Logging;
using Rubrics.Worker.Main.Settings;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Rubrics.Worker
{
    public class RavenCertificateProvider
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger _logger;

        public RavenCertificateProvider(AppSettings appSettings, ILogger logger)
        {
            _appSettings = appSettings;
            _logger = logger;
        }

        public async Task<X509Certificate2> RetrieveCertificate()
        {
            _logger.LogInformation("Retrieving certificate");
            var certificateStream = await GetCertificateStreamFromBlobStorage().ConfigureAwait(false);
            return await ReadCertificateFromStream(certificateStream).ConfigureAwait(false);
        }

        private Task<Stream> GetCertificateStreamFromBlobStorage()
        {
            _logger.LogInformation("Getting client certificate from Blob Storage");

            var certificateStorage = new RavenCertificateStorage(
                _appSettings.AzureWebJobsStorage,
                _appSettings.RubricsCertificateBlobContainer);

            return certificateStorage.GetCertificate(_appSettings.RubricsCertificateFileName);
        }

        private async Task<X509Certificate2> ReadCertificateFromStream(Stream certificateStream)
        {
            _logger.LogInformation("Converting certificate byte stream into certificate");

            byte[] bytes;

            await using (var memoryStream = new MemoryStream())
            {
                await certificateStream.CopyToAsync(memoryStream).ConfigureAwait(false);
                bytes = memoryStream.ToArray();
            }

            return new X509Certificate2(bytes, _appSettings.RubricsCertificatePassword);
        }
    }
}