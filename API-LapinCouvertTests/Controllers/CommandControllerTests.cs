using Microsoft.VisualStudio.TestTools.UnitTesting;
using API_LapinCouvert.Controllers;
using API_LapinCouvert.Services;
using LapinCouvert.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using API_LapinCouvert.DTOs;
using Admin_API.Hubs;
using Microsoft.AspNetCore.Http;
using Models.Models;
using MVC_LapinCouvert.Services;
using Microsoft.EntityFrameworkCore;
using MVC_LapinCouvert.Data;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Configuration;

namespace API_LapinCouvert.Controllers.Tests
{
    [TestClass()]
    public class CommandsControllerTests
    {
        private DbContextOptions<ApplicationDbContext> _options;
        private ApplicationDbContext _dbContext;
        private Mock<UserManager<IdentityUser>> _userManagerMock;
        private Mock<Admin_API.Services.ClientsService> _clientsServiceMock;
        private Mock<CommandsService> _commandsServiceMock;
        private Mock<IHubContext<DeliveryHub>> _hubContextMock;
        private Mock<CartService> _cartServiceMock;
        private Mock<NotificationsService> _notificationsServiceMock;
        private Mock<RandomService> _randomServiceMock;
        private Mock<ChatService> _chatServiceMock;
        private Mock<FirebaseService> _firebaseServiceMock;
        private CommandsController _controller;

        [TestInitialize]
        public void Init()
        {
            // Initialize the In-Memory Database
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "CommandService")
                .UseLazyLoadingProxies(true)
                .Options;

            _dbContext = new ApplicationDbContext(_options);

            // Mock dependencies
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);

            _clientsServiceMock = new Mock<Admin_API.Services.ClientsService>(_dbContext);

            _notificationsServiceMock = new Mock<NotificationsService>(_dbContext);

            _randomServiceMock = new Mock<RandomService>();

            // Setup FirebaseService mock
            _firebaseServiceMock = new Mock<FirebaseService>() { CallBase = false };

            _chatServiceMock = new Mock<ChatService>(
                _dbContext,
                _firebaseServiceMock.Object,
                _notificationsServiceMock.Object);

            _hubContextMock = new Mock<IHubContext<DeliveryHub>>();
            _cartServiceMock = new Mock<CartService>(_dbContext);

            _commandsServiceMock = new Mock<CommandsService>(
                _dbContext,
                _notificationsServiceMock.Object,
                _randomServiceMock.Object,
                _chatServiceMock.Object
                );

            _hubContextMock = new Mock<IHubContext<DeliveryHub>>();
            _cartServiceMock = new Mock<CartService>(_dbContext);

