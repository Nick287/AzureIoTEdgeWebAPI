using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureIoTEdgeWebAPI
{
    public class CloudStorageHelper
    {

        CloudStorageAccount storageAccount = null;
        CloudBlobContainer cloudBlobContainer = null;
        CloudBlobClient cloudBlobClient = null;

        public CloudStorageHelper(string storageconnectionstring)
        {
            if (CloudStorageAccount.TryParse(storageconnectionstring, out storageAccount))
            {
                try
                {
                    // Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
                    cloudBlobClient = storageAccount.CreateCloudBlobClient();
                }
                catch (StorageException ex)
                {
                    Console.WriteLine("Error returned from the service: {0}", ex.Message);
                }
            }
        }

        public MemoryStream DownloadFile(string containerName, string pathAndFileName)
        {
            try
            {
                cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);

                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(pathAndFileName);

                using (var memoryStream = new MemoryStream())
                {
                    cloudBlockBlob.DownloadToStream(memoryStream);
                    //string text = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                    return memoryStream;
                }
            }
            catch (StorageException ex)
            {
                Console.WriteLine("Error returned from the service: {0}", ex.Message);
            }

            return null;
        }
    }
}
