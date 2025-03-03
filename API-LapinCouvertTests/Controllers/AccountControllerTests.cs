using Microsoft.VisualStudio.TestTools.UnitTesting;
using Admin_API.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MVC_LapinCouvert.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using Admin_API.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using API_LapinCouvert.DTOs;
using LapinCouvert.Models;

namespace Admin_API.Controllers.Tests
{
    [TestClass()]
    public class AccountControllerTests
    {
        private DbContextOptions<ApplicationDbContext> _options;
        private ApplicationDbContext _dbContext;
        private Mock<UserManager<IdentityUser>> _userManagerMock;
        private Mock<SignInManager<IdentityUser>> _signInManagerMock;
        private Mock<ClientsService> _clientsServiceMock;
        private AccountController _controller;

        [TestInitialize]
        public void Init()
        {
            // Initialize the In-Memory Database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new ApplicationDbContext(options);
            _dbContext.Database.EnsureDeleted();
            _dbContext.Database.EnsureCreated();

            // Mock dependencies
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);

            _signInManagerMock = new Mock<SignInManager<IdentityUser>>(
                _userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
                null, null, null, null);

            _clientsServiceMock = new Mock<ClientsService>(_dbContext);

            // Initialize the controller
            _controller = new AccountController(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _clientsServiceMock.Object
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

            Client clientSeed = new Client { UserId = "retard", Id = 10, Username = "Retardos", FirstName = "Ret", LastName = "ard"};
            _dbContext.Add(clientSeed);
            _dbContext.SaveChanges();

        }

        [TestMethod()]
        public async Task GetProfileInfoOkDeliveryMan()
        {
            Client clientTemp = new Client { UserId = "temp", Id = 10, FirstName = "Retard", LastName = "Autist", Username = "Hello", IsDeliveryMan = true, };

            _clientsServiceMock.Setup(s => s.GetClientFromUserId(It.IsAny<string>())).Returns(clientTemp);

            ProfileDTO profileDTO = new ProfileDTO
            {
                UserName = clientTemp.Username,
                FirstName = clientTemp.FirstName,
                LastName = clientTemp.LastName,
                ImgUrl = clientTemp.ImageURL,
                IsDeliveryMan = clientTemp.IsDeliveryMan,
                IsActiveAsDeliveryMan = false,
            };

            _clientsServiceMock.Setup(s => s.GetProfileInfo(It.IsAny<string>())).ReturnsAsync(profileDTO);

            ActionResult<ProfileDTO> result = await _controller.GetProfileInfo();

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult.Value);
            Assert.AreEqual(profileDTO, okResult.Value);
        }

        [TestMethod()]
        public async Task GetProfileInfoClientNotFound()
        {
            Client client = null;
            _clientsServiceMock.Setup(s => s.GetClientFromUserId(It.IsAny<string>())).Returns(client);

            _clientsServiceMock.Setup(s => s.GetProfileInfo(It.IsAny<string>())).Throws(new Exception("Client introuvable"));

            ActionResult<ProfileDTO> actionResultDTO = await _controller.GetProfileInfo();

            Assert.IsInstanceOfType(actionResultDTO.Result, typeof(BadRequestObjectResult));
            var badRequestResult = actionResultDTO.Result as BadRequestObjectResult;
            Assert.AreEqual("Erreur lors de l'obtention du profile", badRequestResult.Value);
        }

        [TestMethod()]
        public async Task GetProfileInfoDeliveryManNotFound()
        {
            Client client = new Client { UserId = "temp", Id = 10, FirstName = "Retard", LastName = "Autist", Username = "Hello", IsDeliveryMan = true, };
            _clientsServiceMock.Setup(s => s.GetClientFromUserId(It.IsAny<string>())).Returns(client);

            _clientsServiceMock.Setup(s => s.GetProfileInfo(It.IsAny<string>())).Throws(new Exception("DeliveryMan introuvable"));


            ActionResult<ProfileDTO> actionResultDTO = await _controller.GetProfileInfo();

            Assert.IsInstanceOfType(actionResultDTO.Result, typeof(BadRequestObjectResult));
            Assert.AreEqual("Erreur lors de l'obtention du profile", (actionResultDTO.Result as BadRequestObjectResult).Value);
        }

        [TestMethod()]
        public async Task GetProfileInfoOkClient()
        {

            Client clientTemp = new Client { UserId = "temp", Id = 10, FirstName = "Retard", LastName = "Autist", Username = "Hello", IsDeliveryMan = false, };

            _clientsServiceMock.Setup(s => s.GetClientFromUserId(It.IsAny<string>())).Returns(clientTemp);

            ProfileDTO profileDTO = new ProfileDTO
            {
                UserName = clientTemp.Username,
                FirstName = clientTemp.FirstName,
                LastName = clientTemp.LastName,
                ImgUrl = clientTemp.ImageURL,
                IsDeliveryMan = clientTemp.IsDeliveryMan,
                IsActiveAsDeliveryMan = false,
            };

            _clientsServiceMock.Setup(s => s.GetProfileInfo(It.IsAny<string>())).ReturnsAsync(profileDTO);

            ActionResult<ProfileDTO> result = await _controller.GetProfileInfo();

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            Assert.AreEqual(profileDTO, (result.Result as OkObjectResult).Value);
        }

