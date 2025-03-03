using Humanizer;
using LapinCouvert.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Models;
using Client = LapinCouvert.Models.Client;


namespace MVC_LapinCouvert.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public const string ADMIN_ROLE = "admin";
        public const string DELIVERYMAN_ROLE = "deliveryMan";

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityUser>().HasData(Seed.SeedUsers());
            builder.Entity<IdentityRole>().HasData(Seed.SeedRoles());
            builder.Entity<IdentityUserRole<string>>().HasData(Seed.SeedUserRoles());
            builder.Entity<Client>().HasData(Seed.SeedClients());
            builder.Entity<DeliveryMan>().HasData(Seed.SeedDeliveryMen());
            builder.Entity<Cart>().HasData(Seed.SeedCarts());
            builder.Entity<Category>().HasData(Seed.SeedCategories());
            builder.Entity<Command>().HasData(Seed.SeedCommands());
            builder.Entity<Product>().HasData(Seed.SeedProducts());
            builder.Entity<CommandProduct>().HasData(Seed.SeedCommandProducts());
            builder.Entity<SuggestedProduct>().HasData(Seed.SeedSuggestedProducts());

            builder.Entity<CartProducts>()
                .HasKey(cp => new { cp.CartId, cp.ProductId });

            builder.Entity<CommandProduct>()
                .HasKey(cp => new { cp.CommandId, cp.ProductId });

            builder.Entity<CommandProduct>()
            .HasOne(cp => cp.Command)
            .WithMany()
            .HasForeignKey(cp => cp.CommandId);

            builder.Entity<CommandProduct>()
            .HasOne(cp => cp.Product)
            .WithMany()
            .HasForeignKey(cp => cp.ProductId);

            builder.Entity<CartProducts>()
            .HasOne(cp => cp.Product)
            .WithMany(p => p.CartProducts)
            .HasForeignKey(cp => cp.ProductId);

            builder.Entity<CartProducts>()
            .HasOne(cp => cp.Cart)
            .WithMany(p => p.CartProducts)
            .HasForeignKey(cp => cp.CartId);

            builder.Entity<Client>()
                .HasOne(c => c.Cart)  
                .WithOne(c => c.Client) 
                .HasForeignKey<Client>(c => c.CartId);

            // Configure the one-to - zero - or - one relationship
            builder.Entity<DeliveryMan>()
                .HasOne(d => d.Client) // DeliveryMan has one Client
                .WithOne(c => c.DeliveryMan) // Client has zero or one DeliveryMan
                .HasForeignKey<DeliveryMan>(d => d.ClientId) // Foreign key in DeliveryMan
                .OnDelete(DeleteBehavior.Restrict); // Optional: Configure delete behavior
        }

        public DbSet<Cart> Carts { get; set; } = default!;
        public DbSet<Client> Clients { get; set; } = default!;
        public DbSet<Category> Categories { get; set; } = default!;
        public DbSet<Command> Commands { get; set; } = default!;
        public DbSet<DeliveryMan> DeliveryMans { get; set; } = default!;
        public DbSet<Product> Products { get; set; } = default!;
        public DbSet<SuggestedProduct> SuggestedProducts { get; set; } = default!;
        public DbSet<CartProducts> CartProducts { get; set; } = default!;
        public DbSet<CommandProduct> CommandProducts { get; set; } = default!;
        public DbSet<Chat> Chats { get; set; } = default!;

    }
}
