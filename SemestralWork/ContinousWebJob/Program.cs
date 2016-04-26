using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SemestralWork.Models;

namespace ContinousWebJob
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Continous WebJob runned at [{0}]..", DateTime.Now);
                using (SeminaryWorkTasksEntities context = new SeminaryWorkTasksEntities())
                {
                    Task taskToProcess = context.Tasks.FirstOrDefault(t => t.TaskType.Name == "Continous WebJob");
                    if (taskToProcess != null)
                    {
                        Console.WriteLine("Processing task: '{0}'", taskToProcess.Name);
                        context.Tasks.Remove(taskToProcess);
                        context.SaveChanges();
                    }
                    else
                    {
                        Console.WriteLine("No task to process..");
                    }
                }

                Console.WriteLine("Continous WebJob finished at [{0}]..", DateTime.Now);
                Console.WriteLine("Sleeping for 60 seconds..");
                Thread.Sleep(60000);
            }
        }
    }
}
