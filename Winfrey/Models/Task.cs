using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winfrey.Models
{
    public class Task
    {
        public enum taskType
        {
            task, startMilestone, finishMilestone, hammock, wbs
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public taskType Type { get; set; }

        public double RemaningDuration { get; set; }

        public string CalendarId { get; set; }

        public List<LinkBackwards> BRels { get; set; }
        public List<LinkForewards> FRels { get; set; }

        public DateTime? ES { get; set; } // Earkly Start
        public DateTime? EF { get; set; } // Early Finish
        public DateTime? LS { get; set; } // Late Start
        public DateTime? LF { get; set; } // Late Finish

        public double FF { get; set; }  // Free Float
        public double TF { get; set; } // Total Float
        public bool isCritical => TF <= 0;

        public DateTime? Mid => ES.Value + (EF.Value - ES.Value) / 2;

        // for testing in the datagrid
        public bool IsStart { get; set; }
        public bool IsFinish { get; set; }

        public int Row { get; set; }

        public string Suc
        {
            get
            {
                var s = "";
                foreach(var r in FRels)
                {
                    s += r.SucceedingTaskId + ", ";
                }
                return s;
            }
        }

        public Task() 
        { 
            Name = "NewTask";
            Type = taskType.task;
            BRels = new List<LinkBackwards>();
            FRels = new List<LinkForewards>();
        }

        public void Reset(bool full = false)
        {
            if (full)
            {
                BRels.Clear();
                FRels.Clear();
            }
            ES = null;
            EF = null;
            LS = null;
            LF = null;
            FF = -1;
            TF = -1;    
            IsStart = false;
            IsFinish = false;
        }
    }
}
