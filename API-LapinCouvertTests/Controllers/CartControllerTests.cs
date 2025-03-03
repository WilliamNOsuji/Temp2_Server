using Microsoft.VisualStudio.TestTools.UnitTesting;
using API_LapinCouvert.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using API_LapinCouvert.Services;
using Microsoft.EntityFrameworkCore;
using MVC_LapinCouvert.Data;
using LapinCouvert.Models;
using Microsoft.AspNetCore.Identity;
using Models.Models;
using Microsoft.AspNetCore.Mvc;

namespace API_LapinCouvert.Controllers.Tests
{
    [TestClass()]
    public class CartControllerTests
    {


        [TestMethod()]
        public void AddQuantityCartProductTest()
        {

            Cart cart = new Cart
            {
                Id = 1
            };
            Product product = new Product
            {
                Id = 1
            };

            CartProducts cartProducts = new CartProducts
            {
                CartId = 1,
                ProductId = 1,
                Quantity = 1
            };

            Mock<CartService> serviceMock = new Mock<CartService>();
            Mock<CartController> controller = new Mock<CartController>(serviceMock.Object) { CallBase = true };

            serviceMock.Setup(serviceMock => serviceMock.GetCartFromClientId(It.IsAny<int>())).Returns(cart);
            serviceMock.Setup(serviceMock => serviceMock.GetProductFromProductId(It.IsAny<int>())).Returns(product);
            serviceMock.Setup(serviceMock => serviceMock.GetCartProductIfAlready(It.IsAny<int>(), It.IsAny<int>())).Returns(cartProducts);

            var actionResult = controller.Object.AddQuantityCartProduct(1, 1);
            var result = actionResult.Result as OkObjectResult;


            Assert.IsNotNull(result);
            Assert.AreEqual(2, cartProducts.Quantity);
        }

        [TestMethod()]
        public void AddQuantityCartProductNullTest()
        {

            Cart cart = null;
            Product product = new Product
            {
                Id = 1
            };

            CartProducts cartProducts = new CartProducts
            {
                CartId = 1,
                ProductId = 1,
                Quantity = 1
            };

            Mock<CartService> serviceMock = new Mock<CartService>();
            Mock<CartController> controller = new Mock<CartController>(serviceMock.Object) { CallBase = true };

            serviceMock.Setup(serviceMock => serviceMock.GetCartFromClientId(It.IsAny<int>())).Returns(cart);
            serviceMock.Setup(serviceMock => serviceMock.GetProductFromProductId(It.IsAny<int>())).Returns(product);
            serviceMock.Setup(serviceMock => serviceMock.GetCartProductIfAlready(It.IsAny<int>(), It.IsAny<int>())).Returns(cartProducts);

            var actionResult = controller.Object.AddQuantityCartProduct(1, 1);
            var result = actionResult.Result as BadRequestObjectResult;


            Assert.IsNotNull(result);
            Assert.AreEqual("Le produit ou votre panier est introuvable.", result.Value);
        }

        [TestMethod()]
        public void DecreaseQuantityCartProductNullTest()
        {

            Cart cart = null;
            Product product = new Product
            {
                Id = 1
            };

            CartProducts cartProducts = new CartProducts
            {
                CartId = 1,
                ProductId = 1,
                Quantity = 12
            };

            Mock<CartService> serviceMock = new Mock<CartService>();
            Mock<CartController> controller = new Mock<CartController>(serviceMock.Object) { CallBase = true };

            serviceMock.Setup(serviceMock => serviceMock.GetCartFromClientId(It.IsAny<int>())).Returns(cart);
            serviceMock.Setup(serviceMock => serviceMock.GetProductFromProductId(It.IsAny<int>())).Returns(product);
            serviceMock.Setup(serviceMock => serviceMock.GetCartProductIfAlready(It.IsAny<int>(), It.IsAny<int>())).Returns(cartProducts);

            var actionResult = controller.Object.DecreaseQuantityCartProduct(1, 1);
            var result = actionResult.Result as BadRequestObjectResult;


            Assert.IsNotNull(result);
            Assert.AreEqual("Le produit ou votre panier est introuvable.", result.Value);
        }

