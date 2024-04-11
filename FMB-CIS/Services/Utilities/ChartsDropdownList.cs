using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Services.Utilities
{
    public class ChartsDropdownList
    {
        public IEnumerable<SelectListItem> Months
        {
            get
            {
                int curMonth = DateTime.Now.Month;
                return
                    Enumerable.Range(1, 12)
                    .Select(x => new SelectListItem
                    {
                        Value = x.ToString("0"),
                        Text = DateTimeFormatInfo.CurrentInfo.GetMonthName(x),
                        Selected = (x == curMonth)
                    });
            }
        }

        public IEnumerable<SelectListItem> Years
        {
            get
            {
                int curYear = DateTime.Now.Year;
                return Enumerable.Range(DateTime.Now.Year - 10, 11)
                    .Select(x => new SelectListItem 
                    { 
                        Value = x.ToString("0"), 
                        Text = x.ToString("0"), 
                        Selected = (x == curYear) 
                    }).Reverse();
            }
        }
    }
}
