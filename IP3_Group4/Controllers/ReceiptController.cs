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
                    ReceiptTemplate template; // creates ReceiptTemplate object to store the corresponding template


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

                                    productLines.Add(pl); // adds ProductLine to receipt object
                                    

                                } while (!lines[i].ToLower().Contains(template.ProductEndPrompt)); // checks for end of product list

                                continue; // skips to next line
                            }

                            // checks if next line is the payment method
                            if (template.PaymentTypePrompt != "" && lines[i].ToLower().Contains(template.PaymentTypePrompt))
                            {
                                i++;

                                if (lines[i].ToLower().Contains("card") || lines[i].ToLower().Contains("visa") || lines[i].ToLower().Contains("mastercard")) // if payment type is card, deals with it
                                {
                                    PaymentType payType = db.PaymentTypes.ToList().First(pt => pt.Type == "Card");
                                    receipt.PaymentType = payType;
                                    receipt.PaymentID = payType.ID;
                                }
                                else if (lines[i].ToLower().Contains("cash")) // if payment type is cash, deals with it
                                {
                                    PaymentType payType = db.PaymentTypes.ToList().First(pt => pt.Type == "Cash");
                                    receipt.PaymentType = payType;
                                    receipt.PaymentID = payType.ID;
                                }
                                else // if payment type is neither, deals with it
                                {
                                    throw new Exception("PaymentType not found");
                                }

                                continue;
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
                                for (int j = 0; j < receipt.ProductLines.Count; j++) // loops through all read-in products
                                {
                                    if (receipt.ProductLines[j].Price == 0) // checks the ProductLine doesn't already have a price attached
                                    {
                                        receipt.ProductLines[j].Price = decimal.Parse(lines[i].Replace(",", ".").Substring(1)); // if no price attached, sets item price to the read-in value
                                        receipt.TotalPrice += receipt.ProductLines[j].LineTotal; // adds ProductLine total to receipt total
                                    }

                                    i++; // goes to next scanner line to avoid repetition
                                }
                                break; // once prices are read, we dont need anymore info
                            }
                        }

                        string id = User.Identity.GetUserId();
                        var user = db.Users.Find(id);
                        receipt.UserID = user.Id; // sets the ID of the user that uploaded the receipt
                        receipt.User = user;

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
        public ActionResult EditReceipt(Receipt receipt) // Action for EditReceipt view
        {
            return View(receipt); // returns the view with the selected receipt as model
        }

        public ActionResult Overview() // Action for Receipt Overview page
        {
            string id = User.Identity.GetUserId();
            List<Receipt> receipts = db.Receipts.Include(r => r.ProductLines).Where(r => r.UserID == id).OrderByDescending(r => r.PurchaseDate).ToList(); // retrieves all receipts, and corresponding productlines, from database that belong to user

            if (!receipts.Any()) // checks if any receipts were found
                ViewBag.Message = "No receipts uploaded yet..."; // if not sends message to view  

            return View(receipts); // returns view with the list of receipts
        }

        [HttpGet]
        public ActionResult Details(Receipt receipt)
        {
            List<ProductLine> products = db.ProductLine.Where(pl => pl.ReceiptID == receipt.ID).ToList();
            return View(products);
        }
    }
}