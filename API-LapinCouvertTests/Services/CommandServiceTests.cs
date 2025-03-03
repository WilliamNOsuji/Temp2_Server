using Microsoft.VisualStudio.TestTools.UnitTesting;
using API_LapinCouvert.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MVC_LapinCouvert.Data;
using Admin_API.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using API_LapinCouvert.DTOs;
using LapinCouvert.Models;
using Models.Models;

namespace API_LapinCouvert.Services.Tests
{
    [TestClass()]
    public class CommandServiceTests
    {
        private DbContextOptions<ApplicationDbContext> _options;
        private ApplicationDbContext _dbContextMock;
        private Mock<NotificationsService> _notificationsServiceMock;
        private CommandsService _commandsServiceMock;
        private Mock<RandomService> _randomServiceMock;
        private Mock<ChatService> _chatServiceMock;

        [TestInitialize]
        public void Init()
        {
            // Initialize the In-Memory Database
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "CommandService")
                .UseLazyLoadingProxies(true)
                .Options;

            _dbContextMock = new ApplicationDbContext(_options);

            _notificationsServiceMock = new Mock<NotificationsService>(_dbContextMock);
            _randomServiceMock = new Mock<RandomService>();
            _chatServiceMock = new Mock<ChatService>(_dbContextMock, null, _notificationsServiceMock.Object);

            // Create a constructor-accessible field for ChatService
            var field = typeof(CommandsService).GetField("_chatService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Initialize the Service
            _commandsServiceMock = new CommandsService(
                _dbContextMock,
                _notificationsServiceMock.Object,
                _randomServiceMock.Object,
                _chatServiceMock.Object
            );

            // Set the ChatService field using reflection
            if (field != null)
            {
                field.SetValue(_commandsServiceMock, _chatServiceMock.Object);
            }

            var hasher = new PasswordHasher<IdentityUser>();
            IdentityUser[] identityUsers = new IdentityUser[]
            {
                new IdentityUser
                {
                    Id = "User1Id",
                    UserName = "User",
                    Email = "user@user.com",
                    // La comparaison d'identity se fait avec les versions normalisés
                    NormalizedEmail = "USER@USER.COM",
                    NormalizedUserName = "USER",
                    EmailConfirmed = true,
                    // On encrypte le mot de passe
                    PasswordHash = hasher.HashPassword(null, "Passw0rd!"),
                    LockoutEnabled = true,
                },
                new IdentityUser
                {
                    Id = "User2Id",
                    UserName = "User2",
                    Email = "user2@user.com",
                    // La comparaison d'identity se fait avec les versions normalisés
                    NormalizedEmail = "USER2@USER.COM",
                    NormalizedUserName = "USER2",
                    EmailConfirmed = true,
                    // On encrypte le mot de passe
                    PasswordHash = hasher.HashPassword(null, "Passw0rd!"),
                    LockoutEnabled = true,
                },
            };
            Client[] clients = new Client[]
            {
                new Client
                {
                    Id = 1,
                    FirstName = "Yopp",
                    LastName = "Yupp",
                    UserId = "User1Id",
                    IsBanned = false,
                    CartId = 1,
                    Username = "User",
                },
                new Client
                {
                    Id = 2,
                    FirstName = "Greg",
                    LastName = "Grego",
                    UserId = "User2Id",
                    IsBanned = false,
                    CartId = 2,
                    Username = "User2",
                },
            };

            _dbContextMock.AddRange(identityUsers);
            _dbContextMock.AddRange(clients);
            _dbContextMock.SaveChanges();
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Clean up the database after each test
            _dbContextMock.Database.EnsureDeleted();
        }

        [TestMethod]
        public async Task CreateCommandTest()
        {

            Client client = _dbContextMock.Clients.Where(c => c.Id == 1).FirstOrDefault();
            CommandDTO commandDTO = new CommandDTO
            {
                Address = "",
                Currency = "",
                DeviceTokens = [""],
                PhoneNumber = "",
                TotalPrice = 100,
            };


            _randomServiceMock.Setup(r => r.Next(new Random().Next(), new Random().Next())).Returns(It.IsAny<int>());

            Command command = new Command
            {
                CommandNumber = It.IsAny<int>(),
                ClientPhoneNumber = commandDTO.PhoneNumber,
                ArrivalPoint = commandDTO.Address,
                TotalPrice = commandDTO.TotalPrice,
                Currency = commandDTO.Currency,
                ClientId = client.Id,
                OrderTime = DateTime.Now.ToUniversalTime(),
                DeviceToken = commandDTO.DeviceTokens
            };

            Command result = await _commandsServiceMock.CreateCommand(It.IsAny<int>(), commandDTO);



            Assert.IsInstanceOfType(result, typeof(Command));
            Assert.AreEqual(1, _dbContextMock.Commands.Count());
            Assert.AreEqual(result, _dbContextMock.Commands.Where(c => c.Id == result.Id).FirstOrDefault());
            Assert.AreEqual(command.CommandNumber, result.CommandNumber);
        }

