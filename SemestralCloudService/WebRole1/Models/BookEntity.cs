namespace DataTableStorage1Sample.Model
{
    using Microsoft.WindowsAzure.Storage.Table;

    public class BookEntity : TableEntity
    {
        public BookEntity() { }

        public BookEntity(string autorName, string bookName)
        {
            this.PartitionKey = autorName;
            this.RowKey = bookName;
        }
      
        public int Count { get; set; }
        public int Price { get; set; }
    }
}
