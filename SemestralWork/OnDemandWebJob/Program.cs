using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SemestralWork.Models;

namespace OnDemandWebJob
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("On-Demand WebJob runned at [{0}]..", DateTime.Now);

            using (SeminaryWorkTasksEntities context = new SeminaryWorkTasksEntities())
            {
                Task taskToProcess = context.Tasks.FirstOrDefault(t => t.TaskType.Name == "On-Demand WebJob");
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

            Console.WriteLine("On-Demand WebJob finished at [{0}]..", DateTime.Now);
        }
    }
}
