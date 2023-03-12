using IP3_Group4.Data;
using IP3_Group4.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace IP3_Group4.Controllers
{
    public class StatsController : Controller
    {
        // Handles Views relating to users viewing data that has been processed. Basically the hub for most of the stuff.
        private Analytics analytics;
        private DBContext dbContext = new DBContext();

        public ActionResult Dashboard()
        {
            
            analytics = new Analytics(GetUsersReceipts(), GetUsersBudget()); // initialise analytics class to pass to views

            return View(analytics);
        }

        public ActionResult Spending()
        {
            //Assign Analytics here
            ViewBag.Analytics = new Analytics(GetUsersReceipts(), GetUsersBudget());

            return View();
        }

        public ActionResult Budget()
        {
            Budget budge = dbContext.Budgets.First(b => b.UserID == User.Identity.GetUserId());
            if (budge != null)
            {
                return View(budge);
            } else
            {
                ViewBag.Message = "You can set up a budget on this page!";
                return View();
            }
        }

        [HttpPost]
        public ActionResult Budget(Budget budge)
        {
            if (budge != null && budge.Amount != 0)
            {
                budge.UserID = User.Identity.GetUserId();
                budge.LastReset = DateTime.Now;
                budge.NextReset = DateTime.Now.AddDays(30);

                dbContext.Budgets.Add(budge);
                dbContext.SaveChangesAsync();

                ViewBag.Message = "Budget set successfully!";
                return View(budge);
            } else if (budge != null)
            {
                ViewBag.Message = "Budgets must be at least £0.01!";
                return View();
            } else
            {
                ViewBag.Message = "Failed to set budget!";
                return View();
            }
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

        private Budget GetUsersBudget()
        {
            return dbContext.Budgets.First(b => b.UserID == User.Identity.GetUserId());        
        }
    }
}