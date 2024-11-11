using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Winfrey.Models.Link;

namespace Winfrey.Models
{
    public class LinkBackwards
    {
        public string PrecedingTaskId { get; set; }
        public double Lag { get; set; }
        public linkType Type { get; set; }

        public LinkBackwards(Link link) 
        {
            PrecedingTaskId = link.PrecedingTaskId;
            Lag = link.Lag;
            Type = link.Type;
        }
    }
}
