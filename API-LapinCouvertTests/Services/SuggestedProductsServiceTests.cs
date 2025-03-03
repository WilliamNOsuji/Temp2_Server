using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            // In-memory DB options
            options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .UseLazyLoadingProxies(true)
                .Options;
        }

        [TestInitialize]
        public void Init()
        {
            _db = new ApplicationDbContext(options);

            // Seed data
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
                NormalizedEmail = "USER@USER.COM",
                NormalizedUserName = "USER",
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null, "Passw0rd!"),
                LockoutEnabled = true,
            },
            new IdentityUser
            {
                Id = "User2Id",
                UserName = "User2",
                Email = "user2@user.com",
                NormalizedEmail = "USER2@USER.COM",
                NormalizedUserName = "USER2",
                EmailConfirmed = true,
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
            _db.Dispose();
        }

        [TestMethod]
        public async Task AddVoteForTest()
        {
            var service = new SuggestedProductsService(_db);
            var clientId = _db.Clients.First(c => c.UserId == "User1Id").Id;
            VoteRequestDTO vr = new VoteRequestDTO { Id = 1 };

            await service.VoteFor(clientId, vr.Id);
            var product = _db.SuggestedProducts.FirstOrDefault(s => s.Id == vr.Id);


            Assert.AreEqual(1, product.Votes.Count(v => v.IsFor));
        }

        [TestMethod]
        public async Task AddVoteAgainstTest()
        {
            var service = new SuggestedProductsService(_db);
            var clientId = _db.Clients.First(c => c.UserId == "User1Id").Id;
            VoteRequestDTO vr = new VoteRequestDTO { Id = 1 };

            await service.VoteAgainst(clientId, vr.Id);
            var product = _db.SuggestedProducts.FirstOrDefault(s => s.Id == vr.Id);

            Assert.AreEqual(1, product.Votes.Count(v => !v.IsFor));
        }

        [TestMethod]
        public async Task AddVoteFor2Time_SameClient_Test()
        {
            // Arrange
            var service = new SuggestedProductsService(_db);
            var clientId = _db.Clients.First(c => c.UserId == "User1Id").Id;
            VoteRequestDTO vr = new VoteRequestDTO { Id = 1 };

            await service.VoteFor(clientId, vr.Id);
            await service.VoteFor(clientId, vr.Id);

            var product = _db.SuggestedProducts.FirstOrDefault(s => s.Id == vr.Id);

            Assert.AreEqual(0, product.Votes.Count(v => v.IsFor));
        }

        [TestMethod]
        public async Task AddVoteAgainst2Time_SameClient_Test()
        {
            var service = new SuggestedProductsService(_db);
            var clientId = _db.Clients.First(c => c.UserId == "User1Id").Id;
            VoteRequestDTO vr = new VoteRequestDTO { Id = 1 };

            await service.VoteAgainst(clientId, vr.Id);
            await service.VoteAgainst(clientId, vr.Id);

            var product = _db.SuggestedProducts.FirstOrDefault(s => s.Id == vr.Id);

            Assert.AreEqual(0, product.Votes.Count(v => !v.IsFor));
        }

        [TestMethod]
        public async Task AddVoteForThenAgainstTest()
        {
            // Arrange
            var service = new SuggestedProductsService(_db);
            var clientId = _db.Clients.First(c => c.UserId == "User1Id").Id;
            VoteRequestDTO vr = new VoteRequestDTO { Id = 1 };

            await service.VoteFor(clientId, vr.Id);
            await service.VoteAgainst(clientId, vr.Id);

            var product = _db.SuggestedProducts.FirstOrDefault(s => s.Id == vr.Id);

            Assert.AreEqual(0, product.Votes.Count(v => v.IsFor));
            Assert.AreEqual(1, product.Votes.Count(v => !v.IsFor));
        }

        [TestMethod]
        public async Task AddVoteAgainstThenForTest()
        {
            var service = new SuggestedProductsService(_db);
            var clientId = _db.Clients.First(c => c.UserId == "User1Id").Id;
            VoteRequestDTO vr = new VoteRequestDTO { Id = 1 };

            await service.VoteAgainst(clientId, vr.Id);
            await service.VoteFor(clientId, vr.Id);

            var product = _db.SuggestedProducts.FirstOrDefault(s => s.Id == vr.Id);

            Assert.AreEqual(1, product.Votes.Count(v => v.IsFor));
            Assert.AreEqual(0, product.Votes.Count(v => !v.IsFor));
        }

        [TestMethod]
        public async Task AddVoteFor_MultipleClients_Test()
        {
            var service = new SuggestedProductsService(_db);
            var clientId1 = _db.Clients.First(c => c.UserId == "User1Id").Id;
            var clientId2 = _db.Clients.First(c => c.UserId == "User2Id").Id;
            VoteRequestDTO vr = new VoteRequestDTO { Id = 1 };

            await service.VoteFor(clientId1, vr.Id);
            await service.VoteFor(clientId2, vr.Id);

            var product = _db.SuggestedProducts.FirstOrDefault(s => s.Id == vr.Id);

            Assert.AreEqual(2, product.Votes.Count(v => v.IsFor));
        }

        [TestMethod]
        public async Task AddVoteAgainst_MultipleClients_Test()
        {
            var service = new SuggestedProductsService(_db);
            var clientId1 = _db.Clients.First(c => c.UserId == "User1Id").Id;
            var clientId2 = _db.Clients.First(c => c.UserId == "User2Id").Id;
            VoteRequestDTO vr = new VoteRequestDTO { Id = 1 };

            await service.VoteAgainst(clientId1, vr.Id);
            await service.VoteAgainst(clientId2, vr.Id);

            var product = _db.SuggestedProducts.FirstOrDefault(s => s.Id == vr.Id);

            Assert.AreEqual(2, product.Votes.Count(v => !v.IsFor));
        }
    }


}