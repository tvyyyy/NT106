using System;
using System.Collections.Generic;

namespace BobaShop.CustomerApp
{
    public class CartTopping
    {
        public string ToppingName { get; set; } = "";
        public decimal Price { get; set; }
    }

    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = "Trà Sữa";
        public decimal BasePrice { get; set; }
        public int Quantity { get; set; } = 1;
        public string Size { get; set; } = "M";
        public string SugarLevel { get; set; } = "100%";
        public string IceLevel { get; set; } = "100%";
        public string Note { get; set; } = "";
        public bool IsSelected { get; set; } = true;

        public List<CartTopping> Toppings { get; set; } = new List<CartTopping>();

        // Đơn giá 1 ly (Giá gốc + Tổng giá Toppings)
        public decimal UnitPrice
        {
            get
            {
                decimal toppingTotal = 0;
                if (Toppings != null)
                {
                    foreach (var t in Toppings)
                    {
                        toppingTotal += t.Price;
                    }
                }
                return BasePrice + toppingTotal;
            }
        }

        // Tổng tiền (Đơn giá x Số lượng)
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}