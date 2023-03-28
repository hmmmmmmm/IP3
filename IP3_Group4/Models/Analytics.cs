﻿using System;
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
        public List<MonthlyReceipts> MonthlyReceipts { get; set; }
        public List<Receipt> AllReceipts { get; set; }
        public decimal RemainingBudget { get; set; } // Amount left in the budget after deducting total of all user's receipts
        public decimal TotalBudget { get; set; }
        public int DaysLeftInBudget { get; set; } = 0; // number of days left in the budget
        public decimal SpentLastMonth { get; set; }


        public Analytics(List<Receipt> receipts, Budget budge)
        {
            if (receipts.Any()) // checks user has actually scanned a receipt
            {
                AllReceipts = receipts;
                WeekTotal = 0;
                MonthTotal = 0;

                DateTime weekStart = DateTime.Now.AddDays(-7); // gets date of 7 days ago
                DateTime monthStart = DateTime.Now.AddDays(-30); // gets date of 30 days ago

                List<ShopCounter> shops = new List<ShopCounter>(); // creates list to be used to store different shops and how many visits
                List<ProductCounter> products = new List<ProductCounter>(); // creates list to be used to store different products and how many times they've been bought
                int totalProds = 0; // the total number of products user has bought

                if (budge != null) // checks if user has set up their budget
                {
                    TotalBudget = budge.Amount;
                    RemainingBudget = budge.Amount; // sets current remaining budget to the budgets amount (calculations done further down)
                    DaysLeftInBudget = (int)(budge.NextReset.Date - DateTime.Now.Date).TotalDays;
                }
                else
                {
                    RemainingBudget = -1;
                }


                foreach (Receipt r in receipts) // loops through all user's receipts
                {
                    if (r.PurchaseDate >= monthStart) // checks if the receipt was printed within the last 30 days
                    {
                        MonthTotal += r.TotalPrice; // if yes, adds that to the total spent across the month

                        if (r.PurchaseDate >= weekStart) // checks if receipt was printed within the last week
                        {
                            WeekTotal += r.TotalPrice; // if yes, adds that to the total spent across the week
                        }
                    }

                    if (budge != null && r.PurchaseDate.Date >= budge.LastReset.Date) // checks budget isnt null again and checks receipt was printed since last reset
                    {
                        RemainingBudget -= r.TotalPrice; // subtracts receipt's amount from amount left in budget
                    }

                    int sIndex = shops.FindIndex(s => s.Shop == r.Shop); // tries to locate the shop the receipt was bought from within list of shops
                    if (sIndex >= 0) // if the shop has already been added to the list of shops
                    {
                        shops[sIndex].Visits++; // increments number of visits by one
                        shops[sIndex].Spent += r.TotalPrice;
                    } else // if the shop hasn't already been added
                    {
                        shops.Add(new ShopCounter(r.Shop, r.TotalPrice)); // adds the shop to the list (with default visits value of one)
                    }
                        

                    foreach (ProductLine pl in r.ProductLines) // loops through all the productlines in the receipt
                    {
                        int pIndex = products.FindIndex(p => p.Product == pl.ItemName); // checks to see if this product has been located in a previous receipt
                        if (pIndex >= 0) // if it has been in a previous receipt
                        {
                            products[pIndex].Buys += pl.Quantity; // increases the number of buys by quantity in productline
                        }
                            
                        else // if it hasnt been in a previous receipt
                        {
                            products.Add(new ProductCounter(pl.ItemName, pl.Quantity, pl.Price)); // adds the product to the list of found products with the productline's quantity
                        }
                            

                        totalProds += pl.Quantity;
                    }
                }

                MonthlyReceipts = new List<MonthlyReceipts>();
                DateTime startPeriod = DateTime.Now.AddYears(-1);

                for (int i = 0; i < 12; i++)
                {
                    List<Receipt> thisMonthsReceipts = new List<Receipt>();
                    DateTime currentMonthStart = startPeriod.AddMonths(i + 1);
                    foreach (Receipt r in AllReceipts)
                    {
                        if (r.PurchaseDate.Month == currentMonthStart.Month)
                        {
                            thisMonthsReceipts.Add(r);
                        }
                    }

                    MonthlyReceipts.Add(new MonthlyReceipts(startPeriod.AddMonths(i + 1), thisMonthsReceipts));
                }

                MonthlyReceipts.Reverse();

                Shops = shops.OrderBy(s => s.Visits).ToList(); // orders the shops by least to most popular
                Shops.Reverse();


                if (products.Count != 0) // checks if any products have been found
                {
                    Products = products.OrderBy(p => p.Buys).ToList(); // orders the products by least to most bought
                    Products.Reverse(); // reverses order so Products[0] is always the most bought

                    foreach (ProductCounter pc in products) // loops through each product found
                        pc.GenerateStats(totalProds); // calculates what percentage of the total bought products this product is
                }

            }
            else // if there are no receipts scanned in
            {
                Shops = new List<ShopCounter> { new ShopCounter("No shops visited yet...", 0) }; // creates dummy ShopCounter
                Products = new List<ProductCounter> { new ProductCounter("No products bought yet...", 0, 0) }; // creates dummy ProductCounter
                MonthTotal = 0; WeekTotal = 0; // sets week and month totals to 0
                RemainingBudget = -1;
                AllReceipts = new List<Receipt>();
            }
        }
    }

    // counts total visits to a shop
    public class ShopCounter
    {
        public string Shop { get; set; } // the name of the shop
        public int Visits { get; set; } // the number of visits to that shop
        public decimal Spent { get; set; } = 0;

        public ShopCounter(string shop, decimal spent) // usual constructor
        {
            Shop = shop; // sets the shop name
            Visits = 1; // when the first receipt from that shop is scanned, that must be the first visit
            Spent = spent;
        }

        public ShopCounter(string shop, int visits) // used when no receipts have been scanned
        {
            Shop = shop; // sets the shop name
            Visits = visits; // sets the number of visits to that shop
        }
    }

    // counts total number of a product bought
    public class ProductCounter
    {
        public string Product { get; set; } // name of the product
        public int Buys { get; set; } // number of times user has bought it
        public double PercentageOfItemsBought { get; set; } // what percentage of all products bought is this product bought
        public decimal ItemPrice { get; set; } // the price for one of this item
        public decimal TotalSpent { get { return ItemPrice * Buys; } } // the total amount spent on this item

        public ProductCounter(string product, int buys, decimal itemPrice) // only needed constructor. always passing in 0 or the quantity of a product so no default value
        {
            Product = product; // sets name of product
            Buys = buys; // sets number of buys
            ItemPrice = itemPrice;
        }

        public void GenerateStats(int totalProds) // calculates the percentage of products bought this product is
        {
            PercentageOfItemsBought = Math.Round((double)Buys / totalProds, 2); // does the maths and rounds to two decimal places
        }
    }
}