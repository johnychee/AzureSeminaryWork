using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace WebRole1.Models
{
  public static class QueueManager
  {
    internal const string queueName = "messages";
    private static CloudQueue messagesQueue;
    public static CloudQueue MessagesQueue
    {
      get { return messagesQueue ?? (messagesQueue = InitializeQueue()); }
    }

    private static CloudQueue InitializeQueue()
    {
      // Create or reference an existing table
      return CreateQueue();
    }

    private static CloudQueue CreateQueue()
    {
      // Retrieve storage account information from connection string.
      CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(CloudConfigurationManager.GetSetting("StorageConnectionString"));
      // Create a queue client for interacting with the queue service
      CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
      CloudQueue queue = queueClient.GetQueueReference(queueName);
      try
      {
        queue.CreateIfNotExists();
      }
      catch (StorageException ex)
      {
        Console.WriteLine("If you are running with the default configuration please make sure you have started the storage emulator. Press the Windows key and type Azure Storage to select and run it from the list of applications - then restart the sample.");
        Console.ReadLine();
        throw;
      }

      return queue;
    }

    public static CloudQueueMessage DequeueMessage()
    {
      return MessagesQueue.GetMessage();
    }

    public static void DeleteMessage(CloudQueueMessage message)
    {
      MessagesQueue.DeleteMessage(message);
    }

    public static CloudQueueMessage PeekOneMessage()
    {
      return MessagesQueue.PeekMessage();
    }

    public static IEnumerable<CloudQueueMessage> PeekMessages()
    {
      return MessagesQueue.PeekMessages(30);
    }

    public static void CreateRandomQueueMessage()
    {
      MessagesQueue.AddMessage(GenerateRandomMessage());
    }

    private static CloudQueueMessage GenerateRandomMessage()
    {
      Random rnd = new Random();
      int num = rnd.Next(0, 20);
      if (num < 7)
      {
        return new CloudQueueMessage("Amazing email message");
      }
      if (num < 14)
      {
        return new CloudQueueMessage("Handsome SMS message");
      }
      return new CloudQueueMessage("Awesome SOAP message");
    }

    /// <summary>
    /// Validate the connection string information in app.config and throws an exception if it looks like 
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
  }
}