            // Initialize the controller
            _controller = new CommandsController(
                _userManagerMock.Object,
                _commandsServiceMock.Object,
                _hubContextMock.Object,
                _clientsServiceMock.Object,
                _cartServiceMock.Object,
                _notificationsServiceMock.Object
            );
            // Mock the User property of the controller
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.NameIdentifier, "test-user-id")
            }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Clean up the database after each test
            _dbContext.Database.EnsureDeleted();
        }

        // Add test for GetCommand method
        [TestMethod()]
        public async Task GetCommand_ReturnsNotFound_WhenCommandIsNull()
        {
            // Arrange
            _commandsServiceMock.Setup(s => s.GetCommandById(It.IsAny<int>())).ReturnsAsync((Command)null);

            // Act
            var result = await _controller.GetCommand(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            Assert.AreEqual("Command not found", (result as NotFoundObjectResult).Value);
        }

        [TestMethod()]
        public async Task GetCommand_ReturnsOk_WhenCommandIsFound()
        {
            // Arrange
            var command = new Command { Id = 1 };
            var client = new Client { Id = 1, UserId = "test-user-id" };

            _commandsServiceMock.Setup(s => s.GetCommandById(It.IsAny<int>())).ReturnsAsync(command);
            _clientsServiceMock.Setup(s => s.GetClientFromUserId(It.IsAny<string>())).Returns(client);

            // Act
            var result = await _controller.GetCommand(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.AreEqual(command, (result as OkObjectResult).Value);
        }

        [TestMethod()]
        public async Task Create_ReturnsNotFound_WhenClientIsNull()
        {

            Client client = null;
            _clientsServiceMock.Setup(s => s.GetClientFromUserId(It.IsAny<string>())).Returns(client);


            var result = await _controller.Create(new CommandDTO());


            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
            Assert.AreEqual("Client introuvable", (result.Result as NotFoundObjectResult).Value);
        }

        [TestMethod()]
        public async Task Create_ReturnsNotFound_WhenCartIsNull()
        {

            Client client = new Client { Id = 1, UserId = "test-user-id" };
            Cart cart = null;
            _clientsServiceMock.Setup(s => s.GetClientFromUserId(It.IsAny<string>())).Returns(client);
            _cartServiceMock.Setup(s => s.GetCartWithIncludeFromClientId(It.IsAny<int>())).Returns(cart);


            var result = await _controller.Create(new CommandDTO());


            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
            Assert.AreEqual("Commande introuvable", (result.Result as NotFoundObjectResult).Value);
        }

        [TestMethod()]
        public async Task Create_ReturnsBadRequest_WhenCommandCreationFails()
        {

            Client client = new Client { Id = 1, UserId = "test-user-id" };
            Cart cart = new Cart { Id = 1 };
            //CommandDTO commandDTO = new CommandDTO { Address = "Address", Currency = "Currency", DeviceTokens = ["DeviceToken"], PhoneNumber = "123-456-7890", TotalPrice = 1.0 };
            _clientsServiceMock.Setup(s => s.GetClientFromUserId(It.IsAny<string>())).Returns(client);
            _cartServiceMock.Setup(s => s.GetCartWithIncludeFromClientId(It.IsAny<int>())).Returns(cart);
            _commandsServiceMock.Setup(s => s.CreateCommand(It.IsAny<int>(), It.IsAny<CommandDTO>())).ReturnsAsync((Command)null);


            var result = await _controller.Create(new CommandDTO());


            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            Assert.AreEqual("Erreur lors de la creation de la commande", (result.Result as BadRequestObjectResult).Value);
        }

        [TestMethod()]
        public async Task Create_ReturnsOk_WhenCommandIsCreatedSuccessfully()
        {

            var client = new Client { Id = 1, UserId = "test-user-id" };
            var cart = new Cart
            {
                Id = 1,
                CartProducts = new List<CartProducts>()
            };
            var command = new Command { Id = 1 };
            var commandProducts = new List<CommandProduct>();

            // Create a valid CommandDTO object
            var commandDTO = new CommandDTO
            {
                Address = "123 Test Street",
                Currency = "USD",
                DeviceTokens = new List<string> { "device-token-1" },
                PhoneNumber = "123-456-7890",
                TotalPrice = 100.0
            };

            // Mock dependencies
            _clientsServiceMock.Setup(s => s.GetClientFromUserId(It.IsAny<string>())).Returns(client);
            _cartServiceMock.Setup(s => s.GetCartWithIncludeFromClientId(It.IsAny<int>())).Returns(cart);
            _commandsServiceMock.Setup(s => s.CreateCommand(It.IsAny<int>(), commandDTO)).ReturnsAsync(command);
            _commandsServiceMock.Setup(s => s.ConvertCartToCommandProducts(It.IsAny<Cart>(), It.IsAny<Command>())).ReturnsAsync(commandProducts);
            _commandsServiceMock.Setup(s => s.UpdateInventory(It.IsAny<List<CartProducts>>()));
            _commandsServiceMock.Setup(s => s.ClearCartAsync(It.IsAny<Cart>()));
            _notificationsServiceMock.Setup(s => s.SendFirebaseNotificationToDeliveryMen(It.IsAny<string>()));


            var result = await _controller.Create(commandDTO);


            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            Assert.AreEqual(command, (result.Result as OkObjectResult).Value);
        }

        [TestMethod()]
        public async Task GetClientCommands_ReturnsNotFound_WhenClientIsNull()
        {

            _clientsServiceMock.Setup(s => s.GetClientFromUserId(It.IsAny<string>())).Returns((Client)null);


            var result = await _controller.GetClientCommands();


            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
            Assert.AreEqual("Le client est introuvable.", (result.Result as NotFoundObjectResult).Value);
        }

        [TestMethod()]
        public async Task GetClientCommands_ReturnsNotFound_WhenCommandsAreNull()
        {

            var client = new Client { Id = 1, UserId = "test-user-id" };
            _clientsServiceMock.Setup(s => s.GetClientFromUserId(It.IsAny<string>())).Returns(client);
            _commandsServiceMock.Setup(s => s.GetClientCommand(It.IsAny<int>())).Returns((List<Command>)null);


            var result = await _controller.GetClientCommands();


            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
            Assert.AreEqual("Commandes introuvables ou aucune commande.", (result.Result as NotFoundObjectResult).Value);
        }

        [TestMethod()]
        public async Task GetClientCommands_ReturnsOk_WhenCommandsAreFound()
        {

            var client = new Client { Id = 1, UserId = "test-user-id" };
            var commands = new List<Command> { new Command { Id = 1 } };
            _clientsServiceMock.Setup(s => s.GetClientFromUserId(It.IsAny<string>())).Returns(client);
            _commandsServiceMock.Setup(s => s.GetClientCommand(It.IsAny<int>())).Returns(commands);


            var result = await _controller.GetClientCommands();


            Assert.AreEqual(commands, result.Value);
        }

        [TestMethod()]
        public async Task GetAllAvailableCommands_ReturnsNotFound_WhenCommandsAreNull()
        {

            _commandsServiceMock.Setup(s => s.GetAvailableCommands()).ReturnsAsync((List<Command>)null);


            var result = await _controller.GetAllAvailableCommands();


            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
            Assert.AreEqual("Commandes introuvables ou aucune commande.", (result.Result as NotFoundObjectResult).Value);
        }

        [TestMethod()]
        public async Task GetAllAvailableCommands_ReturnsOk_WhenCommandsAreFound()
        {
            var commands = new List<Command> { new Command { Id = 1 } };
            _commandsServiceMock.Setup(s => s.GetAvailableCommands()).ReturnsAsync(commands);

            var result = await _controller.GetAllAvailableCommands();

            Assert.AreEqual(commands, result.Value);
        }

        [TestMethod()]
        public async Task GetMyDeliveries_ReturnsNotFound_WhenDeliveryManIsNull()
        {

            _commandsServiceMock.Setup(s => s.GetDeliveryManById(It.IsAny<string>())).Returns((DeliveryMan)null);


            var result = await _controller.GetMyDeliveries();


            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
            Assert.AreEqual("Le livreur est introuvable.", (result.Result as NotFoundObjectResult).Value);
        }

        [TestMethod()]
        public async Task GetMyDeliveries_ReturnsNotFound_WhenCommandsAreNull()
        {

            var deliveryMan = new DeliveryMan { Id = 1 };
            _commandsServiceMock.Setup(s => s.GetDeliveryManById(It.IsAny<string>())).Returns(deliveryMan);
            _commandsServiceMock.Setup(s => s.GetMyDeliveries(It.IsAny<int>())).ReturnsAsync((List<Command>)null);

            var result = await _controller.GetMyDeliveries();

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
            Assert.AreEqual("Commandes introuvables ou aucune commande.", (result.Result as NotFoundObjectResult).Value);
        }

        [TestMethod()]
        public async Task GetMyDeliveries_ReturnsOk_WhenCommandsAreFound()
        {

            var deliveryMan = new DeliveryMan { Id = 1 };
            var commands = new List<Command> { new Command { Id = 1 } };
            _commandsServiceMock.Setup(s => s.GetDeliveryManById(It.IsAny<string>())).Returns(deliveryMan);
            _commandsServiceMock.Setup(s => s.GetMyDeliveries(It.IsAny<int>())).ReturnsAsync(commands);

            var result = await _controller.GetMyDeliveries();

            Assert.AreEqual(commands, result.Value);
        }

        [TestMethod()]
        public async Task AssignADelivery_ReturnsNotFound_WhenDeliveryManIsNull()
        {
            _commandsServiceMock.Setup(s => s.GetDeliveryManById(It.IsAny<string>())).Returns((DeliveryMan)null);

            var result = await _controller.AssignADelivery(1);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
            Assert.AreEqual("Le livreur est introuvable.", (result.Result as NotFoundObjectResult).Value);
        }

        [TestMethod()]
        public async Task AssignADelivery_ReturnsOk_WhenAssignmentIsSuccessful()
        {
            var deliveryMan = new DeliveryMan { Id = 1 };
            _commandsServiceMock.Setup(s => s.GetDeliveryManById(It.IsAny<string>())).Returns(deliveryMan);
            _commandsServiceMock.Setup(s => s.AssignADelivery(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync("Assignation réussie.");

            var result = await _controller.AssignADelivery(1);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            Assert.AreEqual("Assignation réussie.", (result.Result as OkObjectResult).Value);
        }

        [TestMethod()]
        public async Task DeliveryInProgress_ReturnsNotFound_WhenDeliveryManIsNull()
        {
            // Arrange
            _commandsServiceMock.Setup(s => s.GetDeliveryManById(It.IsAny<string>())).Returns((DeliveryMan)null);

            // Act
            var result = await _controller.DeliveryInProgress(1);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
            Assert.AreEqual("Le livreur est introuvable.", (result.Result as NotFoundObjectResult).Value);
        }

        [TestMethod()]
        public async Task DeliveryInProgress_ReturnsOk_WhenDeliveryInProgressIsSuccessful()
        {
            // Arrange
            var deliveryMan = new DeliveryMan { Id = 1 };
            _commandsServiceMock.Setup(s => s.GetDeliveryManById(It.IsAny<string>())).Returns(deliveryMan);
            _commandsServiceMock.Setup(s => s.DeliveryInProgress(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync("Commande en cours de livraison. Chat initialisé.");

            // Act
            var result = await _controller.DeliveryInProgress(1);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            Assert.AreEqual("Commande en cours de livraison. Chat initialisé.", (result.Result as OkObjectResult).Value);
        }

        [TestMethod()]
        public async Task CommandDelivered_ReturnsNotFound_WhenDeliveryManIsNull()
        {
            _commandsServiceMock.Setup(s => s.GetDeliveryManById(It.IsAny<string>())).Returns((DeliveryMan)null);

            var result = await _controller.CommandDelivered(1);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
            Assert.AreEqual("Le livreur est introuvable.", (result.Result as NotFoundObjectResult).Value);
        }

        [TestMethod()]
        public async Task CommandDelivered_ReturnsOk_WhenDeliveryIsSuccessful()
        {

            var deliveryMan = new DeliveryMan { Id = 1 };
            _commandsServiceMock.Setup(s => s.GetDeliveryManById(It.IsAny<string>())).Returns(deliveryMan);
            _commandsServiceMock.Setup(s => s.CommandDelivered(It.IsAny<DeliveryMan>(), It.IsAny<int>())).ReturnsAsync("Livraison réussie. Chat terminé.");


            var result = await _controller.CommandDelivered(1);


            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            Assert.AreEqual("Livraison réussie. Chat terminé.", (result.Result as OkObjectResult).Value);
        }

        [TestMethod()]
        public async Task CancelADelivery_ReturnsNotFound_WhenDeliveryManIsNull()
        {

            _commandsServiceMock.Setup(s => s.GetDeliveryManById(It.IsAny<string>())).Returns((DeliveryMan)null);


            var result = await _controller.CancelADelivery(1);


            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
            Assert.AreEqual("Le livreur est introuvable.", (result.Result as NotFoundObjectResult).Value);
        }

        [TestMethod()]
        public async Task CancelADelivery_ReturnsOk_WhenCancellationIsSuccessful()
        {

            var deliveryMan = new DeliveryMan { Id = 1 };
            _commandsServiceMock.Setup(s => s.GetDeliveryManById(It.IsAny<string>())).Returns(deliveryMan);
            _commandsServiceMock.Setup(s => s.CancelADelivery(It.IsAny<DeliveryMan>(), It.IsAny<int>())).ReturnsAsync("Annulation réussie. Chat terminé.");


            var result = await _controller.CancelADelivery(1);


            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            Assert.AreEqual("Annulation réussie. Chat terminé.", (result.Result as OkObjectResult).Value);
        }
    }
}