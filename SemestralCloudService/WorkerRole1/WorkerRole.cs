using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using WebRole1.Models;
using Task = System.Threading.Tasks.Task;
using Microsoft.Azure;

namespace WorkerRole1
{
  public class WorkerRole : RoleEntryPoint
  {
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
    internal const string BookTableName = "Book";
    internal CloudTable booksTable;

    public override void Run()
    {
      Trace.TraceInformation("WorkerRole1 is running");

      try
      {
        this.RunAsync(this.cancellationTokenSource.Token).Wait();
      }
      finally
      {
        this.runCompleteEvent.Set();
      }
    }

    public override bool OnStart()
    {
      // Set the maximum number of concurrent connections
      ServicePointManager.DefaultConnectionLimit = 12;

      // For information on handling configuration changes
      // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

      bool result = base.OnStart();

      Trace.TraceInformation("WorkerRole1 has been started");

      return result;
    }

    public override void OnStop()
    {
      Trace.TraceInformation("WorkerRole1 is stopping");

      this.cancellationTokenSource.Cancel();
      this.runCompleteEvent.WaitOne();

      base.OnStop();

      Trace.TraceInformation("WorkerRole1 has stopped");
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
      InitializeLibrary();
      while (true)
      {
        Console.WriteLine("Continous WorkerRole started its job at [{0}]..", DateTime.Now);
        processWorkerRoleJob();
        Console.WriteLine("Continous WorkerRole finished its job at [{0}]..", DateTime.Now);
        Console.WriteLine("Sleeping for 60 seconds..");
        Thread.Sleep(60000);
        processRandomCountOfQueueMessages();
      }
    }

    private void processRandomCountOfQueueMessages()
    {
      Console.WriteLine("Running processRandomCountOfQueueMessages started its job at [{0}]..", DateTime.Now);
      Random rnd = new Random();
      int num = rnd.Next(1, 3);
      Console.WriteLine("'{0}' queue messages will be processed..", num);
      for (int i = 0; i < num; i++)
      {
        CloudQueueMessage message = QueueManager.DequeueMessage();
        if (message != null)
        {
          Console.WriteLine("Processing & deleting message with content: {0}", message.AsString);
          QueueManager.DeleteMessage(message);
        }
        else
        {
          Console.WriteLine("No queue message to process");
        }
      }
      Console.WriteLine("processRandomCountOfQueueMessages finished its job at [{0}]..", DateTime.Now);
    }

    private void processWorkerRoleJob()
    {
      Console.WriteLine("Running processWorkerRoleJob started its job at [{0}]..", DateTime.Now);
      using (SeminaryWorkTasksEntities context = new SeminaryWorkTasksEntities())
      {
        WebRole1.Models.Task taskToProcess = context.Tasks.FirstOrDefault(t => t.TaskType.Name == "Continous WebRole");
        if (taskToProcess != null)
        {
          Console.WriteLine("Processing task: '{0}'", taskToProcess.Name);
          context.Tasks.Remove(taskToProcess);
          context.SaveChanges();
          GenerateRandomNumberOfQueueMessages();
        }
        else
        {
          Console.WriteLine("No task to process..");
        }
      }
      Console.WriteLine("CprocessWorkerRoleJob finished its job at [{0}]..", DateTime.Now);
    }

    private void GenerateRandomNumberOfQueueMessages()
    {
      Random rnd = new Random();
      int num = rnd.Next(3, 8);

      for (int i = 0; i < num; i++)
      {
        QueueManager.CreateRandomQueueMessage(); //Simulating some random event like notification, mail, sms and etc.
      }
    }

    private void InitializeLibrary()
    {
      // Create or reference an existing table
      booksTable = CreateTableAsync().Result;
    }

    private async Task<CloudTable> CreateTableAsync()
    {
      // Retrieve storage account information from connection string.
      CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(CloudConfigurationManager.GetSetting("StorageConnectionString"));

      // Create a table client for interacting with the table service
      CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

      Console.WriteLine("1. Create a Table for the demo");

      // Create a table client for interacting with the table service 
      CloudTable table = tableClient.GetTableReference(BookTableName);
      try
      {
        if (await table.CreateIfNotExistsAsync())
        {
          Console.WriteLine("Created Table named: {0}", BookTableName);
        }
        else
        {
          Console.WriteLine("Table {0} already exists", BookTableName);
        }
      }
      catch (StorageException ex)
      {
        Console.WriteLine(ex.Message);
        Console.ReadLine();
        throw;
      }

      return table;
    }

    /// <summary>
    /// Validate the connection string information in app.config and throws an exception if it looks like 
    /// the user hasn't updated this to valid values. 
    /// </summary>
    /// <param name="storageConnectionString">Connection string for the storage service or the emulator</param>
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
        Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
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
