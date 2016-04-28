using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using AppointmentBot.Common.Model;

namespace AppointmentBot.Service.Controllers.Api
{
    [RoutePrefix("api/appointment")]
    public class AppointmentController : ApiController
    {
        [HttpGet]
        [Route("GetAvailableCalendars")]
        public async Task<IEnumerable<Calendar>> GetAvailableCalendars()
        {
            return new [] {
                new Calendar {
                    Name = "Roman",
                    Identifier = Guid.Parse("381f33ca-274b-4d9d-9528-163773f4e44e")
                },
                new Calendar {
                    Name = "Daniel",
                    Identifier = Guid.Parse("f0815f6c-520e-4e96-9e4d-648b5ced7405")
                },
                new Calendar {
                    Name = "Philipp",
                    Identifier = Guid.Parse("55e5db6c-002e-4cf3-a53a-3dea35c8b842")
                }
            };
        }

        [HttpPost]
        [Route("GetPossibleAppointmentsInTimeRange")]
        public async Task<IEnumerable<TimeRange>> GetPossibleAppointmentsInTimeRange(AppointmentRequest appointmentRequest)
        {
            return new[] {
               new TimeRange()
               {
                   Start = new DateTime(2016,28,4,15,0,0),
                   End = new DateTime(2016,28,4,16,0,0)
               },
                new TimeRange()
               {
                   Start = new DateTime(2016,29,4,10,0,0),
                   End = new DateTime(2016,29,4,12,0,0)
               }
            };
        }

        [HttpGet]
        [Route("GetPossibleAppointmentsInTimeRangeTest")]
        public async Task<IEnumerable<TimeRange>> GetPossibleAppointmentsInTimeRange()
        {
            return new[] {
               new TimeRange()
               {
                   Start = new DateTime(2016,4,28,15,0,0),
                   End = new DateTime(2016,4,28,16,0,0)
               },
                new TimeRange()
               {
                   Start = new DateTime(2016,4,29,10,0,0),
                   End = new DateTime(2016,4,29,12,0,0)
               }
            };
        }
    }
}