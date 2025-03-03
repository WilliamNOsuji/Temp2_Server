using API_LapinCouvert.DTOs;
using LapinCouvert.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MVC_LapinCouvert.Data;
using WebAPI.DTOs;

namespace Admin_API.Services
{
    public class ClientsService
    {
        private ApplicationDbContext _dbContext;

        public ClientsService(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        public virtual async Task<Client> CreateClient(IdentityUser user, RegisterDTO registerDTO)
        {
                Client client = new Client()
                {
                    Id = 0,
                    UserId = user.Id,
                    Username = user.UserName,
                    FirstName = registerDTO.FirstName,
                    LastName = registerDTO.LastName,
                };

                await _dbContext.AddAsync(client);
                await _dbContext.SaveChangesAsync();

                Cart cart = new Cart()
                {
                    ClientId = client.Id,
                };

                await _dbContext.AddAsync(cart);
                await _dbContext.SaveChangesAsync();
                return client;
        }

        public virtual async Task<DeliveryMan> getClientDeliverMan(int clientId)
        {
            try
            {
                return await _dbContext.DeliveryMans.SingleAsync(c => c.ClientId == clientId);
            }
            catch (Exception)
            {
                throw new Exception("Pas de Livreur trouvée");
            }
                

        }

        public virtual async Task<Client> GetClientFromUserId(string userId)
        {
                return _dbContext.Clients.Single(c => c.UserId == userId);
        
        }

        public virtual async Task<Client> GetClientFromUserName(string userName)
        {
                return _dbContext.Clients.Single(c => c.User!.UserName == userName);

        }

        public virtual async void UpdateClient(Client client)
        {
            var existingClient = _dbContext.Clients.FindAsync(client.Id);
            if (existingClient == null)
            {
                throw new Exception("Client non trouvé dans la base de données");
            }

            _dbContext.Entry(existingClient).CurrentValues.SetValues(client);
            await _dbContext.SaveChangesAsync();
        }

        public virtual async Task<ProfileDTO> GetProfileInfo(string userId)
        {

            Client client = await GetClientFromUserId(userId);
            if (client == null)
            {
                throw new Exception("Client introuvable");
            }

            ProfileDTO profile = new ProfileDTO
            {
                UserName = client.Username,
                FirstName = client.FirstName,
                LastName = client.LastName,
                ImgUrl = client.ImageURL,
                IsDeliveryMan = client.IsDeliveryMan,
                IsActiveAsDeliveryMan = false,
            };

            if (client.IsDeliveryMan == true)
            {
                var deliveryMan = await getClientDeliverMan(client.Id);
                if (deliveryMan == null)
                {
                    throw new Exception("Livreur introuvable");
                }

                profile.IsActiveAsDeliveryMan = deliveryMan.IsActive;
            }

            return profile;
           
        }
    }
}
