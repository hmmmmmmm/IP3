using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using IP3_Group4.Data;

namespace IP3_Group4.Models
{
    public class Budget
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime NextReset { get; set; }
        public DateTime LastReset { get; set; }

        [NotMapped]
        public BudgetAnalytics Analytics { get; set; } // make sure to check if null in View

        #region User Stuff
        [Display(Name = "User"), ForeignKey("User")]
        public string UserID { get; set; } // ID of the user
        public User User { get; set; } // Actual user object
        #endregion

        public Budget(int id, decimal amount, DateTime nextReset, DateTime lastReset)
        {
            Id = id;
            Amount = amount;
            NextReset = nextReset;
            LastReset = lastReset;

            if (Amount != 0)
            {
                List<Receipt> receipts = new DBContext().Receipts.ToList();
                decimal spent = 0;
                foreach (Receipt receipt in receipts)
                {
                    if (receipt.UserID == UserID)
                    {
                        spent += receipt.TotalPrice;
                    }
                }

                Analytics = new BudgetAnalytics(this, spent);
            }
                
        }

        public Budget() {
            Amount = 0;
            LastReset = DateTime.Now;
            NextReset = DateTime.Now.AddDays(30);
        }
    }

    // ViewModel for Budgets
    public class BudgetAnalytics
    {
        public decimal BudgetAmount { get; set; }
        public decimal Remaining { get; set; }
        public int DaysLeft { get; set; }
        public int TargetPercentage { get; set; } // the percentage user will reach by next reset
        // by the end of this month, at this rate, you will have spent {TargetPercentage} of your budget

        public BudgetAnalytics(Budget budge, decimal spent)
        {
            BudgetAmount = budge.Amount;
            Remaining = BudgetAmount - spent;

            // calculates what percent of the user's budget will be spent by end of reset period
            DaysLeft = (int)(budge.NextReset - DateTime.Now).TotalDays;
            int daysGone = (int) (budge.LastReset - DateTime.Now).TotalDays;
            decimal spentByEnd = (spent / daysGone) * 30;
            TargetPercentage = (int) (spentByEnd / budge.Amount);
            // credits to my friend Calvin for helping me not overthink that crap :D
        }
    }
}
