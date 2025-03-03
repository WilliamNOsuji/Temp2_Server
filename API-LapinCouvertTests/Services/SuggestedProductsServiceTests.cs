﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using API_LapinCouvert.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LapinCouvert.Models;
using Microsoft.EntityFrameworkCore;
using MVC_LapinCouvert.Data;
using API_LapinCouvert.DTOs;
using Moq;
using Microsoft.AspNetCore.Identity;
using MVC_LapinCouvert.Services;
using ClientsService = Admin_API.Services.ClientsService;

namespace API_LapinCouvert.Services.Tests
{
    [TestClass()]
    public class SuggestedProductsServiceTests
    {
        private ApplicationDbContext _db;
        private DbContextOptions<ApplicationDbContext> options;


        public SuggestedProductsServiceTests()
        {
            // TODO On initialise les options de la BD, on utilise une InMemoryDatabase
            options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .UseLazyLoadingProxies(true)
                .Options;
        }

        [TestInitialize]
        public void Init()
        {

            // TODO avoir la durée de vie d'un context la plus petite possible
            _db = new ApplicationDbContext(options);
            // TODO on ajoute des données de tests
            SuggestedProduct[] suggestedProducts = new SuggestedProduct[]
            {
                new SuggestedProduct
                {
                    Id = 1,
                    Name = "Chat Dragon",
                    Photo = "https://i.pinimg.com/originals/a8/16/49/a81649bd4b0f032ce633161c5a076b87.jpg"
                },
                new SuggestedProduct
                {
                    Id = 2,
                    Name = "Chat Awesome",
                    Photo = "https://i0.wp.com/thediscerningcat.com/wp-content/uploads/2021/02/tabby-cat-wearing-sunglasses.jpg"
                }
            };

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


            _db.AddRange(suggestedProducts);
            _db.AddRange(identityUsers);
            _db.AddRange(clients);
            _db.SaveChanges();

        }

        [TestCleanup]
        public void Dispose()
        {
            //TODO on efface les données de tests pour remettre la BD dans son état initial
            _db.Dispose();
        }

        [TestMethod]
        public async Task AddVoteForTest()
        {
            Mock<UserIdGetService> serviceMock = new Mock<UserIdGetService>();
            Mock<ClientsService> serviceClientMock = new Mock<ClientsService>();
            serviceMock.Setup(s => s.getUserId()).Returns("User1Id");
            serviceClientMock.Setup(s => s.GetClientFromUserId("User1Id")).Returns(It.IsAny<Client>());
            SuggestedProductsService service = new SuggestedProductsService(_db);

            VoteRequestDTO vr = new VoteRequestDTO
            {
                Id = 1,
            };

            await service.VoteFor(_db.Clients.Where(c => c.UserId == "User1Id").FirstOrDefault(), vr.Id);

            SuggestedProduct product = _db.SuggestedProducts.Where(s => s.Id == vr.Id).FirstOrDefault();


            Assert.AreEqual(1, product.ForClients.Count);
        }

        [TestMethod]
        public async Task AddVoteAgainstTest()
        {
            Mock<UserIdGetService> serviceMock = new Mock<UserIdGetService>();
            Mock<ClientsService> serviceClientMock = new Mock<ClientsService>();
            serviceMock.Setup(s => s.getUserId()).Returns("User1Id");
            serviceClientMock.Setup(s => s.GetClientFromUserId("User1Id")).Returns(It.IsAny<Client>());
            SuggestedProductsService service = new SuggestedProductsService(_db);

            VoteRequestDTO vr = new VoteRequestDTO
            {
                Id = 1,
            };

            await service.VoteAgainst(_db.Clients.Where(c => c.UserId == "User1Id").FirstOrDefault(), vr.Id);

            SuggestedProduct product = _db.SuggestedProducts.Where(s => s.Id == vr.Id).FirstOrDefault();


            Assert.AreEqual(1, product.AgainstClients.Count);
        }

        [TestMethod]
        public async Task AddVoteFor2Time_SameClient_Test()
        {
            Mock<UserIdGetService> serviceMock = new Mock<UserIdGetService>();
            Mock<ClientsService> serviceClientMock = new Mock<ClientsService>();
            serviceMock.Setup(s => s.getUserId()).Returns("User1Id");
            serviceClientMock.Setup(s => s.GetClientFromUserId("User1Id")).Returns(It.IsAny<Client>());
            SuggestedProductsService service = new SuggestedProductsService(_db);

            VoteRequestDTO vr = new VoteRequestDTO
            {
                Id = 1,
            };

            await service.VoteFor(_db.Clients.Where(c => c.UserId == "User1Id").FirstOrDefault(), vr.Id);
            await service.VoteFor(_db.Clients.Where(c => c.UserId == "User1Id").FirstOrDefault(), vr.Id);

            SuggestedProduct product = _db.SuggestedProducts.Where(s => s.Id == vr.Id).FirstOrDefault();


            Assert.AreEqual(0, product.ForClients.Count);
        }