        [TestMethod()]
        public void DecreaseQuantityCartProductTest()
        {

            Cart cart = new Cart
            {
                Id = 1
            };
            Product product = new Product
            {
                Id = 1
            };

            CartProducts cartProducts = new CartProducts
            {
                CartId = 1,
                ProductId = 1,
                Quantity = 2
            };

            Mock<CartService> serviceMock = new Mock<CartService>();
            Mock<CartController> controller = new Mock<CartController>(serviceMock.Object) { CallBase = true };

            serviceMock.Setup(serviceMock => serviceMock.GetCartFromClientId(It.IsAny<int>())).Returns(cart);
            serviceMock.Setup(serviceMock => serviceMock.GetProductFromProductId(It.IsAny<int>())).Returns(product);
            serviceMock.Setup(serviceMock => serviceMock.GetCartProductIfAlready(It.IsAny<int>(), It.IsAny<int>())).Returns(cartProducts);

            var actionResult = controller.Object.DecreaseQuantityCartProduct(1, 1);
            var result = actionResult.Result as OkObjectResult;


            Assert.IsNotNull(result);
            Assert.AreEqual(1, cartProducts.Quantity);
        }

        [TestMethod()]
        public void DeleteCartProductTest()
        {
            Cart cart = new Cart
            {
                Id = 1
            };
            Product product = new Product
            {
                Id = 1
            };
            Product product2 = new Product
            {
                Id = 2
            };

            CartProducts cartProducts = new CartProducts
            {
                CartId = 1,
                ProductId = 1,
                Quantity = 2
            };
            CartProducts cartProduct2 = new CartProducts
            {
                CartId = 1,
                ProductId = 2,
                Quantity = 2
            };



            Mock<CartService> serviceMock = new Mock<CartService>();
            Mock<CartController> controller = new Mock<CartController>(serviceMock.Object) { CallBase = true };

            serviceMock.Setup(serviceMock => serviceMock.GetCartFromClientId(It.IsAny<int>())).Returns(cart);
            serviceMock.Setup(serviceMock => serviceMock.GetProductFromProductId(It.IsAny<int>())).Returns(product);
            serviceMock.Setup(serviceMock => serviceMock.GetCartProductIfAlready(It.IsAny<int>(), It.IsAny<int>())).Returns(cartProducts);

            var actionResult = controller.Object.DeleteCartProduct(1, 1);
            var result = actionResult.Result as OkObjectResult;


            Assert.IsNotNull(result);
            Assert.AreEqual("Produit supprimé", result.Value);

        }

        [TestMethod()]
        public void DeleteCartProductNullTest()
        {
            Cart cart = null;
            Product product = new Product
            {
                Id = 1
            };
            Product product2 = new Product
            {
                Id = 2
            };

            CartProducts cartProducts = new CartProducts
            {
                CartId = 1,
                ProductId = 1,
                Quantity = 2
            };
            CartProducts cartProduct2 = new CartProducts
            {
                CartId = 1,
                ProductId = 2,
                Quantity = 2
            };



            Mock<CartService> serviceMock = new Mock<CartService>();
            Mock<CartController> controller = new Mock<CartController>(serviceMock.Object) { CallBase = true };

            serviceMock.Setup(serviceMock => serviceMock.GetCartFromClientId(It.IsAny<int>())).Returns(cart);
            serviceMock.Setup(serviceMock => serviceMock.GetProductFromProductId(It.IsAny<int>())).Returns(product);
            serviceMock.Setup(serviceMock => serviceMock.GetCartProductIfAlready(It.IsAny<int>(), It.IsAny<int>())).Returns(cartProducts);

            var actionResult = controller.Object.DeleteCartProduct(1, 1);
            var result = actionResult.Result as BadRequestObjectResult;


            Assert.IsNotNull(result);
            Assert.AreEqual("Le produit ou votre panier est introuvable.", result.Value);

        }

        [TestMethod()]
        public void AddCartProductNullTest()
        {
            Cart cart = null;
            Product product = new Product
            {
                Id = 1
            };

            CartProducts cartProducts = new CartProducts
            {
                CartId = 1,
                ProductId = 1,
                Quantity = 1
            };

            Mock<CartService> serviceMock = new Mock<CartService>();
            Mock<CartController> controller = new Mock<CartController>(serviceMock.Object) { CallBase = true };

            serviceMock.Setup(serviceMock => serviceMock.GetCartFromClientId(It.IsAny<int>())).Returns(cart);
            serviceMock.Setup(serviceMock => serviceMock.GetProductFromProductId(It.IsAny<int>())).Returns(product);
            serviceMock.Setup(serviceMock => serviceMock.GetCartProductIfAlready(It.IsAny<int>(), It.IsAny<int>())).Returns(cartProducts);

            var actionResult = controller.Object.AddCartProduct(1, 1);
            var result = actionResult.Result as BadRequestObjectResult;


            Assert.IsNotNull(result);
            Assert.AreEqual("Le produit ou votre panier est introuvable.", result.Value);
        }

