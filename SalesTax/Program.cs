using System;
using System.Collections.Generic;
using System.Diagnostics;
using static SalesTax.CashRegister;

namespace SalesTax
{
    /// <summary>
    /// Public class to model a Cash Register
    /// </summary>
    public class CashRegister
    {
        /// <summary>
        /// Collection of current basket goods
        /// </summary>
        private Stack<Good> BasketGoods;

        /// <summary>
        /// Collection of books exempted from sales tax
        /// </summary>
        private HashSet<string> ExemptedBooksInventory;

        /// <summary>
        /// Collection of food exempted from sales tax
        /// </summary>
        private HashSet<string> ExemptedFoodInventory;

        /// <summary>
        /// Collection of medical products exempted from sales tax
        /// </summary>
        private HashSet<string> ExemptedMedicalProductsInventory;

        /// <summary>
        /// Wrapper for a good's basic information
        /// </summary>
        public struct DetailedGood
        {
            /// <summary>
            /// Constructor for modifying struct's members
            /// </summary>
            /// <param name="description">The label of the item</param>
            /// <param name="isImported">Attribute to maintain if item is imported</param>
            /// <param name="price">The price of the item</param>
            public DetailedGood(string description, bool isImported, double price)
            {
                this.Description = description;
                this.IsImported = isImported;
                this.Price = price;
            }

            /// <summary>
            /// The label of the item
            /// </summary>
            public string Description { get; }

            /// <summary>
            /// Bool to maintain if item is imported
            /// </summary>
            public bool IsImported { get; }

            /// <summary>
            /// The price of the item
            /// </summary>
            public double Price { get; }
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="exemptedFood">Collection of food items that are exempted from sales tax</param>
        /// <param name="exemptedBooks">Collection of books that are exempted from sales tax</param>
        /// <param name="exemptedMedicalProds">Collection of medical products that are exempted from sales tax</param>
        public CashRegister(string[] exemptedFood, string[] exemptedBooks, string[] exemptedMedicalProds)
        {
            BasketGoods = new Stack<Good>();
            ExemptedBooksInventory = new HashSet<string>();
            ExemptedFoodInventory = new HashSet<string>();
            ExemptedMedicalProductsInventory = new HashSet<string>();

            foreach (string item in exemptedFood)
            {
                ExemptedFoodInventory.Add(item);
            }

            foreach (string item in exemptedBooks)
            {
                ExemptedBooksInventory.Add(item);
            }

            foreach (string item in exemptedMedicalProds)
            {
                ExemptedMedicalProductsInventory.Add(item);
            }
        }

        /// <summary>
        /// Function takes in a collection of goods and updates the internal member based on the respective object type 
        /// </summary>
        /// <param name="detailedGoods"></param>
        public void ScanShoppingBasket(List<DetailedGood> detailedGoods)
        {
            foreach (DetailedGood good in detailedGoods)
            {
                if (ExemptedFoodInventory.Contains(good.Description))
                {
                    BasketGoods.Push(new Food(good.Description, good.IsImported, good.Price));
                }
                else if (ExemptedBooksInventory.Contains(good.Description))
                {
                    BasketGoods.Push(new Book(good.Description, good.IsImported, good.Price));
                }
                else if (ExemptedMedicalProductsInventory.Contains(good.Description))
                {
                    BasketGoods.Push(new MedicalProduct(good.Description, good.IsImported, good.Price));
                }
                else
                {
                    BasketGoods.Push(new Good(good.Description, good.IsImported, good.Price));
                }
            }
        }

        /// <summary>
        /// Get purchase details from receipt
        /// </summary>
        public Receipt GetReceipt()
        {
            return new Receipt(this.BasketGoods);
        }

        /// <summary>
        /// Private class to model a receipt
        /// </summary>
        public class Receipt
        {
            private const double SalesTaxRate = .1;
            private const double ImportTax = .05;

            private Dictionary<string, Good> PurchasedGoods;

            /// <summary>
            /// Wrapper for final and detailed information of purchase
            /// </summary>
            public struct ReceiptDetails
            {
                /// <summary>
                /// Struct constructor
                /// </summary>
                /// <param name="goodsDetails">List of objects containing the quantity bought, the item description, and the total cost for this item kind</param>
                /// <param name="salesTax"></param>
                /// <param name="totalCost"></param>
                public ReceiptDetails(List<Tuple<int, string, double>> goodsDetails, double salesTax, double totalCost)
                {
                    this.TotalCost = totalCost;
                    this.SalesTax = salesTax;
                    this.GoodsDetails = goodsDetails;
                }

