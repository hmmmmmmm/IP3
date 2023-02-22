using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using System.Web.Security;
using IP3_Group4.Models;

namespace IP3_Group4.Data
{
    public class DBInit : DropCreateDatabaseIfModelChanges<DBContext>
    {
        protected override void Seed(DBContext context)
        {
            base.Seed(context);

            string[] paymenttypes = { "Card", "Cash", "GiftCard" };
            foreach (var type in paymenttypes)
            {
                var pt = new PaymentType(type);
                context.PaymentTypes.Add(pt);
            }

            context.SaveChanges();
        }
    }
}