using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Services.Utilities
{
    public static class DateManipulation
    {
        static List<Week> GetWeeks(int year, DayOfWeek firstDayOfWeek = DayOfWeek.Monday)
        {
            var jan1 = new DateTime(year, 1, 1);
            var dec31 = new DateTime(year, 12, 31);
            var startOfFirstWeek = jan1.AddDays(firstDayOfWeek - jan1.DayOfWeek);

            return Enumerable.Range(0, 54)
                .Select(i => new Week
                {
                    StartDate = i == 0
                        ? jan1
                        : startOfFirstWeek.AddDays(i * 7),
                    EndDate = startOfFirstWeek.AddDays(i * 7 + 6).Year == year
                        ? startOfFirstWeek.AddDays(i * 7 + 6)
                        : dec31,
                    Number = i + 1
                })
                .TakeWhile(week => week.StartDate.Year == year)
                .ToList();
        }
    }

    public class Week
    {
        public int Number { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
