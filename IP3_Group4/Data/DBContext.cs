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
        public DbSet<Receipt> Receipts { get; set; }

        public DbSet<ProductLine> ProductLine { get; set; }

        public DbSet<PaymentType> PaymentTypes { get; set; }

        public DbSet<ReceiptTemplate> ReceiptTemplates { get; set; }

        public DbSet<Budget> Budgets { get; set; }

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