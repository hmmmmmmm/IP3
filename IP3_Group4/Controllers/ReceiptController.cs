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
                if (file.ContentLength > 0)
                {
                    string filePath = Server.MapPath(Url.Content("~/ReceiptR-key.json"));

                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", filePath);

                    //System.Drawing.Image source = System.Drawing.Image.FromStream(file.InputStream, true, true);
                    Google.Cloud.Vision.V1.Image img = new Google.Cloud.Vision.V1.Image();
                    img = Google.Cloud.Vision.V1.Image.FromStream(file.InputStream);

                    ImageAnnotatorClient client = ImageAnnotatorClient.Create();
                    var response = client.DetectText(img);

                    System.Diagnostics.Debug.WriteLine(response.ToString());              

                    Receipt receipt = new Receipt();
                    List<ProductLine> productLines = new List<ProductLine>();
                    bool inProds = false, startInTwo = false;

                    for (int i = 0; i < response.Count; i++)
                    {
                        if (response[i].Description.ToLower().Contains("b&m retail ltd"))
                        {
                            receipt.Shop = "B&M Retail Ltd";
                            startInTwo = true;
                            continue;
                        }

                        if (startInTwo)
                        {
                            inProds = true;
                            startInTwo = false;
                            continue;

                        }

                        if (response[i].Description.ToLower().Contains("total ( "))
                        {
                            inProds = false;
                            continue;
                        }

                        if (inProds)
                        {
                            ProductLine pl = new ProductLine() { ItemName = response[i].Description };

                            if (response[i + 1].Description.ToLower().Contains(" x "))
                            {
                                string[] quantArr = response[i + 1].Description.Split(' ');
                                pl.Quantity = int.Parse(quantArr[0]);
                                pl.Price = decimal.Parse(quantArr[2]);
                            }
                            receipt.ProductLines.Add(pl);
                        }

                        if (response[i].Description.ToLower() == "paid by")
                        {
                            receipt.PaymentType = db.PaymentTypes.Find(response[i + 1].Description);
                            
                            //switch (response[i].Description.ToLower())
                            //{
                            //    case "cash":
                            //        receipt.PaymentType = db.PaymentTypes.Find("Cash");
                            //        break;

                            //    case "card":
                            //        receipt.PaymentType = db.PaymentTypes.Find("Card");
                            //        break;

                            //    case "giftcard":
                            //        receipt.PaymentType = db.PaymentTypes.Find("GiftCard");
                            //        break;

                            //    default:
                            //        throw new Exception("PaymentType Not Found");
                            //}
                        }
                    }

                    return View();
                }
                else
                {
                    throw new Exception();
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