                /// <summary>
                /// List of Tuple objects where we maintain information about quantities bought, descriptions, and total cost of each item
                /// </summary>
                public List<Tuple<int, string, double>> GoodsDetails { get; }

                /// <summary>
                /// Sales tax
                /// </summary>
                public double SalesTax { get; }

                /// <summary>
                /// Total cost
                /// </summary>
                public double TotalCost { get; }
            }

            /// <summary>
            /// Class constructor. It takes in a collection and builds a map of goods and the respective quantities of each
            /// </summary>
            /// <param name="basketGoods">Collection of items to be purchased</param>
            public Receipt(Stack<Good> basketGoods)
            {
                PurchasedGoods = new Dictionary<string, Good>();

                while (basketGoods.Count > 0)
                {
                    Good currentGood = basketGoods.Pop();

                    if (PurchasedGoods.ContainsKey(currentGood.Description))
                    {
                        PurchasedGoods[currentGood.Description].Quantity++;
                    }
                    else
                    {
                        PurchasedGoods.Add(currentGood.Description, currentGood);
                    }
                }
            }

            /// <summary>
            /// Helper internal function that calculates the total cost and breakdown of the sale and returns an object with all the relevant information
            /// </summary>
            private ReceiptDetails CalculateReceiptDetails()
            {
                List<Tuple<int, string, double>> goodsDetails = new List<Tuple<int, string, double>>();
                double totalSalesTax = 0;
                double totalCost = 0;

                foreach (KeyValuePair<string, Good> item in PurchasedGoods)
                {
                    Good currentGood = item.Value;

                    double tax = currentGood.IsImported ? ImportTax : 0;
                    tax += currentGood.IsExemptFromSalesTax ? 0 : SalesTaxRate;

                    // Rounded tax to nearest .05
                    double roundedTax = Math.Round(tax * 20) / 20;
                    double itemTotalPrice = currentGood.Price * (1 + roundedTax);

                    double costPerQuantity = Math.Round(itemTotalPrice * currentGood.Quantity, 2);

                    totalCost += costPerQuantity;
                    totalSalesTax += (roundedTax * currentGood.Price * currentGood.Quantity);

                    goodsDetails.Add(new Tuple<int, string, double>(item.Value.Quantity, item.Key, costPerQuantity));
                }

                return new ReceiptDetails(goodsDetails, Math.Round(totalSalesTax, 2), Math.Round(totalCost, 2));
            }

            /// <summary>
            /// Prints the cost details of a basket. It gives the total cost of an item, as well as the final breakdown of sales tax vs. total cost
            /// </summary>
            public void PrintReceiptDetails()
            {
                ReceiptDetails details = CalculateReceiptDetails();

                foreach (Tuple<int, string, double> detailedGood in details.GoodsDetails)
                {
                    Console.WriteLine(detailedGood.Item1 + " " + detailedGood.Item2 + ": " + detailedGood.Item3);
                }

                Console.WriteLine("Sales Tax: " + details.SalesTax);
                Console.WriteLine("Total: " + details.TotalCost);
            }

            public static bool TestAllImportedGoods(string[] exemptedFood, string[] exemptedBooks, string[] exemptedMedProducts)
            {
                CashRegister register = new CashRegister(exemptedFood, exemptedBooks, exemptedMedProducts);

                List<DetailedGood> basket = new List<DetailedGood>();
                basket.Add(new DetailedGood("imported box of chocolates", true, 8));
                basket.Add(new DetailedGood("imported bottle of perfume", true, 40));
                basket.Add(new DetailedGood("imported bottle of perfume", true, 40));
                basket.Add(new DetailedGood("box of imported chocolates", true, 8));

                register.ScanShoppingBasket(basket);
                Receipt receipt = register.GetReceipt();
                ReceiptDetails details = receipt.CalculateReceiptDetails();

                return details.GoodsDetails.Count == 3 && details.SalesTax == 12.8 && details.TotalCost == 108.8;
            }

