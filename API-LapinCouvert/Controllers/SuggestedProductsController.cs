using API_LapinCouvert.DTOs;
using API_LapinCouvert.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Admin_API.Services;
using Microsoft.AspNetCore.Authorization;
using LapinCouvert.Models;
using MVC_LapinCouvert.Data;
using static LapinCouvert.Models.SuggestedProduct;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Models.Models;
using Microsoft.EntityFrameworkCore;

namespace API_LapinCouvert.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SuggestedProductsController : ControllerBase
    {
        private readonly SuggestedProductsService _suggestedProductsService;
        private readonly ClientsService _clientsService;
        private readonly UserIdGetService _userIdGetService;

        public SuggestedProductsController(SuggestedProductsService suggestedProductsService, ClientsService clientsService, UserIdGetService userIdGetService)
        {
            _suggestedProductsService = suggestedProductsService;
            _clientsService = clientsService;
            _userIdGetService = userIdGetService;
        }

        [Authorize]
        [HttpPost("{suggestedProductId}")]
        public virtual async Task<ActionResult<SuggestedProduct>> VoteFor(int suggestedProductId)
        {
            var userId = _userIdGetService.getUserId();
            if (userId == null)
            {
                return NotFound("Identifiant utilisateur non trouvé.");
            }

            Client client = _clientsService.GetClientFromUserId(userId);
            if (client == null)
            {
                return NotFound("Client non trouvé pour cet utilisateur.");
            }

            try
            {
                await _suggestedProductsService.VoteFor(client.Id, suggestedProductId);
            }
            catch (Exception e)
            {
                return BadRequest("Erreur lors du vote pour le produit suggéré");
            }

            return Ok();
        }

        [Authorize]
        [HttpPost("{suggestedProductId}")]
        public virtual async Task<ActionResult<SuggestedProduct>> VoteAgainst(int suggestedProductId)
        {
            var userId = _userIdGetService.getUserId();
            if (userId == null)
            {
                return NotFound("Identifiant utilisateur non trouvé.");
            }

            Client client = _clientsService.GetClientFromUserId(userId);
            if (client == null)
            {
                return NotFound("Client non trouvé pour cet utilisateur.");
            }

            try
            {
                await _suggestedProductsService.VoteAgainst(client.Id, suggestedProductId);
            }
            catch (Exception e)
            {
                return BadRequest("Erreur lors du vote contre le produit suggéré");
            }

            return Ok();
        }

        [Authorize]
        [HttpGet]
        public virtual async Task<ActionResult> GetSuggestedProducts()
        {
            var userId = _userIdGetService.getUserId();
            if (userId == null)
            {
                return NotFound("Identifiant utilisateur non trouvé.");
            }

            try
            {
                List<SuggestedProductDTO> productDtos = await _suggestedProductsService.GetSuggestedProducts(userId);
                return Ok(productDtos);
            }
            catch (Exception e)
            {
                return NotFound("Erreur lors de la récupération des produits suggérés");
            }
        }
    }
}
