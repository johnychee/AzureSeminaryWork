using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataTableStorage1Sample.Model;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace WebRole1.Models
{
    public static class LibraryManager
    {
        internal const string BookTableName = "Book";
        private static CloudTable booksTable;
        public static CloudTable BooksTable
        {
            get { return booksTable ?? (booksTable = InitializeLibrary()); }
        }

        public static Dictionary<string, List<BookEntity>> GetAllBooksFromLibrary()
        {
            Dictionary<string, List<BookEntity>> authorsBooksDictionary = new Dictionary<string, List<BookEntity>>();
            List<LibraryAuthor> authors = GetAllAuthors();

            // Query for all the data within a partition 
            Console.WriteLine("6. Retrieve entities with surname of Smith.");
            foreach (LibraryAuthor author in authors)
            {
                authorsBooksDictionary.Add(author.AuthorName, GetAllBooksForAuthor(BooksTable, author.StoragePartitionKey));
            }
            return authorsBooksDictionary;
        }

        public static List<LibraryAuthor> GetAllAuthors()
        {
            using (SeminaryWorkTasksEntities context = new SeminaryWorkTasksEntities())
            {
                return context.LibraryAuthors.ToList();
            }
        }

        public static List<BookEntity> GetAllBooksForAuthor(CloudTable booksTable, string partitionKey)
        {
            List<BookEntity> books = new List<BookEntity>();

            TableQuery<BookEntity> partitionScanQuery = new TableQuery<BookEntity>().Where
                (TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            TableContinuationToken token = null;
            // Page through the results
            do
            {
                TableQuerySegment<BookEntity> segment = booksTable.ExecuteQuerySegmented(partitionScanQuery, token);
                token = segment.ContinuationToken;
                foreach (BookEntity book in segment)
                {
                    books.Add(book);
                }
            }
            while (token != null);
            return books;
        }

        private static CloudTable InitializeLibrary()
        {
            // Create or reference an existing table
            return CreateTable();
        }

        private static CloudTable CreateTable()
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
                if (table.CreateIfNotExists())
                {
                    Console.WriteLine("Created Table named: {0}", BookTableName);
                }
                else
                {
                    Console.WriteLine("Table {0} already exists", BookTableName);
                }
            }
            catch (StorageException)
            {
                Console.WriteLine("If you are running with the default configuration please make sure you have started the storage emulator. Press the Windows key and type Azure Storage to select and run it from the list of applications - then restart the sample.");
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

        public static BookEntity InsertOrMergeBook(BookEntity newBook)
        {
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(newBook);
            TableResult result = BooksTable.Execute(insertOrMergeOperation);
            QueueManager.CreateRandomQueueMessage(); //Simulating some random event like notification, mail, sms and etc.
            return result.Result as BookEntity;
        }

        public static BookEntity CreateNewBook(string author, string bookName, int price, int count)
        {
            BookEntity newBook = new BookEntity(author, bookName)
            {
                Count = count,
                Price = price
            };
            return InsertOrMergeBook(newBook);
        }

        public static BookEntity RetrieveBookUsingPointQuery(string partitionKey, string rowKey)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<BookEntity>(partitionKey, rowKey);
            TableResult result = BooksTable.ExecuteAsync(retrieveOperation).Result;
            BookEntity book = result.Result as BookEntity;
            QueueManager.CreateRandomQueueMessage(); //Simulating some random event like notification, mail, sms and etc.
            return book;
        }

        public static void DeleteBook(BookEntity book)
        {
            TableOperation deleteOperation = TableOperation.Delete(book);
            BooksTable.Execute(deleteOperation);
            QueueManager.CreateRandomQueueMessage(); //Simulating some random event like notification, mail, sms and etc.
        }
    }
}