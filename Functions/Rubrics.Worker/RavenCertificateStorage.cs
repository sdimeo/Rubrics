using System.IO;
using System.Threading.Tasks;
using ITOps.BlobStorage;

namespace Rubrics.Worker
{
    public class RavenCertificateStorage : AzureBlobStorage
    {
        public RavenCertificateStorage(string storageConnectionString, string blobContainerName) 
            : base(storageConnectionString, blobContainerName)
        { }

        public Task<Stream> GetCertificate(string name)
        {
            return Get(name);
        }
    }
}