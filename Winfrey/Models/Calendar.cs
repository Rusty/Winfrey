using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winfrey.Models
{
    public class Calendar
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public double[] HoursPerDay { get; set; }
        public double HoursPerWeek { get; set; }

        public WorkingWeek StandardWorkWeek { get; set; }
        public CalendarExceptions HolidayOrExceptions { get; set; }
    }

    public class WorkingWeek
    {
        public List<CalendarWorkWeek> StandardWorkingHours { get; set; }    
    }
    
    public class CalendarExceptions
    {
        public List<CalendarException> HoildayOrException { get; set; }
    }

    public class CalendarWorkWeek
    {
        public string DayOfWeek { get; set; }
        public List<WorkTimeRange> WorkTime { get; set; }
    }
    public class CalendarException
    {
        public DateTime Date { get; set; }
        public List<WorkTimeRange> WorkTime { get; set; }
    }
    public class WorkTimeRange
    {
        public DateTime Start { get; set; }
        public DateTime Finish { get; set; }
    }

}
