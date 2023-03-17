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

        private Analytics analytics; // stores instance of the Analytics class to be accessed at any time
        private DBContext dbContext = new DBContext(); // instance of database for retrieving data

        public ActionResult Dashboard() // Action for the Dashboard page
        {
            analytics = new Analytics(GetUsersReceipts(), GetUsersBudget()); // initialise analytics class to pass to views

            return View(analytics);
        }

        public ActionResult Spending() // Action for Spending page
        {
            if (analytics == null) // if user hasnt been to dashboard yet
                return RedirectToAction("Dashboard"); // sends them to dashboard
            else // otherwise allows user to pass
                ViewBag.Analytics = analytics; // puts data in a ViewBag to be accessed by charts
                                               // NOT SURE THIS ACTUALLY WORKS
            return View(); // returns the View
        }

        [HttpGet]
        public ActionResult Budget() // Action for Budget page
        {
            Budget budge = dbContext.Budgets.First(b => b.UserID == User.Identity.GetUserId()); // gets the user's budget info from database
            if (budge != null) // checks a budget was actually found
            {
                return View(budge); // if so, returns the budget to the view
            } else // otherwise
            {
                ViewBag.Message = "You can set up a budget on this page!"; // sets up a cheery message for the user
                return View(); // returns the View without the budget
            }
        }

        [HttpPost]
        public ActionResult Budget(Budget budge) // Action for when Budget is created / updated
        {
            if (budge != null && budge.Amount > 0) // checks budget is set up, and that the amount is valid
            {
                // NEED TO CHECK IF THIS IS AN UPDATE OR A CREATION!!!

                budge.UserID = User.Identity.GetUserId(); // sets the budgets UserID to users id
                budge.LastReset = DateTime.Now; // sets today as the last reset date
                budge.NextReset = DateTime.Now.AddDays(30); // sets next reset for in 30 day's time

                dbContext.Budgets.Add(budge); // adds the budget to the database queue
                dbContext.SaveChangesAsync(); // saves the budget to database

                ViewBag.Message = "Budget set successfully!"; // creates success message for view
                return View(budge); // returns the view with the budget in case user wants to update
            } else if (budge != null) // if a budget was passed but the amount isnt valid
            {
                ViewBag.Message = "Budgets must be at least £0.01!"; // creates warning message for view
                return View(budge); // returns the view
            } else // if no budget object was passed
            {
                ViewBag.Message = "Failed to set budget!"; // creates error message for view
                return View(); // returns the view
            }
        } 

        private List<Receipt> GetUsersReceipts() // method to get the receipts belonging to user
        {
            List<Receipt> receipts = dbContext.Receipts.Where(r => r.UserID == User.Identity.GetUserId()).ToList(); // gets all receipts belonging to user
            foreach (Receipt receipt in receipts) // loops through each receipt and gets the productlines
                receipt.ProductLines = dbContext.ProductLine.Where(r => r.ReceiptID == receipt.ID).ToList(); // retrieves receipt's productlines

            // WOULD CODE IN RECEIPT OVERVIEW ACTION WORK?

            return receipts; // returns the list of receipts
        }

        private Budget GetUsersBudget() // method to get the user's budget object
        {
            return dbContext.Budgets.First(b => b.UserID == User.Identity.GetUserId()); // returns the user's budget
        }
    }
}