        [TestMethod]
        public async Task AddVoteAgainst2Time_SameClient_Test()
        {
            Mock<UserIdGetService> serviceMock = new Mock<UserIdGetService>();
            Mock<ClientsService> serviceClientMock = new Mock<ClientsService>();
            serviceMock.Setup(s => s.getUserId()).Returns("User1Id");
            serviceClientMock.Setup(s => s.GetClientFromUserId("User1Id")).Returns(It.IsAny<Client>());
            SuggestedProductsService service = new SuggestedProductsService(_db);

            VoteRequestDTO vr = new VoteRequestDTO
            {
                Id = 1,
            };

            await service.VoteAgainst(_db.Clients.Where(c => c.UserId == "User1Id").FirstOrDefault(), vr.Id);
            await service.VoteAgainst(_db.Clients.Where(c => c.UserId == "User1Id").FirstOrDefault(), vr.Id);

            SuggestedProduct product = _db.SuggestedProducts.Where(s => s.Id == vr.Id).FirstOrDefault();


            Assert.AreEqual(0, product.AgainstClients.Count);
        }

        [TestMethod]
        public async Task AddVoteForThenAgainstTest()
        {

            Mock<UserIdGetService> serviceMock = new Mock<UserIdGetService>();
            Mock<ClientsService> serviceClientMock = new Mock<ClientsService>();
            serviceMock.Setup(s => s.getUserId()).Returns("User1Id");
            serviceClientMock.Setup(s => s.GetClientFromUserId("User1Id")).Returns(It.IsAny<Client>());
            SuggestedProductsService service = new SuggestedProductsService(_db);

            VoteRequestDTO vr = new VoteRequestDTO
            {
                Id = 1,
            };

            await service.VoteFor(_db.Clients.Where(c => c.UserId == "User1Id").FirstOrDefault(), vr.Id);
            await service.VoteAgainst(_db.Clients.Where(c => c.UserId == "User1Id").FirstOrDefault(), vr.Id);

            SuggestedProduct product = _db.SuggestedProducts.Where(s => s.Id == vr.Id).FirstOrDefault();

            Assert.AreEqual(0, product.ForClients.Count);
            Assert.AreEqual(1, product.AgainstClients.Count);
        }

        [TestMethod]
        public async Task AddVoteAgainstThenForTest()
        {
            Mock<UserIdGetService> serviceMock = new Mock<UserIdGetService>();
            Mock<ClientsService> serviceClientMock = new Mock<ClientsService>();
            serviceMock.Setup(s => s.getUserId()).Returns("User1Id");
            serviceClientMock.Setup(s => s.GetClientFromUserId("User1Id")).Returns(It.IsAny<Client>());
            SuggestedProductsService service = new SuggestedProductsService(_db);

            VoteRequestDTO vr = new VoteRequestDTO
            {
                Id = 1,
            };

            await service.VoteAgainst(_db.Clients.Where(c => c.UserId == "User1Id").FirstOrDefault(), vr.Id);
            await service.VoteFor(_db.Clients.Where(c => c.UserId == "User1Id").FirstOrDefault(), vr.Id);

            SuggestedProduct product = _db.SuggestedProducts.Where(s => s.Id == vr.Id).FirstOrDefault();

            Assert.AreEqual(1, product.ForClients.Count);
            Assert.AreEqual(0, product.AgainstClients.Count);
        }

        [TestMethod]
        public async Task AddVoteFor_MultipleClients_Test()
        {
            Mock<UserIdGetService> serviceMock = new Mock<UserIdGetService>();
            Mock<ClientsService> serviceClientMock = new Mock<ClientsService>();
            serviceMock.Setup(s => s.getUserId()).Returns("User1Id");
            serviceClientMock.Setup(s => s.GetClientFromUserId("User1Id")).Returns(It.IsAny<Client>());
            SuggestedProductsService service = new SuggestedProductsService(_db);

            VoteRequestDTO vr = new VoteRequestDTO
            {
                Id = 1,
            };

            await service.VoteFor(_db.Clients.Where(c => c.UserId == "User1Id").FirstOrDefault(), vr.Id);
            await service.VoteFor(_db.Clients.Where(c => c.UserId == "User2Id").FirstOrDefault(), vr.Id);

            SuggestedProduct product = _db.SuggestedProducts.Where(s => s.Id == vr.Id).FirstOrDefault();

            Assert.AreEqual(2, product.ForClients.Count);
        }

        [TestMethod]
        public async Task AddVoteAgainst_MultipleClients_Test()
        {
            Mock<UserIdGetService> serviceMock = new Mock<UserIdGetService>();
            Mock<ClientsService> serviceClientMock = new Mock<ClientsService>();
            serviceMock.Setup(s => s.getUserId()).Returns("User1Id");
            serviceClientMock.Setup(s => s.GetClientFromUserId("User1Id")).Returns(It.IsAny<Client>());
            SuggestedProductsService service = new SuggestedProductsService(_db);

            VoteRequestDTO vr = new VoteRequestDTO
            {
                Id = 1,
            };

            await service.VoteAgainst(_db.Clients.Where(c => c.UserId == "User1Id").FirstOrDefault(), vr.Id);
            await service.VoteAgainst(_db.Clients.Where(c => c.UserId == "User2Id").FirstOrDefault(), vr.Id);

            SuggestedProduct product = _db.SuggestedProducts.Where(s => s.Id == vr.Id).FirstOrDefault();

            Assert.AreEqual(2, product.AgainstClients.Count);
        }
    }
}