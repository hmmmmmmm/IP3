using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IP3_Group4.Models
{
    public class Budget
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public int ResetDay { get; set; }


        [Display(Name = "User"), ForeignKey("User")]
        public string UserID { get; set; } // ID of the user
        public User User { get; set; } // Actual user object

        public Budget(int id, decimal amount, int resetDay)
        {
            Id = id;
            Amount = amount;
            ResetDay = resetDay;
        }

        public Budget() {
            Amount = 0;
            ResetDay = 1;
        }
    }
}
