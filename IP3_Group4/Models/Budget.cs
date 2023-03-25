using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using IP3_Group4.Data;

namespace IP3_Group4.Models
{
    public class Budget // the class for storing budgets
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; } // budget's ID
        public decimal Amount { get; set; } // amount allowed for budget
        [DataType(DataType.DateTime), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime NextReset { get; set; } // date of when budget will next reset
        [DataType(DataType.DateTime), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime LastReset { get; set; } // date of when budget was last reset
        [Display(Name = "Reset Period")]
        public int ResetPeriod { get; set; } // how many days after LastReset should the budget reset?

        [NotMapped]
        public BudgetAnalytics Analytics { get { return GetAnalytics(); } } // make sure to check if null in View
        // stores all the info on the Budget to be viewed in a View

        #region User Stuff
        [Display(Name = "User"), ForeignKey("User")]
        public string UserID { get; set; } // ID of the user
        public User User { get; set; } // Actual user object
        #endregion

        public Budget(int id, decimal amount, DateTime nextReset, DateTime lastReset, int resetPeriod) // standard constructor
        {
            ID = id; // sets budget's id
            Amount = amount; // sets budget's amount
            NextReset = nextReset; // sets budget's next reset date
            LastReset = lastReset; // sets budget's previous reset date
            ResetPeriod = resetPeriod; // set budget's reset period
        }

        public Budget() { // empty constructor
            Amount = -1; // sets amount to 0
            LastReset = DateTime.Now.Date; // sets last reset to today
            NextReset = DateTime.Now.Date.AddDays(ResetPeriod); // sets next reset to in ResetPeriod days
        }

        public BudgetAnalytics GetAnalytics()
        {
            if (Amount != 0) // checks the budget has actually been set
            {
                List<Receipt> receipts = new DBContext().Receipts.Include("ProductLines").Where(r => r.UserID == UserID).ToList(); // retrieves all the users receipts from database
                decimal spent = 0; // stores the amount spent

                receipts.RemoveAll(r => r.PurchaseDate.Date < LastReset.Date);

                foreach (Receipt receipt in receipts)
                {
                    spent += receipt.TotalPrice;
                }// loops through all receipts
                    // adds total price of receipt to total spent

                return new BudgetAnalytics(this, spent); // creates new BudgetAnalytics class
            }
            return null;
        }
    }

    // ViewModel for Budgets
    public class BudgetAnalytics
    {
        public decimal BudgetAmount { get; set; } // the amount in the budget
        public decimal Remaining { get; set; } // how much of the budget remains
        public int DaysLeft { get; set; } // how many days are left until budget resets
        public double TargetPercentage { get; set; } // the percentage user will reach by next reset
        // by the end of this month, at this rate, you will have spent {TargetPercentage} of your budget

        public BudgetAnalytics(Budget budge, decimal spent) // constructor for class
        {
            BudgetAmount = budge.Amount; // sets amount in budget
            Remaining = BudgetAmount - spent; // sets how much is left in budget

            // calculates what percent of the user's budget will be spent by end of reset period
            DaysLeft = (int)(budge.NextReset.Date - DateTime.Now.Date).TotalDays; //figures out how many days are left
            int daysGone = (int) (DateTime.Now - budge.LastReset).TotalDays; // figures out how many days have gone by since last reset

            if (daysGone == 0)
                daysGone = 1;

            decimal spentByEnd = (spent / daysGone) * budge.ResetPeriod; // figures out how much is spent on average per day, then multiplies by 30 to find out how much will be spent by next reset
            TargetPercentage = (double) (spentByEnd / budge.Amount) * 100; // turns the amount into a percentage
            TargetPercentage = (double)Math.Round(TargetPercentage, 2);
            // credits to my bestie Calvin for helping me not overthink this lol :D
        }
    }
}