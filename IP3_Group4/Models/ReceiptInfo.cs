using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace IP3_Group4.Models
{
    public class ReceiptInfo
    {
        [Key]
        public int ID { get; set; }

        public string Brand { get; set; }

        public List<ProdLine> ProductLines { get; set; }
    }

    // TEMPORARY: delete and refactor when OrderLine class or equivalent is made
    public class ProdLine
    {
        [Key]
        public int ID { get; set; }
        public string Product { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }

        public ProdLine(int iD, string product, int quantity, double price)
        {
            ID = iD;
            Product = product;
            Quantity = quantity;
            Price = price;
        }
    }
}