using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winfrey.Models
{
    public class Link
    {   
        public enum linkType
        {
            FS, SS, SF, FF
        }
        
        public double Lag { get; set; }
        public linkType Type { get; set; }
        public string PrecedingTaskId { get; set; }
        public string SucceedingTaskId { get; set; }


        public Link() 
        { 
            Type = linkType.FS;
        }
    }
}
