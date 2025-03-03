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
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.UserId == userId);

            List<SuggestedProduct> products = await _context.SuggestedProducts
                .Include(sp => sp.Votes) 
                .Where(sp => sp.FinishDate > DateTime.UtcNow)
                .ToListAsync();

            List<SuggestedProductDTO> productDtos = new();

            foreach (var product in products)
            {
                string userVote = "";

                if (client != null)
                {
                    Vote? vote = product.Votes.FirstOrDefault(v => v.ClientId == client.Id);

                    if (vote != null)
                    {
                        userVote = vote.IsFor ? "For" : "Against";
                    }
                }

                productDtos.Add(new SuggestedProductDTO
                {
                    Id = product.Id,
                    Name = product.Name,
                    Photo = product.Photo
                            ?? "https://fondationdesgouverneurs.org/wp-content/uploads/2023/12/placeholder-287.png",
                    FinishDate = product.FinishDate,
                    UserVote = userVote
                });
            }

            return productDtos;
        }

        public virtual async Task<bool> VoteFor(int clientId, int suggestedProductId)
        {
            var product = await _context.SuggestedProducts
                .Include(p => p.Votes)
                .FirstOrDefaultAsync(p => p.Id == suggestedProductId);

            if (product == null)
            {
                throw new Exception("Produit suggéré introuvable.");
            }

            var existingVote = product.Votes.FirstOrDefault(v => v.ClientId == clientId);

            if (existingVote != null && existingVote.IsFor == true)
            {
                product.Votes.Remove(existingVote);
                _context.Votes.Remove(existingVote);
            }
            else
            {
                if (existingVote != null)
                {
                    product.Votes.Remove(existingVote);
                    _context.Votes.Remove(existingVote);
                }

                var vote = new Vote
                {
                    ClientId = clientId,
                    SuggestedProductId = suggestedProductId,
                    IsFor = true
                };
                product.Votes.Add(vote);
                _context.Votes.Add(vote);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public virtual async Task<bool> VoteAgainst(int clientId, int suggestedProductId)
        {
            var product = await _context.SuggestedProducts
                .Include(p => p.Votes)
                .FirstOrDefaultAsync(p => p.Id == suggestedProductId);

            if (product == null)
            {
                throw new Exception("Produit suggéré introuvable.");
            }

            var existingVote = product.Votes.FirstOrDefault(v => v.ClientId == clientId);

            if (existingVote != null && existingVote.IsFor == false)
            {
                product.Votes.Remove(existingVote);
                _context.Votes.Remove(existingVote);
            }
            else
            {
                if (existingVote != null)
                {
                    product.Votes.Remove(existingVote);
                    _context.Votes.Remove(existingVote);
                }

                var vote = new Vote
                {
                    ClientId = clientId,
                    SuggestedProductId = suggestedProductId,
                    IsFor = false
                };
                product.Votes.Add(vote);
                _context.Votes.Add(vote);
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
