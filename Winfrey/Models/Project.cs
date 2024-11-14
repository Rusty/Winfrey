using OpenTK.Graphics.ES20;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winfrey.Models
{
    public class Project
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime DataDate { get; set; }
        public DateTime? FinishDate { get; set; }


        public Dictionary<string, Task> Tasks { get; set; }   
        public List<Link> Links { get; set; }
        public Dictionary<string, Calendar> Calendars { get; set; }

        public Project()
        {
            Tasks = new Dictionary<string, Task>(); 
            Links = new List<Link>();
            Calendars = new Dictionary<string, Calendar>();
        }

        public void LoadProject()
        {
            Tasks["1"] = new Task() { Name="task1", RemaningDuration=5, Id="1", CalendarId="1"};
            Tasks["2"] = new Task() { Name = "task2", RemaningDuration = 5, Id = "2", CalendarId = "1" };
            Tasks["3"] = new Task() { Name = "task3", RemaningDuration = 15, Id = "3", CalendarId = "1" };
            Tasks["4"] = new Task() { Name = "task4", RemaningDuration = 5, Id = "4", CalendarId = "1" };
            Tasks["5"] = new Task() { Name = "task5", RemaningDuration = 15, Id = "5", CalendarId = "1" };
            Tasks["6"] = new Task() { Name = "task6", RemaningDuration = 5, Id = "6", CalendarId = "1" };
            Tasks["7"] = new Task() { Name = "task7", RemaningDuration = 7, Id = "7", CalendarId = "1" };

            Links.Add(new Link() { PrecedingTaskId="1", SucceedingTaskId="2"});
            Links.Add(new Link() { PrecedingTaskId = "1", SucceedingTaskId = "3" });
            Links.Add(new Link() { PrecedingTaskId = "2", SucceedingTaskId = "4" });
            Links.Add(new Link() { PrecedingTaskId = "4", SucceedingTaskId = "5" });
            Links.Add(new Link() { PrecedingTaskId = "3", SucceedingTaskId = "4" });
            Links.Add(new Link() { PrecedingTaskId = "2", SucceedingTaskId = "7" });
            Links.Add(new Link() { PrecedingTaskId = "7", SucceedingTaskId = "4" });

            StartDate = DateTime.Now;
            DataDate = DateTime.Now;
        }

        public void Init()
        {
            foreach (var task in Tasks.Values)
            {
                task.Reset(true);
            }

            foreach (var link in Links)
            {
                Tasks[link.SucceedingTaskId].BRels.Add(new LinkBackwards(link));
                Tasks[link.PrecedingTaskId].FRels.Add(new LinkForewards(link)); 
            }
        }

        public void Analyse()
        {
            foreach (var task in Tasks.Values)
            {
                task.Reset(false);
            }

            // forward pass
            foreach (var task in Tasks.Values.Where(t => !t.BRels.Any())) 
            {
                task.IsStart = true; // for testing
                ForwardPass(DataDate, task);
            }
            FinishDate = Tasks.Values.Max(t => t.EF);

            // backwards pass
            foreach (var task in Tasks.Values.Where(t => !t.FRels.Any()))
            {
                task.IsFinish = true; // for testing
                BackwardPass(FinishDate.Value, task);
            }

            // calculate float and criticality
            foreach (var task in Tasks.Values)
            {
                task.TF = (task.LF.Value - task.EF.Value).TotalDays;
                task.FF = 0;
                if (task.FRels.Any())
                {
                    task.FF = (task.FRels.Min(s => Tasks[s.SucceedingTaskId].ES).Value - task.EF.Value).TotalDays;
                }
            }
        }

        public void ForwardPass(DateTime start, Task task)
        {
            if (start > task.ES || task.ES == null)
                task.ES = start;
            task.EF = DateAdd(task.ES, task.RemaningDuration);
            foreach (var l in task.FRels)
            {
                ForwardPass(task.EF.Value, Tasks[l.SucceedingTaskId]);
            }
        }
        public void BackwardPass(DateTime finish, Task task)
        {
            if (finish < task.LF || task.LF == null)
                task.LF = finish;
            task.LS = DateSub(task.LF, task.RemaningDuration);
            foreach (var l in task.BRels)
            {
                BackwardPass(task.LS.Value, Tasks[l.PrecedingTaskId]);
            }
        }


        DateTime? DateAdd(DateTime? date, double days)
        {
            if (date == null) return null;
            return date.Value.AddDays(days/8);
        }
        DateTime? DateSub(DateTime? date, double days)
        {
            if (date == null) return null;
            return date.Value.AddDays(-days/8);
        }

        public List<Task> GetTasks()
        {
            return Tasks.Values.ToList();
        }


    }
}
