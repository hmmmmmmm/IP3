using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace IP3_Group4.Models
{
    // Class lets us create templates for the app to read in the receipts from

    public class ReceiptTemplate
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public string Shop { get; set; }

        public string ProductStartPrompt { get; set; }

        public string ProductEndPrompt { get; set; }

        public string QuantityLineFormat { get; set; }

        public string PaymentTypePrompt { get; set; }

        public string DateTimePrompt { get; set; }

        public ReceiptTemplate()
        {
            ID = 0;
            Shop = "";
            ProductStartPrompt = "";
            ProductEndPrompt = "";
            QuantityLineFormat = "";
            PaymentTypePrompt = "";
            DateTimePrompt = "";
        }

        public ReceiptTemplate(string shop, string productStartPrompt, string productEndPrompt, string quantityLineFormat, string paymentTypePrompt, string dateTimePrompt)
        {
            Shop = shop;
            ProductStartPrompt = productStartPrompt;
            ProductEndPrompt = productEndPrompt;
            PaymentTypePrompt = paymentTypePrompt;
            DateTimePrompt = dateTimePrompt;
            QuantityLineFormat = quantityLineFormat;
        }
    }
}