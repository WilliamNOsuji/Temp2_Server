using Microsoft.VisualStudio.TestTools.UnitTesting;
using API_LapinCouvert.Controllers;
using API_LapinCouvert.DTOs;
using API_LapinCouvert.Services;
using LapinCouvert.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Admin_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MVC_LapinCouvert.Data;

namespace API_LapinCouvert.Controllers.Tests
{
    [TestClass]
    public class SuggestedProductsControllerTests
    {
        private DbContextOptions<ApplicationDbContext> _options;
        private ApplicationDbContext _dbContext;
        private Mock<UserIdGetService> _userIdGetService;
        private Mock<ClientsService> _clientsServiceMock;
        private Mock<SuggestedProductsService> _suggestProductsServiceMock;
        private SuggestedProductsController _controller;
        private Mock<IHttpContextAccessor> _httpContextAccessor;

        [TestInitialize]
        public void Init()
        {
            _clientsServiceMock = new Mock<ClientsService>(_dbContext);
            _suggestProductsServiceMock = new Mock<SuggestedProductsService>(_dbContext);

            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _userIdGetService = new Mock<UserIdGetService>(_httpContextAccessor.Object);

            _controller = new SuggestedProductsController(
                _suggestProductsServiceMock.Object,
                _clientsServiceMock.Object,
                _userIdGetService.Object
            );

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id")
            }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        #region GetSuggestedProducts Tests

        [TestMethod]
        public async Task GetSuggestedProducts_ReturnsOk_WithProductDtos()
        {
            var expectedProducts = new List<SuggestedProductDTO>
        {
            new SuggestedProductDTO
            {
                Id = 1,
                Name = "Produit A",
                Photo = "https://example.com/a.png",
                FinishDate = DateTime.UtcNow.AddDays(5)
            },
            new SuggestedProductDTO
            {
                Id = 2,
                Name = "Produit B",
                Photo = "https://example.com/b.png",
                FinishDate = DateTime.UtcNow.AddDays(10)
            }
        };

            _userIdGetService.Setup(s => s.getUserId()).Returns("test-user-id");
            _suggestProductsServiceMock
                .Setup(s => s.GetSuggestedProducts("test-user-id"))
                .ReturnsAsync(expectedProducts);

            ActionResult actionResult = await _controller.GetSuggestedProducts();
            var okResult = actionResult as OkObjectResult;

            Assert.IsNotNull(okResult, "Expected an OkObjectResult.");
            Assert.AreEqual(expectedProducts, okResult.Value);
        }

        [TestMethod]
        public async Task GetSuggestedProducts_UserIdNull_ThrowsException()
        {
            _userIdGetService.Setup(s => s.getUserId()).Returns((string)null);

            ActionResult actionResult = await _controller.GetSuggestedProducts();
            var notFoundResult = actionResult as NotFoundObjectResult;

            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual("Identifiant utilisateur non trouvé.", notFoundResult.Value);
        }

        [TestMethod]
        public async Task GetSuggestedProducts_ServiceThrows_ReturnsNotFound()
        {
            _userIdGetService.Setup(s => s.getUserId()).Returns("test-user-id");
            _suggestProductsServiceMock
                .Setup(s => s.GetSuggestedProducts("test-user-id"))
                .ThrowsAsync(new Exception("Service error"));

            ActionResult actionResult = await _controller.GetSuggestedProducts();
            var notFoundResult = actionResult as NotFoundObjectResult;

            Assert.IsNotNull(notFoundResult, "Expected a NotFoundObjectResult.");
            Assert.AreEqual("Erreur lors de la récupération des produits suggérés", notFoundResult.Value);
        }

        #endregion

        #region VoteFor Tests