            private static bool TestAllNotImportedGoods(string[] exemptedFood, string[] exemptedBooks, string[] exemptedMedProducts)
            {
                CashRegister register = new CashRegister(exemptedFood, exemptedBooks, exemptedMedProducts);

                List<DetailedGood> basket = new List<DetailedGood>();
                basket.Add(new DetailedGood("book", false, 15));
                basket.Add(new DetailedGood("music CD", false, 15));
                basket.Add(new DetailedGood("chocolate bar", false, 5));
                basket.Add(new DetailedGood("bottle of perfume", false, 25));
                basket.Add(new DetailedGood("packet of headache pills", false, 8));

                register.ScanShoppingBasket(basket);
                Receipt receipt = register.GetReceipt();
                ReceiptDetails details = receipt.CalculateReceiptDetails();

                return details.GoodsDetails.Count == 5 && details.SalesTax == 4 && details.TotalCost == 72;
            }
            private static bool TestImportedGoodsMix(string[] exemptedFood, string[] exemptedBooks, string[] exemptedMedProducts)
            {
                CashRegister register = new CashRegister(exemptedFood, exemptedBooks, exemptedMedProducts);

                List<DetailedGood> basket = new List<DetailedGood>();
                basket.Add(new DetailedGood("book", false, 15));
                basket.Add(new DetailedGood("music CD", false, 15));
                basket.Add(new DetailedGood("chocolate bar", false, 5));
                basket.Add(new DetailedGood("imported box of chocolates", true, 8));
                basket.Add(new DetailedGood("imported bottle of perfume", true, 40));
                basket.Add(new DetailedGood("imported bottle of perfume", true, 40));
                basket.Add(new DetailedGood("bottle of perfume", false, 25));
                basket.Add(new DetailedGood("packet of headache pills", false, 8));
                basket.Add(new DetailedGood("box of imported chocolates", true, 8));

                register.ScanShoppingBasket(basket);
                Receipt receipt = register.GetReceipt();
                ReceiptDetails details = receipt.CalculateReceiptDetails();

                return details.GoodsDetails.Count == 8 && details.SalesTax == 16.8 && details.TotalCost == 180.8;
            }

            public static void Main(string[] args)
            {
                string[] exemptedFood = new string[] {
                "chocolate bar",
                "imported box of chocolates",
                "box of imported chocolates" };

                string[] exemptedBooks = new string[] {
                "book",
                "imported book" };

                string[] exemptedMedProducts = new string[] {
                "packet of headache pills" };

                bool resultAllImported = TestAllImportedGoods(exemptedFood, exemptedBooks, exemptedMedProducts);
                bool resultNoneImported = TestAllNotImportedGoods(exemptedFood, exemptedBooks, exemptedMedProducts);
                bool resultImportedMix = TestImportedGoodsMix(exemptedFood, exemptedBooks, exemptedMedProducts);

                Debug.Assert(resultAllImported && resultNoneImported && resultImportedMix, "One or more tests failed");
            }
        }
    }

    /// <summary>
    /// Base class to model a consumer good, which is not exempted from sales tax
    /// </summary>
    public class Good
    {
        /// <summary>
        /// Attribute to check if item is exempt from sales tax
        /// </summary>
        public bool IsExemptFromSalesTax { get; set; }

        /// <summary>
        /// Attribute to check if is imported
        /// </summary>
        public bool IsImported { get; set; }

        public double Price { get; set; }
        /// <summary>
        /// Item label
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Quantity of this same-kind good
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="description">Label for the item</param>
        /// <param name="isImported">Attribute to check if is imported</param>
        /// <param name="price">Price of item</param>
        public Good(string description, bool isImported, double price)
        {
            this.IsImported = isImported;
            this.Price = price;
            this.IsExemptFromSalesTax = false;
            this.Description = description;
            this.Quantity = 1;
        }
    }

    /// <summary>
    /// This class models a food item, which is exempted from sales tax
    /// </summary>
    public class Food : Good
    {
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="description">Label for the food item</param>
        /// <param name="isImported">Attribute to check if is imported</param>
        /// <param name="price">Price of food item</param>
        public Food(string description, bool isImported, double price) : base(description, isImported, price)
        {
            this.IsExemptFromSalesTax = true;
        }
    }

    /// <summary>
    /// This class models a medical product, which is exempted from sales tax
    /// </summary>
    public class MedicalProduct : Good
    {
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="description">Label for the medical product</param>
        /// <param name="isImported">Attribute to check if is imported</param>
        /// <param name="price">Price of medical item</param>
        public MedicalProduct(string description, bool isImported, double price) : base(description, isImported, price)
        {
            this.IsExemptFromSalesTax = true;
        }
    }

    /// <summary>
    /// This class models a book, which is exempted from sales tax
    /// </summary>
    public class Book : Good
    {
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="description">Label for the book item</param>
        /// <param name="isImported">Attribute to check if is imported</param>
        /// <param name="price">Price of book</param>
        public Book(string description, bool isImported, double price) : base(description, isImported, price)
        {
            this.IsExemptFromSalesTax = true;
        }
    }
}