using Google.Cloud.Vision.V1;
using IP3_Group4.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Http.Routing;
using System.Web.Mvc;

namespace IP3_Group4.Controllers
{
    public class VisionController : Controller
    {
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
                    string filePath = Server.MapPath(Url.Content("~/ip3-receiptscanner-2ba7c0c23963.json"));

                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", filePath);

                    //System.Drawing.Image source = System.Drawing.Image.FromStream(file.InputStream, true, true);
                    Google.Cloud.Vision.V1.Image img = new Google.Cloud.Vision.V1.Image();
                    img = Google.Cloud.Vision.V1.Image.FromStream(file.InputStream);

                    ImageAnnotatorClient client = ImageAnnotatorClient.Create();
                    TextAnnotation response = client.DetectDocumentText(img);

                    

                    return View();
                } else
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
    }
}