using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace IP3_Group4.Models
{
    public class Receipt
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; } // Primary key for by database

        [Required]
        public string Shop { get; set; } // Shop where receipt was obtained
        public decimal TotalPrice { get; set; } // TotalPrice of the receipt

        [DataType(DataType.DateTime), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}"), Required]
        public DateTime PurchaseDate { get; set; } // Date and Time of purchase

        // Standard constructor
        public Receipt(string Shop, DateTime PurchaseDate)
        {
            this.Shop = Shop;
            this.PurchaseDate = PurchaseDate;
            ProductLines = new List<ProductLine>();
        }

        // Empty constructor
        public Receipt()
        {
            this.ID = 0;
            this.Shop = "";
            this.TotalPrice = 0;
            this.PurchaseDate = DateTime.UtcNow;
            ProductLines = new List<ProductLine>();

        }

        //#region PaymentType Properties
        //// Used to store the PaymentType of the receipt.
        //[Display(Name = "Payment Type"), ForeignKey("PaymentType")]
        //public int PaymentID { get; set; } // PaymentType ID
        //public PaymentType PaymentType { get; set; } // Actual payment type (cash, card)
        //#endregion

        #region ProductLines Properties
        // List of products found on the receipt
        public List<ProductLine> ProductLines { get; set; }
        #endregion

        #region User Properties
        // Stores the user the receipt belongs to. For database purposes mostly.
        [Display(Name = "User"), ForeignKey("User")]
        public string UserID { get; set; } // ID of the user
        public User User { get; set; } // Actual user object
        #endregion
    }

    // Stores different payment types
    public class PaymentType
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; } // PaymentType ID
        public string Type { get; set; } // Actual type of payment (cash, card, etc.)

        // standard constructor
        public PaymentType(string Type)
        {
            this.Type = Type;
        }

        // empty constructor
        public PaymentType()
        {
            this.ID = 0;
            this.Type = "";
            Receipts = new List<Receipt>();
        }

        #region ProductLines Properties
        public List<Receipt> Receipts { get; set; }
        #endregion
    }

    // Stores info on an actual product within a receipt
    public class ProductLine
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; } // ID of the ProductLine
        public string ItemName { get; set; } // name of the Product (usually some kind of gibberish
        public string Brand { get; set; } // the Brand of the product (eg. "Cadbury's")
        public int Quantity { get; set; } // number of the product bought
        public decimal Price { get; set; } // the price for each product
        public decimal LineTotal => (Price *= Quantity); // the total price of Price * Quantity. automatically calculated

        // standard constructor
        public ProductLine(int ID, string ItemName, int Quantity, decimal Price)
        {
            this.ID = ID;
            this.ItemName = ItemName;
            this.Quantity = Quantity;
            this.Price = Price;
        }

        // emtpy constructor
        public ProductLine()
        {
            this.ID = 0;
            this.ItemName = "";
            this.Brand = "";
            this.Quantity = 0;
            this.Price = 0;
        }

        #region Receipt Properties
        // stores info on receipt its stored in (foreign key shennanigans)
        [Display(Name = "Receipt"), ForeignKey("Receipt")]
        public int ReceiptID { get; set; } // ID of the receipt
        public Receipt Receipt { get; set; } // Actual receipt object
        #endregion
    }
}