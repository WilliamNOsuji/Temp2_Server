using Admin_API.Services;
using API_LapinCouvert.DTOs;
using API_LapinCouvert.Services;
using LapinCouvert.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Models;
using MVC_LapinCouvert.Data;
using SQLitePCL;
using Stripe;
using Stripe.Checkout;
using Stripe.FinancialConnections;
using Supabase.Gotrue;
using Supabase.Postgrest;
using System.Security.Claims;
using SessionService = Stripe.Checkout.SessionService;

namespace API_LapinCouvert.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StripeController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ApplicationDbContext _context;
        private readonly CommandsService _commandsService;
        private readonly Admin_API.Services.ClientsService _clientsService;

        public StripeController(UserService userService, ApplicationDbContext applicationDbContext, CommandsService commandsService,Admin_API.Services.ClientsService clientsService)
        {
            _userService = userService;
            _context = applicationDbContext;
            _commandsService = commandsService;
            _clientsService = clientsService;
        }
        private readonly string _stripeSecretKey = "sk_test_51QprthRvxWgpsTY5If1jHdoBnoPV5ep8OSo5s6fhGi4TN7m9U50VfkIaWXyTkGZM38s3jXOBueOxMr5mTPzaHlHT00DWueiFZj"; // Replace with your actual Stripe secret key


        [HttpPost]
        public async Task<ActionResult<PaymentIntentDTO>> CreatePaymentIntent(CommandDTO commandDTO)
        {
            // Convert the amount to cents (smallest currency unit)
            long amountInCents = (long)(commandDTO.TotalPrice * 100);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);
            // Create a customer (optional)
            Customer customer = await _userService.CreateCustomer(client, commandDTO);

            // Set the Stripe API key
            StripeConfiguration.ApiKey = _stripeSecretKey;

            // Configure the payment intent options
            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInCents, // Use the converted amount
                Currency = commandDTO.Currency,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions() { Enabled = true },
                Customer = customer.Id,
            };

            // Create the payment intent
            var service = new PaymentIntentService();
            PaymentIntent pi = await service.CreateAsync(options);

            // Return the payment intent details to the client
            PaymentIntentDTO dto = new PaymentIntentDTO
            {
                ClientSecret = pi.ClientSecret,
                Customer = pi.CustomerId
            };

            return Ok(dto);
        }
         
        [HttpPost()]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CheckoutSessionRequest request)
        {
            StripeConfiguration.ApiKey = _stripeSecretKey;

            try
            {
                // Create product line items from cart
                var lineItems = new List<SessionLineItemOptions>();

                // In a real implementation, you'd fetch these from the cart
                // For demo purposes we're creating a single line item with the total amount
                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(request.TotalPrice * 100), // Convert to cents
                        Currency = request.Currency,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Commande de Lapin Couvert",
                            Description = "Livraison au " + request.Address,
                        }
                    },
                    Quantity = 1,
                });

                // Create checkout session
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = lineItems,
                    Mode = "payment",
                    SuccessUrl = request.SuccessUrl + "?session_id={CHECKOUT_SESSION_ID}",
                    CancelUrl = request.CancelUrl + "?canceled=true",
                    //CustomerEmail = "customer@example.com", // In a real app, use the user's email
                    Metadata = new Dictionary<string, string>
                    {
                        { "ClientId", User.FindFirst(ClaimTypes.NameIdentifier).Value },
                        { "Address", request.Address },
                        { "PhoneNumber", request.PhoneNumber },
                    }
                    
                };

                var service = new Stripe.Checkout.SessionService();
                var session = await service.CreateAsync(options);

                // Create a record in your database for this session
                // SaveCheckoutSession(session.Id, request);

                return Ok(new { id = session.Id, CheckoutUrl = session.Url, clientSecret = session.PaymentIntentId  });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{sessionId}")]
        [Authorize]
        public async Task<IActionResult> VerifyCheckoutSession(string sessionId)
        {
            StripeConfiguration.ApiKey = _stripeSecretKey;
            try
            {
                // Get the current user ID
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var client = await _clientsService.GetClientFromUserId(userId);

                if (client == null)
                {
                    return NotFound("Client not found");
                }

                // Verify the Stripe session
                var sessionService = new SessionService();
                var session = await sessionService.GetAsync(sessionId);

                if (session.PaymentStatus == "paid")
                {
                    // Create a CommandDTO from the session data
                    CommandDTO commandDTO = new CommandDTO
                    {
                        Address = session.Metadata.ContainsKey("Address") ?
                            session.Metadata["Address"] : "Aucune addresse",

                                            PhoneNumber = session.Metadata.ContainsKey("PhoneNumber") ?
                            session.Metadata["PhoneNumber"] : "Aucun numero de telephone",

                        TotalPrice = (double)session.AmountTotal / 100, // Convert from cents

                        Currency = session.Currency,

                        // If you have device tokens in your CommandDTO, provide an empty list
                        DeviceTokens = new List<string>()
                    };

                    // Call the Create method on CommandsController
                    Command command = await _commandsService.CreateCommand(client.Id, commandDTO);

                    if (command == null)
                    {
                        return BadRequest("Error creating command");
                    }

                    return Ok(command);
                }
                else
                {
                    return BadRequest(new { error = "Payment not completed", status = session.PaymentStatus });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private int GetClientIdFromSession(Stripe.Checkout.Session session)
        {
            // In a real implementation, you would extract the client ID
            // from the session or payment intent metadata
            return 1; // Default client ID
        }
    }
}
