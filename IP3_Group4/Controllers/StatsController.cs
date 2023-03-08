using IP3_Group4.Data;
using IP3_Group4.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Http.Hosting;
using System.Web.Mvc;

namespace IP3_Group4.Controllers
{
    public class StatsController : Controller
    {
        // Handles Views relating to users viewing data that has been processed. Basically the hub for most of the stuff.
        private Analytics analytics;
        private DBContext dbContext = new DBContext();

        // GET: Stats
        public ActionResult Dashboard()
        {
            dbContext = new DBContext(); // initialise DBContext

            analytics = new Analytics(GetUsersReceipts()); // initialise analytics class to pass to views

            return View(analytics);
        }

        public ActionResult Spending()
        {
            //Assign Analytics here
            ViewBag.Analytics = new Analytics(GetUsersReceipts());

            return View();
        }

        public ActionResult Budget()
        {
            return View();
        }

        private List<Receipt> GetUsersReceipts()
        {
            // gets all receipts belonging to this user
            List<Receipt> receipts = new List<Receipt>();
            receipts = dbContext.Receipts.ToList();
            for (int i = 0; i < receipts.Count; i++)
            {
                if (receipts[i].UserID != User.Identity.GetUserId())
                    receipts.Remove(receipts[i]);
            }

            return receipts;
        }
    }
}