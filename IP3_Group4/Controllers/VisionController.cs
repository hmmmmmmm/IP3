using Google.Cloud.Vision.V1;
using IP3_Group4.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Web;
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
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "project-optic-f9d22c3d58c9.json");

                    System.Drawing.Image source = System.Drawing.Image.FromStream(file.InputStream, true, true);
                    Google.Cloud.Vision.V1.Image img = new Google.Cloud.Vision.V1.Image();
                    img.Image

                    ImageAnnotatorClient client = ImageAnnotatorClient.Create();
                    TextAnnotation text = client.DetectDocumentText(img);

                    ViewBag.Message = text;

                    return View();
                } else
                {
                    throw new System.Exception();
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