        [TestMethod]
        public async Task UpdateInventoryTest()
        {
            var product = new Product { Id = 1, Name = "Product1", Quantity = 10, Description = "" };
            _dbContextMock.Products.Add(product);
            await _dbContextMock.SaveChangesAsync();

            var cartProducts = new List<CartProducts>
            {
                new CartProducts { ProductId = 1, Quantity = 2, CartId = 1, ImageUrl = "", Name = "" }
            };

            await _commandsServiceMock.UpdateInventory(cartProducts);

            var updatedProduct = await _dbContextMock.Products.FindAsync(1);
            Assert.AreEqual(8, updatedProduct.Quantity);
        }

        [TestMethod]
        public async Task ConvertCartToCommandProductsTest()
        {
            var cart = new Cart { Id = 1 };
            var command = new Command { Id = 1, CommandNumber = 123456, DeliveryManId = null, ArrivalPoint = "", ClientPhoneNumber = "", Currency = "" };
            _dbContextMock.Carts.Add(cart);
            _dbContextMock.Commands.Add(command);
            await _dbContextMock.SaveChangesAsync();

            var cartProducts = new List<CartProducts>
            {
                new CartProducts { ProductId = 1, Quantity = 2, CartId = 1, ImageUrl = "", Name = "" }
            };
            _dbContextMock.CartProducts.AddRange(cartProducts);
            await _dbContextMock.SaveChangesAsync();

            var result = await _commandsServiceMock.ConvertCartToCommandProducts(cart, command);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1, result[0].ProductId);
            Assert.AreEqual(2, result[0].Quantity);
        }

        [TestMethod]
        public async Task ClearCartAsyncTest()
        {
            var cart = new Cart { Id = 1 };
            _dbContextMock.Carts.Add(cart);
            await _dbContextMock.SaveChangesAsync();

            var cartProducts = new List<CartProducts>
            {
                new CartProducts { ProductId = 1, Quantity = 2, CartId = 1, ImageUrl = "", Name = "" }
            };
            _dbContextMock.CartProducts.AddRange(cartProducts);
            await _dbContextMock.SaveChangesAsync();

            await _commandsServiceMock.ClearCartAsync(cart);

            Assert.AreEqual(0, cart.CartProducts.Count);
        }

        [TestMethod]
        public void GetClientCommandTest()
        {
            Client client = _dbContextMock.Clients.FirstOrDefault();
            var command = new Command { Id = 1, Client = client, ClientId = client.Id, CommandNumber = 123456, DeliveryManId = null, ArrivalPoint = "", ClientPhoneNumber = "", Currency = "" };
            _dbContextMock.Commands.Add(command);
            _dbContextMock.SaveChanges();

            var result = _commandsServiceMock.GetClientCommand(1);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(123456, result[0].CommandNumber);
        }

        [TestMethod]
        public async Task GetAvailableCommandsTest()
        {
            var command = new Command { Id = 1, CommandNumber = 123456, DeliveryManId = null, ArrivalPoint = "", ClientPhoneNumber = "", Currency = "" };
            _dbContextMock.Commands.Add(command);
            await _dbContextMock.SaveChangesAsync();

            var result = await _commandsServiceMock.GetAvailableCommands();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(123456, result[0].CommandNumber);
        }

        [TestMethod]
        public async Task GetMyDeliveriesTest()
        {
            var client = _dbContextMock.Clients.FirstOrDefault();
            var deliveryMan = new DeliveryMan { Id = 1, Client = client };
            var command = new Command { Id = 1, CommandNumber = 123456, DeliveryManId = 1, ArrivalPoint = "", ClientPhoneNumber = "", Currency = "" };
            _dbContextMock.Commands.Add(command);
            await _dbContextMock.SaveChangesAsync();

            var result = await _commandsServiceMock.GetMyDeliveries(1);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(123456, result[0].CommandNumber);
        }

        [TestMethod]
        public async Task AssignADeliveryTest()
        {
            var client = _dbContextMock.Clients.FirstOrDefault();
            var deliveryMan = new DeliveryMan { Id = 1, Client = client };
            var command = new Command { Id = 1, CommandNumber = 123456, DeliveryManId = null, ArrivalPoint = "", ClientPhoneNumber = "", Currency = "", Client = client, ClientId = client.Id };
            _dbContextMock.Commands.Add(command);
            await _dbContextMock.SaveChangesAsync();

            var result = await _commandsServiceMock.AssignADelivery(1, 1);

            Assert.AreEqual("Assignation réussie.", result);
            Assert.AreEqual(1, command.DeliveryManId);
        }

