using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DataTableStorage1Sample.Model;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using WebRole1.Models;

namespace WebRole1.Controllers
{
  public class StorageController : Controller
  {
    // GET: Storage Table
    public ActionResult Library()
    {
      ViewBag.Message = "Library books list";
      ViewBag.libraryBooks = LibraryManager.GetAllBooksFromLibrary();
      ViewBag.Authors = LibraryManager.GetAllAuthors();
      return View();
    }

    // GET: Storage Queue
    public ActionResult QueueMessages()
    {
      ViewBag.Message = "Azure queue storage";
      //Just 30 of them
      ViewBag.queueMessages = QueueManager.PeekMessages();
      return View();
    }

    [HttpPost]
    public ActionResult CreateBook(FormCollection fc)
    {

      string author = fc["Author"];
      string bookName = fc["BookName"];
      int count = int.Parse(fc["Count"]);
      int price = int.Parse(fc["Price"]);
      LibraryManager.CreateNewBook(author, bookName, price, count);

      return RedirectToAction("Library");
    }

    public ActionResult Edit(string author, string bookName)
    {
      BookEntity book = LibraryManager.RetrieveBookUsingPointQuery(author, bookName);
      return View(book);
    }

    [HttpPost]
    public ActionResult Edit(string author, string bookName, BookEntity model)
    {
      BookEntity book = LibraryManager.RetrieveBookUsingPointQuery(author, bookName);
      if (book.Price != model.Price || book.Count != model.Count)
      {
        book.Count = model.Count;
        book.Price = model.Price;
        LibraryManager.InsertOrMergeBook(book);
      }

      return RedirectToAction("Library", "Storage");
    }

    public ActionResult Delete(string author, string bookName)
    {
      BookEntity book = LibraryManager.RetrieveBookUsingPointQuery(author, bookName);
      LibraryManager.DeleteBook(book);
      return RedirectToAction("Library", "Storage");
    }

    // GET: Storage blobs
    public ActionResult UploadedFiles()
    {
      ViewBag.Message = "Azure files storage";

      ViewBag.UploadedFiles = BlobManager.GetAllFiles();
      return View();
    }

    [HttpPost]
    public ActionResult SaveFile(FormCollection fc,HttpPostedFileBase file)
    {
      BlobManager.SaveFile(file);

      return RedirectToAction("UploadedFiles");
    }

    public ActionResult DeleteFile(string fileName)
    {
      BlobManager.DeleteFile(fileName);
      return RedirectToAction("UploadedFiles", "Storage");
    }
  }
}