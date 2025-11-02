namespace AknaLoad.Domain.ValueObjects
{
    public class WorkingHours
    {
        public TimeSlot Monday { get; set; } = new();
        public TimeSlot Tuesday { get; set; } = new();
        public TimeSlot Wednesday { get; set; } = new();
        public TimeSlot Thursday { get; set; } = new();
        public TimeSlot Friday { get; set; } = new();
        public TimeSlot Saturday { get; set; } = new();
        public TimeSlot Sunday { get; set; } = new();

        public bool IsAvailableAt(DateTime dateTime)
        {
            var timeSlot = GetTimeSlotForDay(dateTime.DayOfWeek);
            if (!timeSlot.IsWorkingDay)
                return false;

            var timeOfDay = dateTime.TimeOfDay;
            return timeOfDay >= timeSlot.StartTime && timeOfDay <= timeSlot.EndTime;
        }

        private TimeSlot GetTimeSlotForDay(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => Monday,
                DayOfWeek.Tuesday => Tuesday,
                DayOfWeek.Wednesday => Wednesday,
                DayOfWeek.Thursday => Thursday,
                DayOfWeek.Friday => Friday,
                DayOfWeek.Saturday => Saturday,
                DayOfWeek.Sunday => Sunday,
                _ => new TimeSlot()
            };
        }
    }

    public class TimeSlot
    {
        public TimeSpan StartTime { get; set; } = new(0, 0, 0);
        public TimeSpan EndTime { get; set; } = new(23, 59, 59);
        public bool IsWorkingDay { get; set; } = true;

        public TimeSlot() { }

        public TimeSlot(TimeSpan startTime, TimeSpan endTime, bool isWorkingDay = true)
        {
            StartTime = startTime;
            EndTime = endTime;
            IsWorkingDay = isWorkingDay;
        }
    }
}