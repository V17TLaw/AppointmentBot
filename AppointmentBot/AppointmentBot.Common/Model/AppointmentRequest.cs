using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppointmentBot.Common.Model
{
    [Serializable]
    public class AppointmentRequest
    {
        public TimeRange RequestedTimeRange = new TimeRange();

        public TimeSpan AppointmentDuration;

        public IList<Calendar> NeededCalendars = new List<Calendar>();

        public IEnumerable<TimeRange> PossibleAppointmentTimeRanges;
    }
}