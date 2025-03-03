using System.Diagnostics;
using API_LapinCouvert.DTOs;
using LapinCouvert.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Models;
using MVC_LapinCouvert.Data;
using System.Security.Claims;
using Admin_API.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using static LapinCouvert.Models.SuggestedProduct;
using Supabase.Gotrue;
using Client = LapinCouvert.Models.Client;

namespace API_LapinCouvert.Services
{
    public class SuggestedProductsService
    {
        private readonly ApplicationDbContext _context;

        public SuggestedProductsService(ApplicationDbContext context)
        {
            _context = context;
        }
        public SuggestedProductsService()
        {
           
        }

        public virtual async Task<List<SuggestedProductDTO>> GetSuggestedProducts(string userId)
        {
            // Ensure that the UserVotes collection is loaded.
            List<SuggestedProduct> products = await _context.SuggestedProducts
                  //.Where(x => x.FinishDate > DateTime.UtcNow)
                  .ToListAsync();

            // DTO list creation
            List<SuggestedProductDTO> productDtos = products.Select(sp => new SuggestedProductDTO()
            {
                Id = sp.Id,
                Name = sp.Name,
                Photo = sp.Photo ?? "https://fondationdesgouverneurs.org/wp-content/uploads/2023/12/placeholder-287.png",
                FinishDate = sp.FinishDate,
            }).ToList();

            return productDtos;
        }

        public virtual async Task<bool> VoteFor(Client client, int suggestedProductId)
        {
            SuggestedProduct suggestedProduct = await _context.SuggestedProducts.FindAsync(suggestedProductId);
            if (suggestedProduct == null)
            {
                throw new Exception("Produit suggéré introuvable.");
            }

            suggestedProduct.ForClients ??= new List<Client>();
            suggestedProduct.AgainstClients ??= new List<Client>();

            if (suggestedProduct.ForClients.Contains(client))
            {
                suggestedProduct.ForClients.Remove(client);
            }
            else
            {
                suggestedProduct.ForClients.Add(client);
                suggestedProduct.AgainstClients.Remove(client);
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public virtual async Task<bool> VoteAgainst(Client client, int suggestedProductId)
        {
            SuggestedProduct suggestedProduct = await _context.SuggestedProducts.FindAsync(suggestedProductId);
            if (suggestedProduct == null)
            {
                throw new Exception("Produit suggéré introuvable.");
            }

            suggestedProduct.ForClients ??= new List<Client>();
            suggestedProduct.AgainstClients ??= new List<Client>();

            if (suggestedProduct.AgainstClients.Contains(client))
            {
                suggestedProduct.AgainstClients.Remove(client);
            }
            else
            {
                suggestedProduct.AgainstClients.Add(client);
                suggestedProduct.ForClients.Remove(client);
            }

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
