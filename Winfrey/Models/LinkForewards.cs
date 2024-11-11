using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Winfrey.Models.Link;

namespace Winfrey.Models
{
    public class LinkForewards
    {
        public string SucceedingTaskId { get; set; }
        public double Lag { get; set; }
        public linkType Type { get; set; }
        public LinkForewards(Link link)
        {
            SucceedingTaskId = link.SucceedingTaskId;
            Lag = link.Lag;
            Type = link.Type;
        }

    }
}