        [TestMethod]
        public async Task VoteFor_Success_ReturnsOk()
        {
            int suggestedProductId = 100;
            var client = new Client { Id = 1, Username = "Client Test", UserId = "1" };

            _userIdGetService.Setup(s => s.getUserId()).Returns("test-user-id");
            _clientsServiceMock.Setup(s => s.GetClientFromUserId("test-user-id")).Returns(client);

            _suggestProductsServiceMock
                .Setup(s => s.VoteFor(client.Id, suggestedProductId))
                .ReturnsAsync(true);

            ActionResult<SuggestedProduct> actionResult = await _controller.VoteFor(suggestedProductId);

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkResult));
        }

        [TestMethod]
        public async Task VoteFor_UserIdNull_ReturnsNotFound()
        {
            int suggestedProductId = 100;
            _userIdGetService.Setup(s => s.getUserId()).Returns((string)null);

            ActionResult<SuggestedProduct> actionResult = await _controller.VoteFor(suggestedProductId);

            var notFoundResult = actionResult.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual("Identifiant utilisateur non trouvé.", notFoundResult.Value);
        }

        [TestMethod]
        public async Task VoteFor_ClientNull_ReturnsNotFound()
        {
            int suggestedProductId = 100;
            _userIdGetService.Setup(s => s.getUserId()).Returns("test-user-id");
            _clientsServiceMock.Setup(s => s.GetClientFromUserId("test-user-id")).Returns((Client)null);

            ActionResult<SuggestedProduct> actionResult = await _controller.VoteFor(suggestedProductId);

            var notFoundResult = actionResult.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual("Client non trouvé pour cet utilisateur.", notFoundResult.Value);
        }

        [TestMethod]
        public async Task VoteFor_Exception_ReturnsBadRequest()
        {
            int suggestedProductId = 100;
            var client = new Client { Id = 1, Username = "Client Test", UserId = "1" };

            _userIdGetService.Setup(s => s.getUserId()).Returns("test-user-id");
            _clientsServiceMock.Setup(s => s.GetClientFromUserId("test-user-id")).Returns(client);

            _suggestProductsServiceMock
                .Setup(s => s.VoteFor(client.Id, suggestedProductId))
                .ThrowsAsync(new Exception("Vote error"));

            ActionResult<SuggestedProduct> actionResult = await _controller.VoteFor(suggestedProductId);

            var badRequestResult = actionResult.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Erreur lors du vote pour le produit suggéré", badRequestResult.Value);
        }

        #endregion

        #region VoteAgainst Tests

        [TestMethod]
        public async Task VoteAgainst_Success_ReturnsOk()
        {
            // Arrange
            int suggestedProductId = 200;
            var client = new Client { Id = 2, Username = "Client Test 2", UserId = "2" };

            _userIdGetService.Setup(s => s.getUserId()).Returns("test-user-id");
            _clientsServiceMock.Setup(s => s.GetClientFromUserId("test-user-id")).Returns(client);

            _suggestProductsServiceMock
                .Setup(s => s.VoteAgainst(client.Id, suggestedProductId))
                .ReturnsAsync(true);

            ActionResult<SuggestedProduct> actionResult = await _controller.VoteAgainst(suggestedProductId);

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkResult));
        }

        [TestMethod]
        public async Task VoteAgainst_UserIdNull_ReturnsNotFound()
        {
            int suggestedProductId = 200;
            _userIdGetService.Setup(s => s.getUserId()).Returns((string)null);

            ActionResult<SuggestedProduct> actionResult = await _controller.VoteAgainst(suggestedProductId);

            var notFoundResult = actionResult.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual("Identifiant utilisateur non trouvé.", notFoundResult.Value);
        }

        [TestMethod]
        public async Task VoteAgainst_ClientNull_ReturnsNotFound()
        {
            int suggestedProductId = 200;
            _userIdGetService.Setup(s => s.getUserId()).Returns("test-user-id");
            _clientsServiceMock.Setup(s => s.GetClientFromUserId("test-user-id")).Returns((Client)null);

            ActionResult<SuggestedProduct> actionResult = await _controller.VoteAgainst(suggestedProductId);

            var notFoundResult = actionResult.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual("Client non trouvé pour cet utilisateur.", notFoundResult.Value);
        }

        [TestMethod]
        public async Task VoteAgainst_Exception_ReturnsBadRequest()
        {
            int suggestedProductId = 200;
            var client = new Client { Id = 2, Username = "Client Test 2", UserId = "2" };

            _userIdGetService.Setup(s => s.getUserId()).Returns("test-user-id");
            _clientsServiceMock.Setup(s => s.GetClientFromUserId("test-user-id")).Returns(client);

            _suggestProductsServiceMock
                .Setup(s => s.VoteAgainst(client.Id, suggestedProductId))
                .ThrowsAsync(new Exception("Produit suggéré introuvable."));

            ActionResult<SuggestedProduct> actionResult = await _controller.VoteAgainst(suggestedProductId);

            var badRequestResult = actionResult.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Erreur lors du vote contre le produit suggéré", badRequestResult.Value);
        }

        #endregion
    }

}
