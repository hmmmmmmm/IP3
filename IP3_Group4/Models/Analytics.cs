using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IP3_Group4.Models
{

    // ViewModel for dashboard
    public class Analytics
    {
        public decimal WeekTotal { get; set; }  // Total spent by user over one week
        public decimal MonthTotal { get; set; } // Total spent by user over one month
        public List<ShopCounter> Shops { get; set; } // Stores the shops the shops the user has shopped at
        public List<ProductCounter> Products { get; set; } // Stores the products the user has bought
        public Receipt LastReceipt { get; set; } // Stores the last receipt scanned by user
        public decimal RemainingBudget { get; set; }


        public Analytics(List<Receipt> receipts, Budget budge)
        {

            if (receipts.Any())
            {
                LastReceipt = receipts.Last();
                DateTime weekStart = DateTime.Now.AddDays(-7); // gets date of 7 days ago
                DateTime monthStart = DateTime.Now.AddDays(-30); // gets date of 30 days ago

                List<ShopCounter> shops = new List<ShopCounter>(); // creates list to be used to store different shops and how many visits
                List<ProductCounter> products = new List<ProductCounter>();
                int totalProds = 0;

                if (budge != null)
                    RemainingBudget = budge.Amount;

                foreach (Receipt r in receipts) // loops through all receipts
                {
                    if (r.PurchaseDate >= monthStart)
                    {
                        MonthTotal += r.TotalPrice;

                        if (r.PurchaseDate >= weekStart)
                            WeekTotal += r.TotalPrice;
                    }

                    if (budge != null && r.PurchaseDate >= budge.LastReset)
                        RemainingBudget -= r.TotalPrice;
                    

                    int sIndex = shops.FindIndex(s => s.Shop == r.Shop);
                    if (sIndex >= 0)
                    {
                        shops[sIndex].Visits++;
                    }
                    else
                    {
                        shops.Add(new ShopCounter(r.Shop));
                    }

                    foreach (ProductLine pl in r.ProductLines)
                    {
                        int pIndex = products.FindIndex(p => p.Product == pl.ItemName);
                        if (pIndex >= 0)
                        {
                            products[pIndex].Buys += pl.Quantity;
                        }
                        else
                        {
                            products.Add(new ProductCounter(pl.ItemName));
                        }

                        totalProds += pl.Quantity;
                    }
                }

                Shops = shops.OrderBy(s => s.Visits).ToList();
                Products = products.OrderBy(p => p.Buys).ToList();

                if (totalProds != 0)
                {
                    foreach (ProductCounter pc in products)
                        pc.GetPercentOfBought(totalProds);
                }
                //if ()
            }
            else
            {
                Shops = new List<ShopCounter> { new ShopCounter("No shops visited yet...") };
                Products = new List<ProductCounter> { new ProductCounter("No products bought yet...") };
                MonthTotal = 0; WeekTotal = 0;
            }
        }
    }

    // counts total visits to a shop
    public class ShopCounter
    {
        public string Shop { get; set; }
        public int Visits { get; set; }

        public ShopCounter()
        {
            Shop = "No shops visited...";
            Visits = 0;
        }

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

    // counts total number of a product bought
    public class ProductCounter
    {
        public string Product { get; set; }
        public int Buys { get; set; }
        public double PercentageOfItemsBought { get; set; }

        public ProductCounter(string product)
        {
            Product = product;
            Buys = 1;
        }

        public ProductCounter(string product, int buys)
        {
            Product = product;
            Buys = buys;
        }
        public void GetPercentOfBought(int totalProds)
        {
            PercentageOfItemsBought = Math.Round((double) Buys / totalProds, 2);
        }
    }
}