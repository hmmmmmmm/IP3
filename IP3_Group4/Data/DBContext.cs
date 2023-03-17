using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Services.Description;
using IP3_Group4.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace IP3_Group4.Data
{
    public class DBContext : IdentityDbContext<User>
    {
        public DbSet<Receipt> Receipts { get; set; } // the Receipt table

        public DbSet<ProductLine> ProductLine { get; set; } // the ProductLine table

        public DbSet<PaymentType> PaymentTypes { get; set; } // the PaymentTypes table (slightly broken)

        public DbSet<ReceiptTemplate> ReceiptTemplates { get; set; } // the ReceiptTemplates table

        public DbSet<Budget> Budgets { get; set; } // the Budgets table

        public DBContext()
            : base("ReceiptBudgetApp", throwIfV1Schema: false)
        {
            Database.SetInitializer(new DBInit());
        }

        public static DBContext Create()
        {
            return new DBContext();
        }
    }
}