        [TestMethod()]
        public void AddCartProductQuantityErrorTest()
        {
            Cart cart = new Cart
            {
                Id = 1
            };
            Product product = new Product
            {
                Id = 1,
                Quantity = 0
            };

            CartProducts cartProducts = new CartProducts
            {
                CartId = 1,
                ProductId = 1,
                Quantity = 1
            };

            Mock<CartService> serviceMock = new Mock<CartService>();
            Mock<CartController> controller = new Mock<CartController>(serviceMock.Object) { CallBase = true };

            serviceMock.Setup(serviceMock => serviceMock.GetCartFromClientId(It.IsAny<int>())).Returns(cart);
            serviceMock.Setup(serviceMock => serviceMock.GetProductFromProductId(It.IsAny<int>())).Returns(product);
            serviceMock.Setup(serviceMock => serviceMock.GetCartProductIfAlready(It.IsAny<int>(), It.IsAny<int>())).Returns(cartProducts);

            var actionResult = controller.Object.AddCartProduct(1, 1);
            var result = actionResult.Result as BadRequestObjectResult;


            Assert.IsNotNull(result);
            Assert.AreEqual("Ce produit est épuisé", result.Value);
        }


        [TestMethod()]
        public void AddCartProductCartProductNotNullTest()
        {
            Cart cart = new Cart
            {
                Id = 1
            };
            Product product = new Product
            {
                Id = 1,
                Quantity = 2,
                IsDeleted = false
            };

            CartProducts cartProducts = new CartProducts
            {
                CartId = 1,
                ProductId = 1,
                Quantity = 1
            };

            Mock<CartService> serviceMock = new Mock<CartService>();
            Mock<CartController> controller = new Mock<CartController>(serviceMock.Object) { CallBase = true };

            serviceMock.Setup(serviceMock => serviceMock.GetCartFromClientId(It.IsAny<int>())).Returns(cart);
            serviceMock.Setup(serviceMock => serviceMock.GetProductFromProductId(It.IsAny<int>())).Returns(product);
            serviceMock.Setup(serviceMock => serviceMock.GetCartProductIfAlready(It.IsAny<int>(), It.IsAny<int>())).Returns(cartProducts);

            var actionResult = controller.Object.AddCartProduct(1, 1);
            var result = actionResult.Result as OkObjectResult;


            Assert.IsNotNull(result);
            Assert.AreEqual(2, cartProducts.Quantity);
        }

        [TestMethod()]
        public void AddCartProductCartProductIsDeletedTest()
        {
            Cart cart = new Cart
            {
                Id = 1
            };
            Product product = new Product
            {
                Id = 1,
                Quantity = 2,
                IsDeleted = true
            };

            CartProducts cartProducts = new CartProducts
            {
                CartId = 1,
                ProductId = 1,
                Quantity = 1
            };

            Mock<CartService> serviceMock = new Mock<CartService>();
            Mock<CartController> controller = new Mock<CartController>(serviceMock.Object) { CallBase = true };

            serviceMock.Setup(serviceMock => serviceMock.GetCartFromClientId(It.IsAny<int>())).Returns(cart);
            serviceMock.Setup(serviceMock => serviceMock.GetProductFromProductId(It.IsAny<int>())).Returns(product);
            serviceMock.Setup(serviceMock => serviceMock.GetCartProductIfAlready(It.IsAny<int>(), It.IsAny<int>())).Returns(cartProducts);

            var actionResult = controller.Object.AddCartProduct(1, 1);
            var result = actionResult.Result as BadRequestObjectResult;


            Assert.IsNotNull(result);
            Assert.AreEqual("Ce produit n'existe plus", result.Value);
        }

