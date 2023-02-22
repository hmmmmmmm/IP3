using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using System.Web.Security;
using IP3_Group4.Models;

namespace IP3_Group4.Data
{
    public class DBInit : DropCreateDatabaseAlways<DBContext>
    {
        protected override void Seed(DBContext context)
        {
            base.Seed(context);

            // seed payment types
            string[] paymenttypes = { "Card", "Cash", "GiftCard" };
            foreach (var type in paymenttypes)
            {
                var pt = new PaymentType(type);
                context.PaymentTypes.Add(pt);
            }

            // seed receipt templates
            ReceiptTemplate bandm = new ReceiptTemplate("b&m retail ltd", "customer copy", " item", " x ", "paid by", "customer receipt");
            ReceiptTemplate sainsburys = new ReceiptTemplate("sainsbury's supermarkets ltd", "vat number: ", " balance due", " @ ", " balance due", ""); // little funky
            context.ReceiptTemplates.Add(bandm);
            context.ReceiptTemplates.Add(sainsburys);

            context.SaveChanges();
        }
    }
}