        [TestMethod]
        public async Task DeliveryInProgressTest()
        {
            // Arrange
            var client = _dbContextMock.Clients.FirstOrDefault();
            var deliveryMan = new DeliveryMan { Id = 1, Client = client };
            var command = new Command
            {
                Id = 1,
                CommandNumber = 123456,
                DeliveryManId = 1,
                ArrivalPoint = "",
                ClientPhoneNumber = "",
                Currency = "",
                Client = client,
                ClientId = client.Id,
                DeviceToken = new List<string> { "test-token" }
            };
            _dbContextMock.DeliveryMans.Add(deliveryMan);
            _dbContextMock.Commands.Add(command);
            await _dbContextMock.SaveChangesAsync();

            // Mock ChatService
            _chatServiceMock.Setup(c => c.CreateChatAsync(1)).ReturnsAsync("chat-1");

            // Act
            var result = await _commandsServiceMock.DeliveryInProgress(1, 1);

            // Assert
            Assert.AreEqual("Commande en cours de livraison. Chat initialisé.", result);
            Assert.IsTrue(command.IsInProgress);
        }

        [TestMethod]
        public async Task CommandDeliveredTest()
        {
            // Arrange
            var client = _dbContextMock.Clients.FirstOrDefault();
            var deliveryMan = new DeliveryMan { Id = 1, Client = client };
            var command = new Command
            {
                Id = 1,
                CommandNumber = 123456,
                DeliveryManId = 1,
                ArrivalPoint = "",
                ClientPhoneNumber = "",
                Currency = "",
                DeviceToken = new List<string> { "test-token" }
            };
            _dbContextMock.DeliveryMans.Add(deliveryMan);
            _dbContextMock.Commands.Add(command);
            await _dbContextMock.SaveChangesAsync();

            // Mock ChatService
            _chatServiceMock.Setup(c => c.EndChatAsync(1)).Returns(Task.CompletedTask);

            // Act
            var result = await _commandsServiceMock.CommandDelivered(deliveryMan, 1);

            // Assert
            Assert.AreEqual("Livraison réussie. Chat terminé.", result);
            Assert.IsTrue(command.IsDelivered);
            Assert.IsFalse(command.IsInProgress);
            _chatServiceMock.Verify(c => c.EndChatAsync(1), Times.Once);
        }

        [TestMethod]
        public async Task CancelADeliveryTest()
        {
            // Arrange
            var client = _dbContextMock.Clients.FirstOrDefault();
            var deliveryMan = new DeliveryMan { Id = 1, Client = client };
            var command = new Command
            {
                Id = 1,
                CommandNumber = 123456,
                DeliveryManId = 1,
                ArrivalPoint = "",
                ClientPhoneNumber = "",
                Currency = "",
                DeviceToken = new List<string> { "test-token" }
            };
            _dbContextMock.DeliveryMans.Add(deliveryMan);
            _dbContextMock.Commands.Add(command);
            await _dbContextMock.SaveChangesAsync();

            // Mock ChatService
            _chatServiceMock.Setup(c => c.EndChatAsync(1)).Returns(Task.CompletedTask);

            // Act
            var result = await _commandsServiceMock.CancelADelivery(deliveryMan, 1);

            // Assert
            Assert.AreEqual("Annulation réussie. Chat terminé.", result);
            Assert.IsNull(command.DeliveryManId);
            Assert.IsFalse(command.IsInProgress);
            _chatServiceMock.Verify(c => c.EndChatAsync(1), Times.Once);
        }

        [TestMethod]
        public async Task AssignADelivery_CommandNotFound_ReturnsErrorMessage()
        {
            // Arrange
            int nonExistentCommandId = 999;

            // Act
            var result = await _commandsServiceMock.AssignADelivery(1, nonExistentCommandId);

            // Assert
            Assert.AreEqual("Commande introuvable.", result);
        }

        [TestMethod]
        public async Task AssignADelivery_CommandAlreadyDelivered_ReturnsErrorMessage()
        {
            var client = _dbContextMock.Clients.FirstOrDefault();
            var deliveryMan = new DeliveryMan { Id = 1, Client = client };
            var command = new Command { Id = 1, CommandNumber = 123456, DeliveryManId = 1, IsDelivered = true, ArrivalPoint = "", ClientPhoneNumber = "", Currency = "", Client = client, ClientId = client.Id };
            _dbContextMock.Commands.Add(command);
            await _dbContextMock.SaveChangesAsync();

            // Act
            var result = await _commandsServiceMock.AssignADelivery(1, 1);

            // Assert
            Assert.AreEqual("Commande déjà livrée.", result);
        }

