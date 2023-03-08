using Google.Cloud.Vision.V1;
using IP3_Group4.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using IP3_Group4.Data;
using System.Web.Caching;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;

namespace IP3_Group4.Controllers
{
    public class ReceiptController : Controller
    {
        // Used to handle Views relating to the uploading, editing, and viewing of receipts
        DBContext db = new DBContext();

        // GET: Vision
        [HttpGet]
        public ActionResult UploadReceipt()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadReceipt(HttpPostedFileBase file)
        {
            try
            {
                if (file != null) // checks a file was uploaded
                {
                    

                    string filePath = Server.MapPath(Url.Content("~/ReceiptR-key.json")); // sets location of the credentials key

                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", filePath); // sets credentials key to retrieved path

                    //System.Drawing.Image source = System.Drawing.Image.FromStream(file.InputStream, true, true);
                    Google.Cloud.Vision.V1.Image img = new Google.Cloud.Vision.V1.Image(); // creates image object to store receipt image for API
                    img = Google.Cloud.Vision.V1.Image.FromStream(file.InputStream); // retrieves uploaded image from file upload and sets to API image format

                    ImageAnnotatorClient client = ImageAnnotatorClient.Create(); // creates connection to API
                    var response = client.DetectText(img); // calls method to detect text within image

                    Receipt receipt = new Receipt(); // creates receipt object to fill
                    bool hasProds = false; // tells app if it already has accessed the products list.

                    // for debugging: prints API output line by line to console
                    System.Diagnostics.Debug.WriteLine(response[0].Description);

                    // creates a list of strings equal to the output of the API's first index (the entire receipt in a usable format)
                    List<string> lines = response[0].Description.Split('\n').ToList<string>();
                    ReceiptTemplate template;


                    if (lines.Contains("B&M Retail Ltd"))
                    {
                        template = db.ReceiptTemplates.First(rt => rt.Shop == "b&m retail ltd");
                        receipt.Shop = "B&M Retail Ltd"; // sets name of the shop bought from
                    } else if (lines.Contains("Sainsbury's Supermarkets Ltd"))
                    {
                        template = db.ReceiptTemplates.First(rt => rt.Shop == "sainsbury's supermarkets ltd");
                        receipt.Shop = "Sainsbury's Supermarkets Ltd"; // sets name of the shop bought from
                    } else
                    {
                        throw new Exception("No attributed template");
                    }

                    if (template != null)
                    {
                        // loops through every line in the scanned receipt
                        for (int i = 0; i < lines.Count; i++)
                        {
                            // checks if scanner has reached the start of the products list
                            if (!hasProds && lines[i].ToLower().Contains(template.ProductStartPrompt))
                            {

                                
                                hasProds = true; // tells app it has found the list of products


                                i++; // skips to line after products list start marker
                                do
                                {
                                    ProductLine pl = new ProductLine() { ItemName = lines[i] }; // creates ProductLine object to be filled

                                    if (lines[++i].ToLower().Contains(template.QuantityLineFormat)) // checks if the next line is a quantity line
                                    {
                                        string[] quantArr = lines[i].Split(' '); // splits quantity line into sections: 0 = quantity, 2 = price per item
                                        pl.Quantity = int.Parse(quantArr[0]); // sets quantity in ProductLine
                                        pl.Price = decimal.Parse(quantArr[2].Replace("£", "")); // sets price per item in ProductLine, also removes an pound signs
                                        i++; // skips to next line so quantity line isnt added as a product
                                    }
                                    else // if there is no quantity line for the product
                                    {
                                        pl.Quantity = 1; // sets quantity to 1
                                    }

                                    receipt.ProductLines.Add(pl); // adds ProductLine to receipt object
                                    

                                } while (!lines[i].ToLower().Contains(template.ProductEndPrompt)); // checks for end of product list

                                continue;
                            }

                            //// checks if next line is the payment method
                            //if (template.PaymentTypePrompt != "" && lines[i].ToLower().Contains(template.PaymentTypePrompt))
                            //{
                            //    i++;

                            //    if (lines[i].ToLower().Contains("card") || lines[i].ToLower().Contains("visa") || lines[i].ToLower().Contains("mastercard")) // if payment type is card, deals with it
                            //    {
                            //        receipt.PaymentType = db.PaymentTypes.First(pt => pt.Type == "Card");

                            //    }
                            //    else if (lines[i].ToLower().Contains("cash")) // if payment type is cash, deals with it
                            //    {
                            //        receipt.PaymentType = db.PaymentTypes.First(pt => pt.Type == "Cash");
                            //    }
                            //    else // if payment type is neither, deals with it
                            //    {
                            //        throw new Exception("PaymentType Not Found");
                            //    }

                            //    continue;
                            //}

                            // checks if next line is the Date/Time of purchase
                            if (template.DateTimePrompt != "" && lines[i].ToLower().Contains(template.DateTimePrompt))
                            {
                                DateTime dt = DateTime.Parse(lines[++i].ToLower()); // retrieves the Date/Time of the purchase
                                receipt.PurchaseDate = dt; // sets the receipt DateTime to string just read in
                            }

                            // checks if the line is a price or not, and starts getting prices if they are
                            if (lines[i].ToLower().Contains("£"))
                            {
                                for (int j = 0; j < receipt.ProductLines.Count; j++) // loops through all read-in products
                                {
                                    if (receipt.ProductLines[j].Price == 0) // checks the ProductLine doesn't already have a price attached
                                    {
                                        receipt.ProductLines[j].Price = decimal.Parse(lines[i].Replace(",", ".").Substring(1)); // if no price attached, sets item price to the read-in value
                                        receipt.TotalPrice += receipt.ProductLines[j].LineTotal;
                                    }

                                    i++; // goes to next scanner line to avoid repetition
                                }
                                break; // once prices are read, we dont need anymore info
                            }
                        }

                        receipt.UserID = User.Identity.GetUserId();

                        db.Receipts.Add(receipt);
                        db.SaveChanges();

                    } else
                    {
                        throw new Exception("No receipt template");
                    }



                    ViewBag.Successful = "y";
                    return View();
                }
                else
                {
                    throw new Exception("no file upload detected");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
                ViewBag.Successful = "n";
                return View();
            }
        }

        [HttpPost]
        public ActionResult EditReceipt(Receipt receipt)
        {
            return View(receipt);
        }

        public ActionResult Overview()
        {
            List<Receipt> receipts = db.Receipts.Include(r => r.ProductLines).OrderByDescending(r => r.PurchaseDate).ToList();

            if (receipts.Any())
            {
                receipts.RemoveAll(r => r.UserID != User.Identity.GetUserId());
            } else
            {
                ViewBag.Message = "No receipts uploaded yet...";
            }            

            return View(receipts);
        }
    }
}