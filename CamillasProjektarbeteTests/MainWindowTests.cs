using Microsoft.VisualStudio.TestTools.UnitTesting;
using CamillasProjektarbete;
using System;
using System.Collections.Generic;
using System.Text;

using System.IO;

namespace CamillasProjektarbete.Tests
{
    [TestClass()]
    public class MainWindowTests
    {
        [TestMethod()]
        public void ReadProducts()
        {
            var myResultList = MainWindow.ReadProductFile("ProductsTest.csv");
            Assert.AreEqual(2, myResultList.Count);
        }

        [TestMethod()]
        public void ReadDiscountCodes() // input string not in a correct format
        {
            var result = MainWindow.ReadDiscountFile("DiscountCodesTest.csv");
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod()]
        public void ReadFromCart()
        {
            var result = MainWindow.ReadCartFile("CartTest.csv");
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod()]
        public void WriteToCart()
        {
            var result = MainWindow.WriteToCartfile();
            Assert.AreEqual(result, "cart is saved");
        }

        [TestMethod()]
        public void CalculateSum()
        {
            string result = MainWindow.CalculateSum();
            Assert.AreEqual(result, "without discount: 750 with discount: 675,0");
        }
    }
}