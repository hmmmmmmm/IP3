using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IP3_Group4.Controllers
{
    public class StatsController : Controller
    {
        // Handles Views relating to users viewing data that has been processed. Basically the hub for most of the stuff.

        // GET: Stats
        public ActionResult Dashboard()
        {
            return View();
        }

        public ActionResult Spending()
        {
            return View();
        }

        public ActionResult Budget()
        {
            return View();
        }
    }
}