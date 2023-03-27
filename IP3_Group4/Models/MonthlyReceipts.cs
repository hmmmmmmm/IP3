using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IP3_Group4.Models
{
    public class MonthlyReceipts
    {
        public DateTime Month { get; set; }
        public List<DailyReceipts> DailyReceipts { get; set; }
        public int[] DaysInMonth { get {
                int[] daysInMonth = new int[31];
                for (int i = 0; i < 31; i++) 
                {
                    daysInMonth[i] = i+1;
                }
                return daysInMonth;
            }
        }
        public decimal Spent
        {
            get
            {
                decimal t = 0;
                foreach (DailyReceipts r in DailyReceipts)
                {
                    t += r.Spent;
                }
                return t;
            }
        }     

        public MonthlyReceipts(DateTime month, List<Receipt> receipts)
        {
            DailyReceipts = new List<DailyReceipts>(DateTime.DaysInMonth(Month.Year, Month.Month));

            for (int i = 0; i < DateTime.DaysInMonth(Month.Year, Month.Month); i++)
            {
                DailyReceipts.Add(new DailyReceipts(new DateTime(Month.Year, Month.Month, i + 1)));
            }
            
            Month = month;

            foreach (Receipt r in receipts)
            {
                DailyReceipts[r.PurchaseDate.Day - 1].Receipts.Add(r);
            }
            
        }
    }

    public class DailyReceipts
    {
        public DateTime Day { get; set; }
        public List<Receipt> Receipts { get; set; }
        public decimal Spent
        {
            get
            {
                decimal t = 0;
                foreach (Receipt r in Receipts)
                {
                    t += r.TotalPrice;
                }
                return t;
            }
        }

        public DailyReceipts(DateTime day)
        {
            Receipts = new List<Receipt>();
            Day = day;
        }
    }
}