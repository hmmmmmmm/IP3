using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
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


        // for seeding database
        public Receipt(string shop, DateTime purchaseDate, List<ProductLine> productLines, string userID)
        {
            Shop = shop; // sets shop name
            PurchaseDate = purchaseDate; // sets date receipt was printed
            ProductLines = productLines; // sets the productlines
            UserID = userID; // sets receipt's user id

            CalculateTotal(); // calculates the total price on the receipt
        }

        // Standard constructor
        public Receipt(string Shop, DateTime PurchaseDate)
        {
            this.Shop = Shop; // sets shop name
            this.PurchaseDate = PurchaseDate; // sets date and time of purchase
            ProductLines = new List<ProductLine>(); // initialises productline list
        }

        // Empty constructor
        public Receipt()
        {
            this.ID = 0; // sets id
            this.Shop = ""; // sets shop name
            this.TotalPrice = 0; // sets total price of receipt
            this.PurchaseDate = DateTime.UtcNow; // sets the date the receipt was purchased to now
            ProductLines = new List<ProductLine>(); // initialises productline list

        }

        public void CalculateTotal() // calculates total price of receipt
        {
            foreach (ProductLine pl in ProductLines) // loops through all productlines
                TotalPrice += pl.LineTotal; // adds line's price to receipt total
        }

        #region PaymentType Properties
        // broken lol

        //// Used to store the PaymentType of the receipt.
        //[Display(Name = "Payment Type"), ForeignKey("PaymentType")]
        //public int PaymentID { get; set; } // PaymentType ID
        //public PaymentType PaymentType { get; set; } // Actual payment type (cash, card)
        #endregion

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
            this.Type = Type; // sets the type of payment
        }

        // empty constructor
        public PaymentType()
        {
            this.ID = 0; // sets payment type's id
            this.Type = ""; // sets payment type's type
            Receipts = new List<Receipt>(); // why tho...
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
            this.ID = ID; // sets lines id
            this.ItemName = ItemName; // sets lines product name
            this.Quantity = Quantity; // sets number of product bought
            this.Price = Price; // sets price per unit
        }

        // emtpy constructor
        public ProductLine()
        {
            this.ID = 0; // sets id
            this.ItemName = ""; // sets items name
            this.Brand = ""; // sets items brand
            this.Quantity = 0; // sets items quantity
            this.Price = 0; // sets items price
        }

        #region Receipt Properties
        // stores info on receipt its stored in (foreign key shennanigans)
        [Display(Name = "Receipt"), ForeignKey("Receipt")]
        public int ReceiptID { get; set; } // ID of the receipt
        public Receipt Receipt { get; set; } // Actual receipt object
        #endregion
    }
}