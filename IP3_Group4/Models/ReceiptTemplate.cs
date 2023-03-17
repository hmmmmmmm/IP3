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

        // ALWAYS STORE PROPERTIES IN LOWER CASE!!!!!!!!!!

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; } // database primary key, no need to really touch

        [Required]
        public string Shop { get; set; } // Stores the shop where this template can be used

        public string ProductStartPrompt { get; set; } // The line ABOVE where the list of products are on the receipts
                                                                             //▲ ▼ used to mark beginning and end of product list respectively  
        public string ProductEndPrompt { get; set; } // The line BELOW where the list of products are on the receipts

        public string QuantityLineFormat { get; set; } // Allows the format of quantity lines to be defined. (eg. on B&M has format "4 x 0.40" so this would be defined as " x ". Make sure to put in spaces

        public string PaymentTypePrompt { get; set; } // The line above where the payment type is stored. On B&M: "PAID BY".

        public string DateTimePrompt { get; set; } // The line above where the Date and Time of purchase are stored on the receipt. Some receipts don't include it, so just pass "". Do not pass NULL.

        // Empty constructor. Only use for database purposes really.
        public ReceiptTemplate()
        {
            ID = 0; // sets id
            Shop = ""; // sets the shop 
            ProductStartPrompt = ""; // sets the line above the product list
            ProductEndPrompt = ""; // sets the line below the product list
            QuantityLineFormat = ""; // sets the format of quantity lines
            PaymentTypePrompt = ""; // sets the line above the payment type
            DateTimePrompt = ""; // sets line above where DateTime is found
        }

        // Standard constructor. Does what it says on the packet. Don't bother with setting ID tho as the database will do it for you.
        public ReceiptTemplate(string shop, string productStartPrompt, string productEndPrompt, string quantityLineFormat, string paymentTypePrompt, string dateTimePrompt)
        {
            Shop = shop; // sets shop name
            ProductStartPrompt = productStartPrompt; // sets the line above the product list
            ProductEndPrompt = productEndPrompt; // sets the line below the product list
            PaymentTypePrompt = paymentTypePrompt;  // sets the line above the payment type
            DateTimePrompt = dateTimePrompt; // sets line above where DateTime is found
            QuantityLineFormat = quantityLineFormat; // sets the format of quantity lines
        }
    }
}