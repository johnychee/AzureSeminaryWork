using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Mvc;
using SemestralWork.Models;

namespace SemestralWork.Controllers
{
    public class TaskController : Controller
    {

        public ActionResult Create()
        {
            ViewBag.Message = "Create";
            using (SeminaryWorkTasksEntities context = new SeminaryWorkTasksEntities())
            {
                ViewBag.TaskTypes = context.TaskTypes.ToList();
            }

            return View();
        }

        [HttpPost]
        public ActionResult Create(Task model, FormCollection fc)
        {
            
            int taskTypeId = int.Parse(fc["TaskType"]);
            using (SeminaryWorkTasksEntities context = new SeminaryWorkTasksEntities())
            {
                model.TaskType = context.TaskTypes.FirstOrDefault(tt =>
                    tt.Id == taskTypeId);
                context.Tasks.Add(model);
                Console.WriteLine("Creating task '{0}' ..", model.Name);
                context.SaveChanges();
                Console.WriteLine("Task '{0}' created", model.Name);
            }

            if (model.TaskType.Name == "On-Demand WebJob")
            {
                ViewBag.Message = "On-Demand WebJobs page";
                return RedirectToAction("OnDemandJobs");
            }
            else
            {
                ViewBag.Message = "Continous WebJobs page";
                return RedirectToAction("ContinousJobs");
            }
        }

        // GET: Continous WebJob
        public ActionResult ContinousJobs()
        {
            ViewBag.Message = "Continous WebJobs page";
            using (SeminaryWorkTasksEntities context = new SeminaryWorkTasksEntities())
            {
                ViewBag.ContinousWebJobs = context.Tasks.Where(task =>
                    task.TaskType.Name == "Continous WebJob").ToList();
            }

            return View();
        }

        // GET: On-Demand WebJobs
        public ActionResult OnDemandJobs()
        {
            ViewBag.Message = "On-Demand WebJobs page";
            using (SeminaryWorkTasksEntities context = new SeminaryWorkTasksEntities())
            {
                ViewBag.OnDemandWebJobs = context.Tasks.Where(task =>
                    task.TaskType.Name == "On-Demand WebJob").ToList();
            }

            return View();
        }

        public ActionResult Edit(int id)
        {
            Task task;
            using (SeminaryWorkTasksEntities context = new SeminaryWorkTasksEntities())
            {
                ViewBag.TaskTypes = context.TaskTypes.ToList();
                task = context.Tasks.SingleOrDefault(t => t.Id == id);
            }
            if (task != null)
            {
                return View(task);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Edit(int id, Task model, FormCollection fc, string btnCose)
        {
            Task task;
            using (SeminaryWorkTasksEntities context = new SeminaryWorkTasksEntities())
            {
                task = context.Tasks.SingleOrDefault(t => t.Id == id);

                if (task != null)
                {
                    task.Name = model.Name;
                    task.Description = model.Description;
                    int taskTypeId = int.Parse(fc["taskType"]);
                    task.TaskType = context.TaskTypes.FirstOrDefault(t => t.Id == taskTypeId);

                    context.SaveChanges();

                    if (task.TaskType.Name == "Continous WebJob")
                    {
                        return RedirectToAction("ContinousJobs", "Task");
                    }
                    else
                    {
                        return RedirectToAction("OnDemandJobs", "Task");
                    }
                }
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult Delete(int id)
        {
            Task task;
            using (SeminaryWorkTasksEntities context = new SeminaryWorkTasksEntities())
            {
                task = context.Tasks.SingleOrDefault(t => t.Id == id);

                if (task != null)
                {
                    TaskType taskType = task.TaskType;
                    context.Tasks.Remove(task);
                    Console.WriteLine("Removing task '{0}' ..", task.Name);
                    context.SaveChanges();
                    Console.WriteLine("Task '{0}' removed.", task.Name);
                    if (taskType.Name == "Continous WebJob")
                    {
                        return RedirectToAction("ContinousJobs", "Task");
                    }
                    else
                    {
                        return RedirectToAction("OnDemandJobs", "Task");
                    }
                }
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult ProcessODTask()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://hatcheryseminarywork.scm.azurewebsites.net/api/");
            var byteArray = Encoding.ASCII.GetBytes("$hatcheryseminarywork:M0Mp8tWT8qNFfHPm0v4ppR17fcKgY9yKidfyyhsobBaM7ZhSBsiiZZF2hC5F");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            var response = client.PostAsync("triggeredwebjobs/OnDemandWebJob/run", null).Result;
            return RedirectToAction("OnDemandJobs", "Task");
        }
    }
}