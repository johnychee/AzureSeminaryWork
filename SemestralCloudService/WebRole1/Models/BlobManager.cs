using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace WebRole1.Models
{
  public static class BlobManager
  {
    internal const string blobName = "blobsstorage";
    private static CloudBlobContainer blobContainer;
    public static CloudBlobContainer BlobContainer
    {
      get { return blobContainer ?? (blobContainer = InitializeQueue()); }
    }

    private static CloudBlobContainer InitializeQueue()
    {
      // Create or reference an existing table
      return CreateBlobStorage();
    }

    private static CloudBlobContainer CreateBlobStorage()
    {
      // Retrieve storage account information from connection string
      // How to create a storage connection string - http://msdn.microsoft.com/en-us/library/azure/ee758697.aspx
      CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(CloudConfigurationManager.GetSetting("StorageConnectionString"));

      // Create a blob client for interacting with the blob service.
      CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

      // Create a container for organizing blobs within the storage account.
      Console.WriteLine("1. Creating Container");
      CloudBlobContainer container = blobClient.GetContainerReference(blobName);
      try
      {
        container.CreateIfNotExists();
      }
      catch (StorageException ex)
      {
        throw;
      }
      return container;
    }

    /// <summary>
    /// Validates the connection string information in app.config and throws an exception if it looks like 
    /// the user hasn't updated this to valid values. 
    /// </summary>
    /// <param name="storageConnectionString">The storage connection string</param>
    /// <returns>CloudStorageAccount object</returns>
    private static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
    {
      CloudStorageAccount storageAccount;
      try
      {
        storageAccount = CloudStorageAccount.Parse(storageConnectionString);
      }
      catch (FormatException)
      {
        Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
        Console.ReadLine();
        throw;
      }
      catch (ArgumentException)
      {
        Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
        Console.ReadLine();
        throw;
      }

      return storageAccount;
    }

    public static IEnumerable<CloudBlockBlob> GetAllFiles()
    {
      QueueManager.CreateRandomQueueMessage(); //Simulating some random event like notification, mail, sms and etc.
      return BlobContainer.ListBlobs(null, true, BlobListingDetails.All).Cast<CloudBlockBlob>();
    }

    public static void SaveFile(HttpPostedFileBase file)
    {
      CloudBlockBlob blockBlob = BlobContainer.GetBlockBlobReference(file.FileName);
      blockBlob.UploadFromStream(file.InputStream);
      QueueManager.CreateRandomQueueMessage(); //Simulating some random event like notification, mail, sms and etc.
    }

    public static void DeleteFile(string fileName)
    {
      CloudBlockBlob blockBlob = BlobContainer.GetBlockBlobReference(fileName);
      blockBlob.Delete();
      QueueManager.CreateRandomQueueMessage(); //Simulating some random event like notification, mail, sms and etc.
    }
  }
}