        [TestMethod()]
        public async Task ModifyImageProfileUpdateFail()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("profile_image.png");
            mockFile.Setup(f => f.Length).Returns(1024);

            var mockStream = new MemoryStream(new byte[] { 0x20, 0x20, 0x20 });
            mockFile.Setup(f => f.OpenReadStream()).Returns(mockStream);

            Client client = new Client { Id = 1, UserId = "1" };

            // Mock successful client retrieval
            _clientsServiceMock.Setup(s => s.GetClientFromUserId(It.IsAny<string>())).Returns(client);

            // Mock image saving logic (adjust the mock behavior as needed)
            _clientsServiceMock.Setup(s => s.UpdateClient(It.IsAny<Client>())).Throws(new Exception("Client non trouvé dans la base de données"));

            // Act
            ActionResult result = await _controller.ModifyImage(mockFile.Object);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            Assert.AreEqual("Client non trouvé dans la base de données", (result as NotFoundObjectResult).Value);
        }


        [TestMethod()]
        public async Task ModifyImageProfileOk()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("profile_image.png");
            mockFile.Setup(f => f.Length).Returns(1024);

            var mockStream = new MemoryStream(new byte[] { 0x20, 0x20, 0x20 });
            mockFile.Setup(f => f.OpenReadStream()).Returns(mockStream);

            Client client = new Client { Id = 1, UserId = "1" };

            // Mock successful client retrieval
            _clientsServiceMock.Setup(s => s.GetClientFromUserId(It.IsAny<string>())).Returns(client);

            // Mock image saving logic (adjust the mock behavior as needed)
            _clientsServiceMock.Setup(s => s.UpdateClient(It.IsAny<Client>()));

            // Act
            var result = await _controller.ModifyImage(mockFile.Object) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("Profile image updated successfully.", result.Value);
        }

        [TestMethod()]
        public async Task ModifyImageClientDoesntExist()
        {
            Client client = null;
            var mockFile = new Mock<IFormFile>();

            // Mock service returning no client found for the user ID
            _clientsServiceMock.Setup(s => s.GetClientFromUserId(It.IsAny<string>())).Returns(client);

            // Act
            ActionResult result = await _controller.ModifyImage(mockFile.Object);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            Assert.AreEqual("Client introuvable", (result as NotFoundObjectResult).Value);
        }

        [TestMethod()]
        public async Task ModifyImageNoFileProvided()
        {
            Client client = new Client { Id = 1, UserId = "1" };

            _clientsServiceMock.Setup(s => s.GetClientFromUserId(It.IsAny<string>())).Returns(client);

            ActionResult result = await _controller.ModifyImage(null) as ActionResult;

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            Assert.AreEqual("No file provided.", (result as BadRequestObjectResult).Value);
        }


        [TestCleanup]
        public void Cleanup()
        {
            // Clean up the database after each test
            _dbContext.Database.EnsureDeleted();
        }

        [TestMethod()]
        public async Task ModifyProfileProfileDTOIsNull()
        {
            ProfileModificationDTO profileDTO = null;
            var result = await _controller.ModifyProfile(profileDTO);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            Assert.AreEqual("Profile data is invalid.", (result.Result as BadRequestObjectResult).Value);
        }


        [TestMethod()]
        public async Task ModifyProfileClientDoesntExist()
        {
            ProfileModificationDTO profileDTO = new ProfileModificationDTO
            {
                NewFirstName = "Retard",
                NewLastName = "Autist",
                NewPassword = "Deez",
                OldPassword = "Nuts",
            };
            Client client = null;
            _clientsServiceMock.Setup(s => s.GetClientFromUserId(It.IsAny<string>())).Returns(client);

            var result = await _controller.ModifyProfile(profileDTO);


            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
            Assert.AreEqual("Client introuvable", (result.Result as NotFoundObjectResult).Value);
        }


        [TestMethod()]
        public async Task ModifyProfileUserDoesntExist()
        {
            ProfileModificationDTO profileDTO = new ProfileModificationDTO
            {
                NewFirstName = "Retard",
                NewLastName = "Autist",
                NewPassword = "Deez",
                OldPassword = "Nuts",
            };

            Client client = new Client { Id = 1, UserId = "1" };
            IdentityUser identityUser = null;
            _clientsServiceMock.Setup(s => s.GetClientFromUserId(It.IsAny<string>())).Returns(client);
            _userManagerMock.Setup(s => s.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(identityUser);


            var result = await _controller.ModifyProfile(profileDTO);


            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
            Assert.AreEqual("User introuvable", (result.Result as NotFoundObjectResult).Value);
        }


        [TestMethod()]
        public async Task ModifyProfileBadPassword()
        {
            ProfileModificationDTO profileDTO = new ProfileModificationDTO
            {
                NewFirstName = "Retard",
                NewLastName = "Autist",
                NewPassword = "Deez",
                OldPassword = "Nuts",
            };

            Client client = new Client { Id = 1, UserId = "1" };
            IdentityUser identityUser = new IdentityUser();
            IdentityResult identityResult = new IdentityResult();
            _clientsServiceMock.Setup(s => s.GetClientFromUserId(It.IsAny<string>())).Returns(client);
            _userManagerMock.Setup(s => s.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(identityUser);
            _userManagerMock.Setup(s => s.ChangePasswordAsync(identityUser, profileDTO.OldPassword, profileDTO.NewPassword)).ReturnsAsync(identityResult);


            var result = await _controller.ModifyProfile(profileDTO);


            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            Assert.AreEqual("Échec du changement de mot de passe", (result.Result as BadRequestObjectResult).Value);
        }

        [TestMethod()]
        public async Task ModifyProfileUpdateFail()
        {
            ProfileModificationDTO profileDTO = new ProfileModificationDTO
            {
                NewFirstName = "Retard",
                NewLastName = "Autist",
                NewPassword = "Deez",
                OldPassword = "Nuts",
            };

            Client client = new Client { Id = 1, UserId = "1" };
            IdentityUser identityUser = new IdentityUser();
            IdentityResult identityResult = IdentityResult.Success;
            _clientsServiceMock.Setup(s => s.GetClientFromUserId(It.IsAny<string>())).Returns(client);
            _userManagerMock.Setup(s => s.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(identityUser);
            _userManagerMock.Setup(s => s.ChangePasswordAsync(identityUser, profileDTO.OldPassword, profileDTO.NewPassword)).ReturnsAsync(identityResult);

            _clientsServiceMock.Setup(s => s.UpdateClient(It.IsAny<Client>()))
                .Throws(new Exception("Client non trouvé dans la base de données"));


            ActionResult<ProfileDTO> result = await _controller.ModifyProfile(profileDTO);


            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
            Assert.AreEqual("Client non trouvé dans la base de données", (result.Result as NotFoundObjectResult).Value);
        }

        [TestMethod()]
        public async Task ModifyProfileFailedToObtainAsClient()
        {
            ProfileModificationDTO profileDTO = new ProfileModificationDTO
            {
                NewFirstName = "Retard",
                NewLastName = "Autist",
                NewPassword = "Deez",
                OldPassword = "Nuts",
            };

            Client client = new Client { Id = 1, UserId = "1" };
            IdentityUser identityUser = new IdentityUser();
            IdentityResult identityResult = IdentityResult.Success;
            _clientsServiceMock.Setup(s => s.GetClientFromUserId(It.IsAny<string>())).Returns(client);
            _userManagerMock.Setup(s => s.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(identityUser);
            _userManagerMock.Setup(s => s.ChangePasswordAsync(identityUser, profileDTO.OldPassword, profileDTO.NewPassword)).ReturnsAsync(identityResult);

            _clientsServiceMock.Setup(s => s.UpdateClient(It.IsAny<Client>()));

            _clientsServiceMock.Setup(s => s.GetProfileInfo(null)).Throws(new Exception("Client introuvable"));


            ActionResult<ProfileDTO> result = await _controller.ModifyProfile(profileDTO);


            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            Assert.AreEqual("Erreur lors de l'obtention du profile", (result.Result as BadRequestObjectResult).Value);
        }

        [TestMethod()]
        public async Task ModifyProfileFailedToObtainAsDeliveryMan()
        {
            ProfileModificationDTO profileDTO = new ProfileModificationDTO
            {
                NewFirstName = "Retard",
                NewLastName = "Autist",
                NewPassword = "Deez",
                OldPassword = "Nuts",
            };

            Client client = new Client { Id = 1, UserId = "1", IsDeliveryMan = true };
            IdentityUser identityUser = new IdentityUser();
            IdentityResult identityResult = IdentityResult.Success;
            _clientsServiceMock.Setup(s => s.GetClientFromUserId(It.IsAny<string>())).Returns(client);
            _userManagerMock.Setup(s => s.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(identityUser);
            _userManagerMock.Setup(s => s.ChangePasswordAsync(identityUser, profileDTO.OldPassword, profileDTO.NewPassword)).ReturnsAsync(identityResult);

            _clientsServiceMock.Setup(s => s.UpdateClient(It.IsAny<Client>()));

            _clientsServiceMock.Setup(s => s.getClientDeliverMan(It.IsAny<int>())).Throws(new Exception("Pas de Livreur trouvée"));

            //_clientsServiceMock.Setup(s => s.GetProfileInfo(It.IsAny<string>())).Throws(new Exception("Livreur introuvable"));


            ActionResult<ProfileDTO> result = await _controller.ModifyProfile(profileDTO);


            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            Assert.AreEqual("Erreur lors de l'obtention du profile", (result.Result as BadRequestObjectResult).Value);
        }


    }
}