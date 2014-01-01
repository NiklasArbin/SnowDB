using System;

namespace Snow.Core
{
    public class DateTimeNow : IDateTimeNow
    {
        public DateTime Now { get { return DateTime.Now; } }
    }
}