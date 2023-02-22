﻿using Google.Cloud.Vision.V1;
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
                    bool inProds = false, startInTwo = false, hasProds = false, hasPrices = false; // inProds: keeps track of if the program is inside the products list. startInTwo: maybe irrelevant. hasProds: tells app if it already has accessed the products list. // hasPrices tells program if it has the prices.

                    // for debugging: prints API output line by line to console
                    System.Diagnostics.Debug.WriteLine(response[0].Description);
                    List<string> lines = response[0].Description.Split('\n').ToList<string>();

                    // loops through every line in the scanned receipt
                    for (int i = 1; i < lines.Count; i++)
                    {
                        // checks if scanner has reached the start of the products list
                        if (!hasProds && lines[i].ToLower().Contains("customer copy"))
                        {

                            System.Diagnostics.Debug.WriteLine("Found product start");

                            receipt.Shop = "B&M Retail Ltd"; // sets name of the shop bought from
                            //startInTwo = true;
                            hasProds = true; // tells app it has found the list of products
                            inProds = true;
                            continue;
                        }

                        //if (startInTwo)
                        //{
                        //    inProds = true;
                        //    startInTwo = false;
                        //    continue;
                        //}

                        // checks if app has reached the end of the product list
                        if (lines[i].ToLower().Contains(" items"))
                        {

                            System.Diagnostics.Debug.WriteLine("Found product end");

                            inProds = false; // tells app it is now longer in the products list
                            continue;
                        }

                        // if app is in the collection of products
                        if (inProds)
                        {

                            System.Diagnostics.Debug.WriteLine("Found a product");

                            ProductLine pl = new ProductLine() { ItemName = lines[i] }; // creates ProductLine object to be filled

                            if (lines[i + 1].ToLower().Contains(" x ")) // checks if the next line is a quantity line
                            {
                                i++; // if a quantity line, moves scanner to that line
                                string[] quantArr = lines[i].Split(' '); // splits quantity line into sections: 0 = quantity, 2 = price per item
                                pl.Quantity = int.Parse(quantArr[0]); // sets quantity in ProductLine
                                pl.Price = decimal.Parse(quantArr[2]); // sets price per item in ProductLine
                            } else // if there is no quantity line for the product
                            {
                                pl.Quantity = 1; // sets quantity to 1
                            }

                            receipt.ProductLines.Add(pl); // adds ProductLine to receipt

                            continue;
                        }

                        // checks if next line is the payment method
                        if (lines[i].ToLower().Contains("paid by"))
                        {

                            System.Diagnostics.Debug.WriteLine("Found payment type");

                            if (lines[i + 1].ToLower().Contains("card")) // if payment type is card, deals with it
                            {
                                receipt.PaymentType = db.PaymentTypes.First(pt => pt.Type == "Card");

                            } else if (lines[i + 1].ToLower().Contains("cash")) // if payment type is cash, deals with it
                            {
                                receipt.PaymentType = db.PaymentTypes.First(pt => pt.Type == "Cash");
                            }
                            else // if payment type is neither, deals with it
                            {
                                throw new Exception("PaymentType Not Found");
                            }

                            continue;
                        }

                        // checks if next line is the Date/Time of purchase
                        if (lines[i].ToLower().Contains("customer receipt"))
                        {

                            System.Diagnostics.Debug.WriteLine("Found DateTime");

                            DateTime dt = DateTime.Parse(lines[i + 1].ToLower()); // retrieves the Date/Time of the purchase
                            receipt.PurchaseDate = dt; // sets the receipt DateTime to string just read in
                        }

                        // checks if the line is a price or not, and starts getting prices if they are
                        if (!hasPrices && lines[i].ToLower().Contains("£"))
                        {

                            System.Diagnostics.Debug.WriteLine("Found price list");

                            for (int j = 0; j < receipt.ProductLines.Count; j++) // loops through all read-in products
                            {
                                if (receipt.ProductLines[j].Price == 0) // checks the ProductLine doesn't already have a price attached
                                {
                                    receipt.ProductLines[j].Price = decimal.Parse(lines[i].Substring(1)); // if no price attached, sets item price to the read-in value
                                }

                                i++; // goes to next scanner line to avoid repetition
                            }
                            break; // once prices are read, we dont need anymore info
                        }
                    }

                    System.Diagnostics.Debug.WriteLine("----------------------------------");
                    System.Diagnostics.Debug.WriteLine(receipt.PurchaseDate);
                    foreach (var item in receipt.ProductLines)
                    {
                        System.Diagnostics.Debug.WriteLine($"{item.ItemName} ----- Q: {item.Quantity} ----- P: {item.Price} ----- T: {item.LineTotal}");
                    }

                    return View();
                }
                else
                {
                    throw new Exception("Brokey");
                }
            }
            catch
            {
                ViewBag.Message = "File upload failed!!";
                return View();
            }
        }

        public ActionResult EditReceipt()
        {
            return View();
        }

        [HttpPost]
        public ActionResult EditReceipt(Receipt receipt)
        {
            return View();
        }

        public ActionResult Overview()
        {
            return View();
        }
    }
}