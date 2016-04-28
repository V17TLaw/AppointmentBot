using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using AppointmentBot.Common.Model;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace AppointmentBot.Bot.Dialogs
{
    [Serializable]
    public class FindAppointmentDialog : IDialog<object>
    {
        private static string GetCalendarsUrl = "http://localhost:1115/api/Appointment/GetAvailableCalendars";

        private static IEnumerable<string> StartNewFindingProcessStrings = new List<string>()
        {
            "find",
            "start",
            "begin"
        };

        private AppointmentRequest appointmentRequest;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Message> argument)
        {
            var message = await argument;
            if (this.AnyOccurenceFound(message.Text, StartNewFindingProcessStrings))
            {
                this.appointmentRequest = new AppointmentRequest();

                PromptDialog.Text(context,
                    FindParticipants,
                    "For which calendars or persons do you want me to search for a possible appointment time?",
                    "Sorry, I didn't get the calendars you want me to search - please state your desired calendars again!");
            }
            else
            {
                await context.PostAsync("Sorry, I didn't get that - could you please state your desire again?");
                context.Wait(MessageReceivedAsync);
            }
        }

        public async Task FindParticipants(IDialogContext context, IAwaitable<string> argument)
        {
            var participantsAnswer = await argument;

            var calendars = await this.GetAvailableCalendars();

            var possibleParticipants = calendars.Select(c => c.Name).ToList();

            var participants = this.GetOccurencesFound(participantsAnswer, possibleParticipants);

            // await context.PostAsync("OK - I'll try to find appointments for the following list of calendars/persons: " + string.Join(", ", participants));

            foreach(var p in participants)
            {
                var calendarForParticipant = calendars.FirstOrDefault(c => c.Name.Equals(p, StringComparison.InvariantCultureIgnoreCase));
                if(calendarForParticipant != null)
                {
                    this.appointmentRequest.NeededCalendars.Add(calendarForParticipant);
                }
            }

            PromptDialog.Text(context,
                   FindDateTimeRange,
                   "What date or timerange would be appropriate for you?",
                   "Sorry, I didn't get that - please type in again!");
        }

        public async Task FindDateTimeRange(IDialogContext context, IAwaitable<string> argument)
        {
            var time = await argument;

            DateTime resultDateTime;

            if (time.ToLower().Contains("today"))
            {
                this.appointmentRequest.RequestedTimeRange.Start = DateTime.Now.Date;
                this.appointmentRequest.RequestedTimeRange.End = DateTime.Now.Date.AddDays(1).AddTicks(-1);
            }
            else if (time.ToLower().Contains("tomorrow"))
            {
                this.appointmentRequest.RequestedTimeRange.Start = DateTime.Now.Date.AddDays(1);
                this.appointmentRequest.RequestedTimeRange.End = DateTime.Now.Date.AddDays(2).AddTicks(-1);
            }
            else if (time.ToLower().Contains("this week"))
            {
                this.appointmentRequest.RequestedTimeRange.Start = DateTime.Now;
                this.appointmentRequest.RequestedTimeRange.End = this.GetNextWeekDay(DayOfWeek.Sunday).AddDays(1).AddTicks(-1);
            }
            else if (time.ToLower().Contains("next week"))
            {
                this.appointmentRequest.RequestedTimeRange.Start = this.GetNextWeekDay(DayOfWeek.Sunday).AddDays(1);
                this.appointmentRequest.RequestedTimeRange.End = this.appointmentRequest.RequestedTimeRange.Start.AddDays(7).AddTicks(-1);
            }
            else if (time.ToLower().Contains("this month"))
            {
                var now = DateTime.Now;

                this.appointmentRequest.RequestedTimeRange.Start = DateTime.Now;
                this.appointmentRequest.RequestedTimeRange.End = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month)).AddDays(1).AddTicks(-1);
            }
            else if (DateTime.TryParse(time, out resultDateTime))
            {
                this.appointmentRequest.RequestedTimeRange.Start = resultDateTime.Date;
                this.appointmentRequest.RequestedTimeRange.End = this.appointmentRequest.RequestedTimeRange.Start.AddDays(1).AddTicks(-1);
            }
            else
            {
                await context.PostAsync("Sorry, I didn't get that - could you please state your desired date or timerange again?");
                context.Wait(MessageReceivedAsync);
            }

            // await context.PostAsync("OK - I'll try to find appointments for the following range: " + this.appointmentRequest.RequestedTimeRange.Start.ToString("dd.MM.yyyy") + " - " + this.appointmentRequest.RequestedTimeRange.End.ToString("dd.MM.yyyy"));

            PromptDialog.Text(context,
                    FindAppointmentDuration,
                    "How long does the appointment take place?",
                    "Sorry, I didn't get that - please type in again!");
        }

        public async Task FindAppointmentDuration(IDialogContext context, IAwaitable<string> argument)
        {
            var time = await argument;

            // TODO - check durations


            await context.PostAsync("OK - I'll try to find appointments for the following range: " + this.appointmentRequest.RequestedTimeRange.Start.ToString("dd.MM.yyyy") + " - " + this.appointmentRequest.RequestedTimeRange.End.ToString("dd.MM.yyyy"));


            context.Wait(MessageReceivedAsync);
        }

        private DateTime GetNextWeekDay(DayOfWeek day)
        {
            var date = DateTime.Now;

            while(date.DayOfWeek != day)
            {
                date = date.AddDays(1);
            }

            return date.Date;
        }

        private bool AnyOccurenceFound(string inputString, IEnumerable<string> wordsToFind)
        {
            return this.GetOccurencesFound(inputString, wordsToFind).Any();
        }

        private IEnumerable<string> GetOccurencesFound(string inputString, IEnumerable<string> wordsToFind)
        {
            var occurencesFound = new List<string>();

            var messageParts = inputString.Split(new[] { " ", ",", "and" }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.ToLowerInvariant());

            foreach (var wordToFind in wordsToFind)
            {
                if (messageParts.Contains(wordToFind.ToLower()))
                {
                    occurencesFound.Add(wordToFind);
                }
            }

            return occurencesFound;
        }

        private async Task<IEnumerable<Calendar>> GetAvailableCalendars()
        {
            using (var client = new HttpClient())
            {
                var calendarsResultJson = await client.GetStringAsync(FindAppointmentDialog.GetCalendarsUrl);
                var result = JsonConvert.DeserializeObject<IEnumerable<Calendar>>(calendarsResultJson);

                return result;
            }
        }
    }
}