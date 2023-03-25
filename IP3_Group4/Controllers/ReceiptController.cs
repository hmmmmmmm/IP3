using Google.Cloud.Vision.V1;
using IP3_Group4.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP3_Group4.Data;
using Microsoft.AspNet.Identity;
using System.Data.Entity.Migrations;
using System.Net;

namespace IP3_Group4.Controllers
{
    public class ReceiptController : Controller
    {
        // Used to handle Views relating to the uploading, editing, and viewing of receipts


        DBContext db = new DBContext(); // an instance of the database to be accessed

        // GET: Vision
        [HttpGet]
        public ActionResult UploadReceipt() // loads the UploadReceipt page (without a receipt being uploaded)
        {
            return View(); // returns receipt upload page
        }

        [HttpPost]
        public ActionResult UploadReceipt(HttpPostedFileBase file) // catches HTTPPost from a receipt being uploaded
        {
            try // catches any nasty errors
            {
                if (file != null) // checks a file was uploaded
                {
                    string ending = file.FileName.Substring(file.FileName.IndexOf("."));
                    if (ending != ".png" && ending != ".jpg" && ending != ".jpeg")
                        throw new ArgumentException("Invalid file type!");


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

                    List<ProductLine> productLines = new List<ProductLine>(); // creates List to store the product lines
                    List<decimal> prices = new List<decimal>(); // creates List to store the prices
                    ReceiptTemplate template; // creates ReceiptTemplate object to store the corresponding template
                    List<int> pricesToSkip = new List<int>(); // creates List of price indices to be skipped when liking productlines and prices


                    if (lines.Contains("B&M Retail Ltd")) // checks if receipt from B&M
                    {
                        template = db.ReceiptTemplates.First(rt => rt.Shop == "b&m retail ltd"); // gets the B&M template
                        receipt.Shop = "B&M Retail Ltd"; // sets name of the shop bought from

                    } else if (lines.Contains("Sainsbury's Supermarkets Ltd")) // checks if receipt from Sainsbury's
                    {
                        template = db.ReceiptTemplates.First(rt => rt.Shop == "sainsbury's supermarkets ltd"); // gets the Sainsbury's template
                        receipt.Shop = "Sainsbury's Supermarkets Ltd"; // sets name of the shop bought from
                    } else // if no valid template
                    {
                        throw new Exception("No attributed template"); // throws error with the details
                    }

                    if (template != null) // ensures null wasn't returned for the template
                    {
                        // loops through every line in the scanned receipt
                        for (int i = 0; i < lines.Count; i++)
                        {
                            // checks if scanner has reached the start of the products list
                            if (!hasProds && lines[i].ToLower().Contains(template.ProductStartPrompt))
                            {                                
                                hasProds = true; // tells app it has found the list of products

                                i++; // skips to line after products list start marker
                                int prodIndex = 0; // the index of which product loop is at
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

                                    int foundIndex = productLines.FindIndex(pline => pline.ItemName == pl.ItemName); // gets index where a matching ProductLine already exists
                                    if (foundIndex >= 0) // if a matching productline was found
                                    {
                                        productLines[foundIndex].Quantity++; // increases quantity of existing line by one
                                        pricesToSkip.Add(prodIndex); // adds the index where the price will be to list to tell app to skip that index
                                    }
                                    else
                                    {             
                                        productLines.Add(pl); // adds ProductLine to receipt object
                                    }
                                    prodIndex++; // increments product index

                                } while (!lines[i].ToLower().Contains(template.ProductEndPrompt)); // checks for end of product list

                                continue; // skips to next line
                            }

                            // checks if next line is the payment method
                            if (template.PaymentTypePrompt != "" && lines[i].ToLower().Contains(template.PaymentTypePrompt))
                            {
                                i++; // goes to next line where payment type should be

                                if (lines[i].ToLower().Contains("card") || lines[i].ToLower().Contains("visa") || lines[i].ToLower().Contains("mastercard")) // if payment type is card, deals with it
                                {
                                    PaymentType payType = db.PaymentTypes.ToList().First(pt => pt.Type == "Card"); // gets corresponding database entry
                                    receipt.PaymentType = payType; // sets receipt paymenttype object to retrieve object
                                    receipt.PaymentID = payType.ID; // sets receipt paymenttype id to correct object
                                }
                                else if (lines[i].ToLower().Contains("cash")) // if payment type is cash, deals with it
                                {
                                    PaymentType payType = db.PaymentTypes.ToList().First(pt => pt.Type == "Cash"); // gets corresponding database entry
                                    receipt.PaymentType = payType; // sets receipt paymenttype object to retrieve object
                                    receipt.PaymentID = payType.ID; // sets receipt paymenttype id to correct object
                                }

                                continue; // skips to next iteration
                            }

                            // checks if next line is the Date/Time of purchase
                            if (template.DateTimePrompt != "" && lines[i].ToLower().Contains(template.DateTimePrompt))
                            {
                                DateTime dt = DateTime.Parse(lines[++i].ToLower()); // retrieves the Date/Time of the purchase
                                receipt.PurchaseDate = dt; // sets the receipt DateTime to string just read in
                            }

                            
                            // checks if the line is a price or not, and starts getting prices if they are
                            if (lines[i].ToLower().Contains("£"))
                            {
                                for (int j = 0; j < productLines.Count + pricesToSkip.Count; j++) // loops through all read-in products
                                {
                                    prices.Add(decimal.Parse(lines[i].Replace(",", ".").Substring(1))); //adds the price to the list of prices

                                    i++; // goes to next scanner line to avoid repetition
                                }
                                break; // once prices are read, we dont need anymore info
                            }
                        }

                        int numRemoved = 0; // keeps track of number of prices that have been removed so far
                        foreach (int toSkip in pricesToSkip) // loops through pricesToSkip and removes corresponding index in prices
                        {
                            prices.RemoveAt(toSkip - numRemoved); // removes corresponding index
                            numRemoved++; // increments the number of removed indeces by one
                        }

                        for (int i = 0; i < productLines.Count; i++) //loops through productlines and attaches prices
                        {
                            if (productLines[i].Price == 0) // checks the price isnt already assigned
                                productLines[i].Price = prices[i]; // assigns price to productlines
                        }

                        string id = User.Identity.GetUserId(); // gets user's id
                        var user = db.Users.Find(id); // finds user in database
                        receipt.UserID = user.Id; // sets the ID of the user that uploaded the receipt
                        receipt.User = user; // sets the user object in the receipt

                        if (receipt.PaymentType == null) // sets a payment type in the event none was found
                        {
                            PaymentType payType = db.PaymentTypes.ToList().First(pt => pt.Type == "Unknown"); // finds "Unknown" paymenttype in database
                            receipt.PaymentType = payType; // sets receipt paymenttype object to retrieve object
                            receipt.PaymentID = payType.ID; // sets receipt paymenttype id to correct object
                        }                      

                        db.Receipts.Add(receipt); // queues the receipt update to the database
                        db.SaveChanges(); // writes changes to database

                        // need to get the receipt's database ID in order to write the ProductLines successfully
                        receipt = db.Receipts.ToList().Last(r => r.UserID == id); // gets the receipt back
                        foreach (ProductLine pl in productLines) // loops through all productlines
                        {
                            pl.ReceiptID = receipt.ID; // sets line's receipt id
                            db.ProductLine.Add(pl); // queues line update to database
                        }
                        db.SaveChanges(); // writes all changes to database

                    } else // if there's no receipt template
                    {
                        throw new Exception("No receipt template found for " + receipt.Shop); // throws an error saying no template found
                    }

                    ViewBag.Successful = "y"; // tells View the upload was successfuly
                    return View(); // returns the View
                }
                else // if no file was uploaded
                {
                    throw new Exception("No file uploaded!"); // throws exception with details of why
                }
            }
            catch (Exception ex) // catches all exceptions thrown
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message); // writes error message to the Output tab
                if (ex.Message == "Invalid file type!" || ex.Message == "No file uploaded!") // checks if the issue was with the file type
                    ViewBag.Successful = "f"; // if it was, sets ViewBag to inform the View
                else // if it wasn't
                    ViewBag.Successful = "n"; // tells View to give generic failure message
                return View(); // returns View
            }
        }

        [HttpGet]
        public ActionResult EditReceiptProdLines(int? id) // Action for EditReceipt view
        {
            //check if id is null
            if (id == null)
            {
                //return badrequest
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //load the given receipt
            Receipt receipt = db.Receipts.Find(id);
            receipt.ProductLines = db.ProductLine.Where(r => r.ReceiptID == id).ToList();
            receipt.User = db.Users.Find(receipt.UserID);
            receipt.PaymentType = db.PaymentTypes.Find(receipt.PaymentID);

            //check if the blog is null
            if (receipt == null)
            {
                //return http not found
                return HttpNotFound();
            }

            return View(receipt); // returns the view with the selected receipt as model
        }

        [HttpPost]
        public ActionResult EditReceiptProdLines(Receipt receipt)
        {    
            if (ModelState.IsValid)
            {             
                foreach (ProductLine line in receipt.ProductLines)
                {
                    db.ProductLine.AddOrUpdate(line);
                }
                
                db.SaveChanges();
                return RedirectToAction("EditReceipt", new { id = receipt.ID });
            }

            return View(receipt);
        }

        [HttpGet]
        public ActionResult EditReceipt(int? id) // Action for EditReceipt view
        {
            //check if id is null
            if (id == null)
            {
                //return badrequest
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //load the given receipt
            Receipt receipt = db.Receipts.Find(id);

            //check if the blog is null
            if (receipt == null)
            {
                //return http not found
                return HttpNotFound();
            }
            ViewBag.PaymentID = new SelectList(db.PaymentTypes, "ID", "Type");

            return View(receipt); // returns the view with the selected receipt as model
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EditReceipt([Bind(Include = "ID, Shop, PurchaseDate, UserID, PaymentID")] Receipt receipt)
        {
            if (ModelState.IsValid)
            {
                receipt.PaymentType = db.PaymentTypes.Find(receipt.PaymentID);
                receipt.User = db.Users.Find(receipt.UserID);
                receipt.ProductLines = db.ProductLine.Where(r => r.ReceiptID == receipt.ID).ToList();

                db.Receipts.AddOrUpdate(receipt);               
                db.SaveChanges();

                return RedirectToAction("Overview");
            }

            ViewBag.PaymentID = new SelectList(db.PaymentTypes, "ID", "Type");

            return View(receipt);
        }

        public ActionResult Overview() // Action for Receipt Overview page
        {
            string id = User.Identity.GetUserId();
            List<Receipt> receipts = db.Receipts.Include(r => r.ProductLines).Where(r => r.UserID == id).OrderByDescending(r => r.PurchaseDate).ToList(); // retrieves all receipts, and corresponding productlines, from database that belong to user

            if (!receipts.Any()) // checks if any receipts were found
                ViewBag.Message = "No receipts uploaded yet..."; // if not sends message to view  

            return View(receipts); // returns view with the list of receipts
        }

        public ActionResult Details(Receipt receipt)
        {
            receipt = db.Receipts.Find(receipt.ID);
            receipt.ProductLines = db.ProductLine.Where(pl => pl.ReceiptID == receipt.ID).ToList();
            receipt.PaymentType = db.PaymentTypes.Find(receipt.PaymentID);
            receipt.User = db.Users.Find(receipt.UserID);

            return View(receipt);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            Receipt receipt = db.Receipts.FirstOrDefault(r => r.ID == id);

            db.Receipts.Remove(receipt);
            db.SaveChanges();

            ViewBag.Message = "Receipt deleted successfully!";
            return RedirectToAction("Overview");
        }

        [HttpPost]
        public ActionResult DeleteProdLine(int id)
        {
            ProductLine productLine = db.ProductLine.Find(id);
            int receiptID = productLine.ReceiptID;

            db.ProductLine.Remove(productLine);
            db.SaveChanges();

            ViewBag.Message = "Receipt deleted successfully!";
            return RedirectToAction("EditReceiptProdLines", new { id = receiptID });
        }
    }
}