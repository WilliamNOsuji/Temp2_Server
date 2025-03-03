using API_LapinCouvert.DTOs;
using Castle.Core.Resource;
using LapinCouvert.Models;
using MVC_LapinCouvert.Data;
using Stripe;
using System.Linq;

namespace API_LapinCouvert.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;


        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Customer Customer { get; set; }
        public PaymentIntent PaymentIntent { get; set; }

        public async Task<Customer> CreateCustomer(Client client, CommandDTO commandDTO)
        {

            StripeConfiguration.ApiKey = "sk_test_51QprthRvxWgpsTY5If1jHdoBnoPV5ep8OSo5s6fhGi4TN7m9U50VfkIaWXyTkGZM38s3jXOBueOxMr5mTPzaHlHT00DWueiFZj";
            AddressOptions address = new AddressOptions()
            {
                City = "",
                Country = "",
                PostalCode = "",
                State = "",
                Line1 = commandDTO.Address
            };

            ShippingOptions shipping = new ShippingOptions()
            {
                Address = address,
                Name = client.FirstName,
                Phone = commandDTO.PhoneNumber,
            };


            var options = new CustomerCreateOptions
            {
                Email = client.User.Email,
                Name = client.FirstName + " " + client.LastName,
                Address = address,
                Shipping = shipping,
                Phone = commandDTO.PhoneNumber,
                Description = "This is a Stripe Customer Description"
            };

            var service = new CustomerService();
            Customer customer = await service.CreateAsync(options);

            Customer = customer;

            return customer;
        }
    }
}
