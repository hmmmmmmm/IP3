using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using System.Web.Security;
using IP3_Group4.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;
using System.Web.WebPages;

namespace IP3_Group4.Data
{
    public class DBInit : DropCreateDatabaseAlways<DBContext>
    {
        protected override void Seed(DBContext context)
        {
            base.Seed(context);

            // seed payment types
            string[] paymenttypes = { "Card", "Cash", "GiftCard" };
            foreach (var type in paymenttypes)
            {
                var pt = new PaymentType(type);
                context.PaymentTypes.Add(pt);
            }

            // seeds a sample user
            if (!(context.Users.Any(u => u.UserName == "test@receiptr.com")))
            {
                var userStore = new UserStore<User>(context);
                var userManager = new UserManager<User>(userStore);
                var userToInsert = new User { UserName = "test@receiptr.com", Email = "test@receiptr.com" };
                userManager.Create(userToInsert, "Test!23");
            }

            // seed receipt templates
            ReceiptTemplate bandm = new ReceiptTemplate("b&m retail ltd", "customer copy", " item", " x ", "paid by", "customer receipt");
            ReceiptTemplate sainsburys = new ReceiptTemplate("sainsbury's supermarkets ltd", "vat number: ", " balance due", " @ ", " balance due", ""); // little funky
            context.ReceiptTemplates.Add(bandm);
            context.ReceiptTemplates.Add(sainsburys);


            // seed receipts
            string testUserID = context.Users.First(u => u.Email == "test@receiptr.com").Id; // gets test user
            if (!testUserID.IsEmpty())
            {
                Receipt r1 = new Receipt(
                    "Sainsbury's Supermarkets Ltd",
                    DateTime.Now,
                    new List<ProductLine>(),
                    testUserID
                );

                Receipt r2 = new Receipt(
                    "Sainsbury's Supermarkets Ltd",
                    DateTime.Now,
                    new List<ProductLine>
                    {
                        new ProductLine()
                        {
                            ItemName = "Cheese Slices",
                            Quantity = 1,
                            Price = (decimal) 1.50,
                            Brand = "Sainsbury's"
                        },
                        new ProductLine()
                        {
                            ItemName = "Fresh Roll",
                            Quantity = 6,
                            Price = (decimal) 0.50,
                            Brand = "Sainsbury's"
                        },
                        new ProductLine()
                        {
                            ItemName = "Ketchup",
                            Quantity = 1,
                            Price = (decimal) 1.00,
                            Brand = "Heinz"
                        },
                        new ProductLine()
                        {
                            ItemName = "2pt Milk",
                            Quantity = 1,
                            Price = (decimal) 2.00,
                            Brand = "Sainsbury's"
                        },
            },
                    testUserID
                );

                context.Receipts.Add(r1);
                context.Receipts.Add(r2);
                context.SaveChanges();

                r1 = context.Receipts.ToList().Last(r => r.UserID == r1.UserID);
                List<ProductLine> pls1 = new List<ProductLine>
                    {
                        new ProductLine()
                        {
                            ItemName = "Strawberries",
                            Quantity = 1,
                            Price = (decimal) 1.50,
                            Brand = "Sainsbury's",
                            ReceiptID = r1.ID
                        },
                        new ProductLine()
                        {
                            ItemName = "Ramen Noodles",
                            Quantity = 3,
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
                foreach (ProductLine pl in pls1)
                    context.ProductLine.Add(pl);

                r2 = context.Receipts.ToList().Last(r => r.UserID == r2.UserID);
                List<ProductLine> pls2 = new List<ProductLine>
                    {
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
                foreach (ProductLine pl in pls2)
                    context.ProductLine.Add(pl);
            } else
            {
                System.Diagnostics.Debug.WriteLine("!!! couldn't find test user !!!");
            }

            context.SaveChanges();
        }
    }
}