
using LapinCouvert.Models;
using Microsoft.AspNetCore.Identity;
using MVC_LapinCouvert.Data;
using System.Numerics;
namespace MVC_LapinCouvert.Services
{
    public class ClientsService
    {
        private ApplicationDbContext _dbContext;
        
        public ClientsService(ApplicationDbContext context)
        {
            _dbContext = context;
        }
        
        public Client CreateClient(IdentityUser user)
        {
            Client client = new Client()
            {
                Id = 0,
                UserId = user.Id,
                Username = user.UserName,
                FirstName = "", // Set default or collect from form
                LastName = "", // Set default or collect from form
                IsBanned = false,
                IsDeliveryMan = false,
                ImageURL = ""
            };

            _dbContext.Add(client);
            _dbContext.SaveChanges();

            Cart cart = new Cart()
            {
                ClientId = client.Id,
            };

            _dbContext.Add(cart);
            _dbContext.SaveChanges();
            return client;
        }
        
        public virtual Client GetClientFromUserId(string userId)
        {
            return _dbContext.Clients.Single(c => c.UserId == userId);
        }
        
        public Client GetClientFromUserName(string userName)
        {
            return _dbContext.Clients.Single(c => c.User!.UserName == userName);
        }

        // New method to update the client
        public void UpdateClient(Client client)
        {
            // Attach the client entity to the context if it's not already tracked
            var existingClient = _dbContext.Clients.Local.FirstOrDefault(c => c.Id == client.Id);
            if (existingClient == null)
            {
                _dbContext.Attach(client);
            }

            // Mark the entity as modified
            _dbContext.Entry(client).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            // Save changes to the database
            _dbContext.SaveChanges();
        }
    }
}