        [TestMethod]
        public async Task AssignADelivery_CommandAlreadyAssigned_ReturnsErrorMessage()
        {
            var client = _dbContextMock.Clients.FirstOrDefault();
            var deliveryMan = new DeliveryMan { Id = 1, Client = client };
            var deliveryMan2 = new DeliveryMan { Id = 2 };
            var command = new Command { Id = 1, CommandNumber = 123456, DeliveryManId = 2, ClientPhoneNumber = "", ArrivalPoint = "", Currency = "", Client = client, ClientId = client.Id };
            _dbContextMock.Commands.Add(command);
            await _dbContextMock.SaveChangesAsync();

            // Act
            var result = await _commandsServiceMock.AssignADelivery(1, 1);

            // Assert
            Assert.AreEqual("La commande est déjà assignée à un autre livreur.", result);
        }

        [TestMethod]
        public async Task CommandDelivered_CommandNotFound_ReturnsErrorMessage()
        {
            // Arrange
            var command = new Command { Id = 1, CommandNumber = 123456, DeliveryManId = 1, IsDelivered = true, ClientPhoneNumber = "", ArrivalPoint = "", Currency = "" };
            var deliveryMan = new DeliveryMan { Id = 1, Commands = [command] };

            int nonExistentCommandId = 999;

            // Act
            var result = await _commandsServiceMock.CommandDelivered(deliveryMan, nonExistentCommandId);

            // Assert
            Assert.AreEqual("Commande introuvable pour ce livreur.", result);
        }

        [TestMethod]
        public async Task CommandDelivered_CommandAlreadyDelivered_ReturnsErrorMessage()
        {
            // Arrange
            var deliveryMan = new DeliveryMan { Id = 1 };
            var command = new Command { Id = 1, CommandNumber = 123456, DeliveryManId = 1, IsDelivered = true, ClientPhoneNumber = "", ArrivalPoint = "", Currency = "" };
            _dbContextMock.DeliveryMans.Add(deliveryMan);
            _dbContextMock.Commands.Add(command);
            await _dbContextMock.SaveChangesAsync();

            // Act
            var result = await _commandsServiceMock.CommandDelivered(deliveryMan, 1);

            // Assert
            Assert.AreEqual("Commande déjà livrée.", result);
        }

        [TestMethod]
        public async Task CancelADelivery_CommandNotFound_ReturnsErrorMessage()
        {
            // Arrange
            var command = new Command { Id = 1, CommandNumber = 123456, DeliveryManId = 1, IsDelivered = true, ClientPhoneNumber = "", ArrivalPoint = "", Currency = "" };
            var deliveryMan = new DeliveryMan { Id = 1, Commands = [command] };
            int nonExistentCommandId = 999;

            // Act
            var result = await _commandsServiceMock.CancelADelivery(deliveryMan, nonExistentCommandId);

            // Assert
            Assert.AreEqual("Commande introuvable pour ce livreur.", result);
        }

        [TestMethod]
        public async Task CancelADelivery_CommandAlreadyDelivered_ReturnsErrorMessage()
        {
            // Arrange
            var deliveryMan = new DeliveryMan { Id = 1 };
            var command = new Command { Id = 1, CommandNumber = 123456, DeliveryManId = 1, IsDelivered = true, ClientPhoneNumber = "", ArrivalPoint = "", Currency = "" };
            _dbContextMock.DeliveryMans.Add(deliveryMan);
            _dbContextMock.Commands.Add(command);
            await _dbContextMock.SaveChangesAsync();

            // Act
            var result = await _commandsServiceMock.CancelADelivery(deliveryMan, 1);

            // Assert
            Assert.AreEqual("Commande déjà livrée.", result);
        }

        [TestMethod]
        public void GetDeliveryManByIdTest()
        {
            var client = _dbContextMock.Clients.FirstOrDefault();
            var deliveryMan = new DeliveryMan { Id = 1, Client = client };
            _dbContextMock.DeliveryMans.Add(deliveryMan);
            _dbContextMock.SaveChanges();

            var result = _commandsServiceMock.GetDeliveryManById("User1Id");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
        }

        [TestMethod]
        public async Task GetCommandByIdTest()
        {
            // Arrange
            var client = _dbContextMock.Clients.FirstOrDefault();
            var command = new Command
            {
                Id = 1,
                CommandNumber = 123456,
                DeliveryManId = null,
                ArrivalPoint = "",
                ClientPhoneNumber = "",
                Currency = "",
                Client = client,
                ClientId = client.Id
            };
            _dbContextMock.Commands.Add(command);
            await _dbContextMock.SaveChangesAsync();

            // Act
            var result = await _commandsServiceMock.GetCommandById(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual(123456, result.CommandNumber);
            Assert.AreEqual(client.Id, result.ClientId);
        }

        [TestMethod]
        public async Task GetCommandById_ReturnsNull_WhenCommandNotFound()
        {
            // Act
            var result = await _commandsServiceMock.GetCommandById(999);

            // Assert
            Assert.IsNull(result);
        }
    }
}