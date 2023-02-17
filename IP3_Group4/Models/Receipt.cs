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
        public int ID { get; set; }

        [Required]
        public string Shop { get; set; }
        public decimal TotalPrice { get; set; }

        [DataType(DataType.DateTime), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}"), Required]
        public DateTime PurchaseDate { get; set; }

        public Receipt(string Shop, DateTime PurchaseDate)
        {
            this.Shop = Shop;
            this.PurchaseDate = PurchaseDate;
        }

        #region Creator Properties
        [Display(Name = "Payment Type"), ForeignKey("PaymentType")]
        public int PaymentID { get; set; }
        public PaymentType PaymentType { get; set; }
        #endregion

        #region ProductLines Properties
        public List<ProductLine> ProductLines { get; set; }
        #endregion

        #region Poster Properties
        [Display(Name = "User"), ForeignKey("User")]
        public string UserID { get; set; }
        public User User { get; set; }
        #endregion
    }

    public class PaymentType
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string Type { get; set; }

        public PaymentType(string Type)
        {
            this.Type = Type;
        }

        #region ProductLines Properties
        public List<Receipt> Receipt { get; set; }
        #endregion
    }


    public class ProductLine
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }  
        public string ItemName { get; set; }
        public string Brand { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal LineTotal => (Price *= Quantity);
        public ProductLine(int ID, string ItemName, int Quantity, decimal Price)
        {
            this.ID = ID;
            this.ItemName = ItemName;
            this.Quantity = Quantity;
            this.Price = Price;
        }

        #region Poster Properties
        [Display(Name = "Receipt"), ForeignKey("Receipt")]
        public int ReceiptID { get; set; }
        public Receipt Receipt { get; set; }
        #endregion
    }
}