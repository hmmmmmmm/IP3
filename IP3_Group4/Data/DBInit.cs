using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using IP3_Group4.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Web.WebPages;

namespace IP3_Group4.Data
{
    public class DBInit : DropCreateDatabaseIfModelChanges<DBContext>
    {
        protected override void Seed(DBContext context) // seeds data into the database
        {
            base.Seed(context); // calls the base seed method

            // seed payment types
            context.PaymentTypes.Add(new PaymentType("Card")); // seeds card payment type
            context.PaymentTypes.Add(new PaymentType("Cash")); // seeds cash payment type
            context.PaymentTypes.Add(new PaymentType("GiftCard")); // seeds GiftCard payment type

            // seeds a sample user
            if (!(context.Users.Any(u => u.UserName == "test@receiptr.com"))) // checks the sample user doesnt already exist
            {
                var userStore = new UserStore<User>(context); // creates a userStore to get access to UserManager
                var userManager = new UserManager<User>(userStore); // create a UserManager to get access to users
                var userToInsert = new User { UserName = "test@receiptr.com", Email = "test@receiptr.com" }; // creates the basic user
                userManager.Create(userToInsert, "Test!23"); // creates the new user in the UserManager with the object and a password
            }

            // seed receipt templates
            ReceiptTemplate bandm = new ReceiptTemplate("b&m retail ltd", "customer copy", " item", " x ", "paid by", "customer receipt"); // seeds B&M receipt template
            ReceiptTemplate sainsburys = new ReceiptTemplate("sainsbury's supermarkets ltd", "vat number: ", " balance due", " @ ", " balance due", ""); // seeds Sainsburys receipt template
            context.ReceiptTemplates.Add(bandm); // queues B&M template for database
            context.ReceiptTemplates.Add(sainsburys); // queues Sainsbury's template for database

            // seed receipts
            var testUser = context.Users.First(u => u.Email == "test@receiptr.com"); // gets test user's id
            var paymentType = context.PaymentTypes.First(p => p.Type == "card");

            if (testUser != null) // checks the test user was found
            {
                Receipt r1 = new Receipt( // creates new receipt object
                    "Sainsbury's Supermarkets Ltd", // passes Sainsburys to new receipt
                    DateTime.Now, // passes now as the purchase date
                    new List<ProductLine>(), // passes an empty list of productlines
                    testUser, // passes the id of the test user
                    paymentType
                );
                context.Receipts.Add(r1); // adds the receipt to database queue
                context.SaveChanges(); // saves all database changes
                r1 = context.Receipts.ToList().Last(r => r.UserID == r1.UserID); // gets the receipt back (now with an ID)
                List<ProductLine> productLines1 = new List<ProductLine> // creates a list of product lines
                    {
                        new ProductLine() // new product line
                        {
                            ItemName = "Strawberries", // setting line's product name
                            Quantity = 1, // setting line's quantity
                            Price = (decimal) 1.50, // setting line's price (per unit)
                            Brand = "Sainsbury's", // setting line's brand
                            ReceiptID = r1.ID // setting line's receipt ID
                        },
                        new ProductLine() // new product line
                        {
                            ItemName = "Ramen Noodles", // setting line's product name
                            Quantity = 3, // setting line's quantity .... i think u get it
                            Price = (decimal) 1.00,
                            Brand = "Pot Noodle",
                            ReceiptID = r1.ID
                        },
                        new ProductLine()
                        {
                            ItemName = "Cheese Slices",
                            Quantity = 1,
                            Price = (decimal) 1.50,
                            Brand = "Sainsbury's",
                            ReceiptID = r1.ID
                        },
                        new ProductLine()
                        {
                            ItemName = "Fresh Roll",
                            Quantity = 6,
                            Price = (decimal) 0.50,
                            Brand = "Sainsbury's",
                            ReceiptID = r1.ID
                        },
                        new ProductLine()
                        {
                            ItemName = "Ketchup",
                            Quantity = 1,
                            Price = (decimal) 1.00,
                            Brand = "Heinz",
                            ReceiptID = r1.ID
                        }
                    };
                foreach (ProductLine pl in productLines1) // loops through newly created productlines
                    context.ProductLine.Add(pl); // adds those lines to the database queue
                r1.CalculateTotal(); // calculates the total price of the receipt
                


                Receipt r2 = new Receipt( // creates new receipt
                    "Sainsbury's Supermarkets Ltd", // passes Sainsburys as shop
                    DateTime.Now.AddDays(-2), // passes two days ago as purchase date
                    new List<ProductLine>(), // passes empty list of product lines
                    testUser, // passes user's id
                    paymentType
                );
                context.Receipts.Add(r2); // queues receipt for database
                context.SaveChanges(); // adds to database
                r2 = context.Receipts.ToList().Last(r => r.UserID == r2.UserID); // gets the receipt back (now with an ID)
                List<ProductLine> productLines2 = new List<ProductLine> // creates new list of productlines
                    {
                    // it's 1:17am, I'm not writing the same comments for all this lol
                        new ProductLine()
                        {
                            ItemName = "Cheese Slices",
                            Quantity = 1,
                            Price = (decimal) 1.50,
                            Brand = "Sainsbury's",
                            ReceiptID = r2.ID
                        },
                        new ProductLine()
                        {
                            ItemName = "Fresh Roll",
                            Quantity = 6,
                            Price = (decimal) 0.50,
                            Brand = "Sainsbury's",
                            ReceiptID = r2.ID
                        },
                        new ProductLine()
                        {
                            ItemName = "Ketchup",
                            Quantity = 1,
                            Price = (decimal) 1.00,
                            Brand = "Heinz",
                            ReceiptID = r2.ID
                        },
                        new ProductLine()
                        {
                            ItemName = "2pt Milk",
                            Quantity = 1,
                            Price = (decimal) 2.00,
                            Brand = "Sainsbury's",
                            ReceiptID = r2.ID
                        }
                    };
                foreach (ProductLine pl in productLines2) // loops through new productlines
                    context.ProductLine.Add(pl); // queues productlines for database
                r2.CalculateTotal(); // calculates receipt's new total

            } else // if the test user couldnt be retrieved
            {
                System.Diagnostics.Debug.WriteLine("Unable to find test user!"); // writes error to output tab
            }

            context.SaveChanges(); // saves all database changes
        }
    }
}