        [TestMethod()]
        public void AddCartProductCartProductNullTest()
        {
            Cart cart = new Cart
            {
                Id = 1
            };
            Product product = new Product
            {
                Id = 1,
                Quantity = 2,
                IsDeleted = false,
                Name = "Jus d pomme",
                SellingPrice = 789.00,
            };

            CartProducts newCartProducts = new CartProducts(product, cart.Id);
            CartProducts cartProducts = null;

            Mock<CartService> serviceMock = new Mock<CartService>();
            Mock<CartController> controller = new Mock<CartController>(serviceMock.Object) { CallBase = true };

            serviceMock.Setup(serviceMock => serviceMock.GetCartFromClientId(It.IsAny<int>())).Returns(cart);
            serviceMock.Setup(serviceMock => serviceMock.GetProductFromProductId(It.IsAny<int>())).Returns(product);
            serviceMock.Setup(serviceMock => serviceMock.GetCartProductIfAlready(It.IsAny<int>(), It.IsAny<int>())).Returns(cartProducts);

            var actionResult = controller.Object.AddCartProduct(1, 1);
            var result = actionResult.Result as OkObjectResult;

            CartProducts cartProducts1 = result.Value as CartProducts;
            Assert.IsNotNull(result);
            Assert.AreEqual(newCartProducts.Name, cartProducts1.Name);
            Assert.AreEqual(newCartProducts.Prix, cartProducts1.Prix);
            Assert.AreEqual(newCartProducts.Quantity, cartProducts1.Quantity);
            Assert.AreEqual(newCartProducts.MaxQuantity, cartProducts1.MaxQuantity);
            Assert.AreEqual(newCartProducts.ProductId, cartProducts1.ProductId);
            Assert.AreEqual(newCartProducts.CartId, cartProducts1.CartId);


        }


        [TestMethod()]
        public void ValidateCartProductNoCartProductsTest()
        {

            Cart cart = new Cart
            {
                Id = 1
            };
            Product product = new Product
            {
                Id = 1
            };

            CartProducts cartProducts = new CartProducts
            {
                CartId = 1,
                ProductId = 1,
                Quantity = 1
            };

            List<CartProducts> liste = [];

            Mock<CartService> serviceMock = new Mock<CartService>();
            Mock<CartController> controller = new Mock<CartController>(serviceMock.Object) { CallBase = true };

            serviceMock.Setup(serviceMock => serviceMock.GetCartFromClientId(It.IsAny<int>())).Returns(cart);
            serviceMock.Setup(serviceMock => serviceMock.GetProductFromProductId(It.IsAny<int>())).Returns(product);
            serviceMock.Setup(serviceMock => serviceMock.GetCartProductIfAlready(It.IsAny<int>(), It.IsAny<int>())).Returns(cartProducts);

            var actionResult = controller.Object.ValidateCartProduct(liste);
            var result = actionResult.Result as BadRequestObjectResult;


            Assert.IsNotNull(result);
            Assert.AreEqual("Le panier est vide", result.Value);
        }

        [TestMethod()]
        public void ValidateCartProductIsOutOfStockTest()
        {

            Cart cart = new Cart
            {
                Id = 1
            };
            Product product = new Product
            {
                Id = 1
            };

            CartProducts cartProducts = new CartProducts
            {
                CartId = 1,
                ProductId = 1,
                Quantity = 1,
                isOutofStock = true
            };

            List<CartProducts> liste = new List<CartProducts>();
            liste.Add(cartProducts);

            Mock<CartService> serviceMock = new Mock<CartService>();
            Mock<CartController> controller = new Mock<CartController>(serviceMock.Object) { CallBase = true };

            serviceMock.Setup(serviceMock => serviceMock.GetCartProductIfAlready(It.IsAny<int>(), It.IsAny<int>())).Returns(cartProducts);
            serviceMock.Setup(serviceMock => serviceMock.isCartvalid(liste)).Returns(false);

            var actionResult = controller.Object.ValidateCartProduct(liste);
            var result = actionResult.Result as BadRequestObjectResult;


            Assert.IsNotNull(result);
            Assert.AreEqual("Votre panier contient des produits invalides", result.Value);
        }

        [TestMethod()]
        public void ValidateCartProductTest()
        {
            Cart cart = new Cart
            {
                Id = 1
            };
            Product product = new Product
            {
                Id = 1,
                IsDeleted = false,
                Quantity = 1000
            };

            CartProducts cartProducts = new CartProducts
            {
                CartId = 1,
                ProductId = 1,
                Quantity = 100,
                isOutofStock = false,
            };

            List<CartProducts> liste = new List<CartProducts>();
            liste.Add(cartProducts);

            Mock<CartService> serviceMock = new Mock<CartService>();
            Mock<CartController> controller = new Mock<CartController>(serviceMock.Object) { CallBase = true };

            serviceMock.Setup(serviceMock => serviceMock.GetCartProductIfAlready(It.IsAny<int>(), It.IsAny<int>())).Returns(cartProducts);
            serviceMock.Setup(serviceMock => serviceMock.isCartvalid(liste)).Returns(true);


            var actionResult = controller.Object.ValidateCartProduct(liste);
            var result = actionResult.Result as OkObjectResult;


            Assert.IsNotNull(result);
            Assert.AreEqual("Le panier est valide", result.Value);
        }
    }
}