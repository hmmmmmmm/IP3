using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IP3_Group4.Models
{
    public class Analytics
    {
        public decimal WeekTotal { get; set; } // Total spent by user over one week
        public decimal MonthTotal { get; set; } // Total spent by user over one month
        public List<ShopCounter> Shops { get; set; } // Stores the shop the user shopped at the most
        public Receipt LastReceipt { get; set; } // Stores the last receipt scanned by user

        public Analytics(List<Receipt> receipts)
        {

            if (receipts.Any())
            {
                LastReceipt = receipts.Last();
                DateTime weekStart = DateTime.Now.AddDays(-7); // gets date of 7 days ago
                DateTime monthStart = DateTime.Now.AddDays(-30); // gets date of 30 days ago
                List<ShopCounter> shops = new List<ShopCounter>(); // creates list to be used to store different shops and how many visits
                foreach (Receipt r in receipts) // loops through all receipts
                {
                    if (r.PurchaseDate >= monthStart)
                    {
                        MonthTotal += r.TotalPrice;

                        if (r.PurchaseDate >= weekStart)
                            WeekTotal += r.TotalPrice;
                    }

                    int sIndex = shops.FindIndex(s => s.Shop == r.Shop);
                    if (sIndex >= 0)
                    {
                        shops[sIndex].Visits++;
                    }
                    else
                    {
                        shops.Add(new ShopCounter(r.Shop));
                    }
                }

                Shops = shops.OrderBy(s => s.Visits).ToList();
            }
            else
            {
                Shops = new List<ShopCounter>
                {
                    new ShopCounter("No shops yet", 0)
                };
                MonthTotal = 0; WeekTotal = 0;
            }
        }
    }

    public class ShopCounter
    {
        public string Shop { get; set; }
        public int Visits { get; set; }

        public ShopCounter(string shop)
        {
            Shop = shop;
            Visits = 1;
        }

        public ShopCounter(string shop, int visits)
        {
            Shop = shop;
            Visits = visits;
        }
    }
}