using LapinCouvert.Models;
using Microsoft.AspNetCore.Identity;
using Models.Models;
using MVC_LapinCouvert.Data;
using System.Collections.Generic;
using System.Numerics;

namespace Models
{
    public class Seed
    {
        public Seed() { }

        public static void SeedDatabase(ApplicationDbContext context)
        {
            if (!context.Clients.Any())
            {
                context.Clients.AddRange(SeedClients());
                context.SaveChanges(); // Save Clients first
            }

            if (!context.DeliveryMans.Any())
            {
                context.DeliveryMans.AddRange(SeedDeliveryMen());
                context.SaveChanges(); // Save DeliveryMen after Clients
            }

            // Seed other tables
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(SeedCategories());
            }
            if (!context.Carts.Any())
            {
                context.Carts.AddRange(SeedCarts());
            }
            if (!context.Products.Any())
            {
                context.Products.AddRange(SeedProducts());
            }
            if (!context.Commands.Any())
            {
                context.Commands.AddRange(SeedCommands());
            }
            if (!context.CommandProducts.Any())
            {
                context.CommandProducts.AddRange(SeedCommandProducts());
            }

            if (!context.SuggestedProducts.Any())
            {
                context.SuggestedProducts.AddRange(SeedSuggestedProducts());
            }

            context.SaveChanges();
        }

        public static DeliveryMan[] SeedDeliveryMen()
        {
            return new DeliveryMan[]
            {
                new DeliveryMan
                {
                    Id = 1,
                    Money = 150.00,
                    IsActive = true,
                    ClientId = 1 // Valid ClientId from the Client table
                },
                new DeliveryMan
                {
                    Id = 2,
                    Money = 200.50,
                    IsActive = true,
                    ClientId = 2 // Valid ClientId from the Client table
                }
            };
        }

        public static SuggestedProduct[] SeedSuggestedProducts()
        {
            return new SuggestedProduct[]
            {
                new SuggestedProduct
                {
                    Id = 1,
                    Name = "Gaufres",
                    FinishDate = DateTime.UtcNow.AddDays(7),
                    Photo = "https://lecoureurnordique.ca/cdn/shop/files/4-Waffle-Berries-box-opened.jpg?v=1722540909&width=1214",
                },
                new SuggestedProduct
                {
                    Id = 2,
                    Name = "Cornets",
                    FinishDate = DateTime.UtcNow.AddDays(7),
                    Photo = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQoBx3hiPWYqDgwmUXn4FfiBrOB6PmOXewpqQ&s",
                },
                new SuggestedProduct
                {
                    Id = 3,
                    Name = "Fudges glacés",
                    FinishDate = DateTime.UtcNow.AddDays(7),
					Photo = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQRlu3mzqkeID_rRd4xy1RnHOBdQf6iiiy3mg&s",
                },
                new SuggestedProduct
                {
                    Id = 4,
                    Name = "Hummus",
                    FinishDate = DateTime.UtcNow.AddDays(7),
                    Photo = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSMtAo5EOV4PwwsX68gcZcbpFmUvlJjwmdrlQ&s",
                },
                new SuggestedProduct
                {
                    Id = 5,
                    Name = "Pâté au poulet",
                    FinishDate = DateTime.UtcNow.AddDays(7),
                    Photo = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQRprmtq6sxdBWIk7WJLe4phNvN687HYJtNRg&s",
                },
                new SuggestedProduct
                {
                    Id = 6,
                    Name = "Sushi",
                    FinishDate = DateTime.UtcNow.AddDays(7),
                    Photo = "https://fastfoodpak.com/cdn/shop/products/H9d0cd66c764247c99bfa03dee91d5474w-removebg-preview.png?v=1642061891",
                },
            };
        }

        public static Command[] SeedCommands()
        {
            return new Command[]
            {
                new Command
                {
                    Id = 1,
                    CommandNumber = 1001,
                    ClientPhoneNumber = "555-1234",
                    ArrivalPoint = "123 Main St",
                    TotalPrice = 45.99,
                    Currency = "USD",
                    IsDelivered = false,
                    ClientId = 1,
                    DeliveryManId = 1  // Assigned to John Doe
                },
                new Command
                {
                    Id = 2,
                    CommandNumber = 1002,
                    ClientPhoneNumber = "555-5678",
                    ArrivalPoint = "456 Elm St",
                    TotalPrice = 89.75,
                    Currency = "USD",
                    IsDelivered = false,
                    ClientId = 2,
                    DeliveryManId = 2  // Assigned to Jane Smith
                },
                new Command
                {
                    Id = 3,
                    CommandNumber = 1003,
                    ClientPhoneNumber = "555-9876",
                    ArrivalPoint = "789 Oak St",
                    TotalPrice = 120.00,
                    Currency = "USD",
                    IsDelivered = false,
                    ClientId = 3,
                    DeliveryManId = null  // Not assigned yet
                },
                new Command
                {
                    Id = 4,
                    CommandNumber = 1004,
                    ClientPhoneNumber = "555-1470",
                    ArrivalPoint = "965 Bruh St",
                    TotalPrice = 15.00,
                    Currency = "CAD",
                    IsDelivered = false,
                    ClientId = 3,
                    DeliveryManId = null  // Not assigned yet
                },
                new Command
                {
                    Id = 5,
                    CommandNumber = 1005,
                    ClientPhoneNumber = "555-4255",
                    ArrivalPoint = "322 Fuck St",
                    TotalPrice = 2000.00,
                    Currency = "CAD",
                    IsDelivered = false,
                    ClientId = 3,
                    DeliveryManId = null  // Not assigned yet
                },
                new Command
                {
                    Id = 6,
                    CommandNumber = 1006,
                    ClientPhoneNumber = "555-2145",
                    ArrivalPoint = "696 Nig St",
                    TotalPrice = 330.50,
                    Currency = "CAD",
                    IsDelivered = false,
                    ClientId = 3,
                    DeliveryManId = null  // Not assigned yet
                }
            };
        }

        public static CommandProduct[] SeedCommandProducts()
        {
            return new CommandProduct[]
            {
                new CommandProduct
                {
                    Id = 1,
                    CommandId = 1, // Lié à la Commande 1001
                    ProductId = 1,
                    Name = "Burger",
                    Price = 12.99,
                    Quantity = 2
                },
                new CommandProduct
                {
                    Id = 2,
                    CommandId = 1, // Lié à la Commande 1001
                    ProductId = 2,
                    Name = "Frites",
                    Price = 4.99,
                    Quantity = 1
                },
                new CommandProduct
                {
                    Id = 3,
                    CommandId = 2, // Lié à la Commande 1002
                    ProductId = 3,
                    Name = "Pizza",
                    Price = 18.50,
                    Quantity = 1
                },
                new CommandProduct
                {
                    Id = 4,
                    CommandId = 2, // Lié à la Commande 1002
                    ProductId = 4,
                    Name = "Soda",
                    Price = 2.50,
                    Quantity = 2
                },
                new CommandProduct
                {
                    Id = 5,
                    CommandId = 3, // Lié à la Commande 1003 (livraison non attribuée)
                    ProductId = 5,
                    Name = "Salade",
                    Price = 9.99,
                    Quantity = 1
                }
            };
        }



        public static Product[] SeedProducts()
        {
            return new Product[]
            {
                new Product
                {
                    Id = 1,
                    Name = "Pogo",
                    Brand = "🤣😂😍",
                    Quantity = 10,
                    Description = "Produit de saucisse congelé",
                    RetailPrice = 10.00,
                    SellingPrice = 70.00,
                    CategoryId = Category.FROZEN_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/poogo-min.jpg",
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 2,
                    Name = "Yaourt",
                    Brand = "Yoplait Inc",
                    Quantity = 5,
                    Description = "Yaourt à la fraise",
                    RetailPrice = 20.00,
                    SellingPrice = 60.00,
                    CategoryId = Category.DAIRY_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Yaourt-min.jpg",
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 3,
                    Name = "Craquelins de blé",
                    Brand = "Christie",
                    Quantity = 15,
                    Description = "Craquelins au goût original",
                    RetailPrice = 40.00,
                    SellingPrice = 50.00,
                    CategoryId = Category.CRACKERS_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Craquelins-min.jpg",
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 4,
                    Name = "Biscuits digestifs",
                    Brand = "McVitie's",
                    Quantity = 30,
                    Description = "Biscuits digestifs à base de blé entier",
                    RetailPrice = 15.00,
                    SellingPrice = 25.00,
                    CategoryId = Category.BUISCUIT_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Biscuits%20digestif-min.jpg",
                    IsDeleted = false,
                },

                // Frozen
                new Product
                {
                    Id = 5,
                    Name = "Pizza congelée",
                    Brand = "DiGiorno",
                    Quantity = 10,
                    Description = "Pizza congelée au pepperoni",
                    RetailPrice = 50.00,
                    SellingPrice = 70.00,
                    CategoryId = Category.FROZEN_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Pizza%20congelee-min.jpg",
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 6,
                    Name = "Crème glacée",
                    Brand = "Ben & Jerry's",
                    Quantity = 20,
                    Description = "Crème glacée au brownie au chocolat",
                    RetailPrice = 40.00,
                    SellingPrice = 60.00,
                    CategoryId = Category.FROZEN_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Creme%20glacee-min.jpg",
                    IsDeleted = false,
                },

                // Dairy
                new Product
                {
                    Id = 7,
                    Name = "Lait",
                    Brand = "Lactaid",
                    Quantity = 50,
                    Description = "Lait entier sans lactose",
                    RetailPrice = 10.00,
                    SellingPrice = 15.00,
                    CategoryId = Category.DAIRY_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Lait-min.jpg",
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 8,
                    Name = "Yaourt",
                    Brand = "Chobani",
                    Quantity = 40,
                    Description = "Yaourt grec au miel",
                    RetailPrice = 20.00,
                    SellingPrice = 30.00,
                    CategoryId = Category.DAIRY_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Yaourt-min.jpg",
                    IsDeleted = false,
                },

                // Legumes
                new Product
                {
                    Id = 9,
                    Name = "Haricots noirs",
                    Brand = "Bush's",
                    Quantity = 35,
                    Description = "Haricots noirs en conserve",
                    RetailPrice = 15.00,
                    SellingPrice = 25.00,
                    CategoryId = Category.LEGUMINEUSE_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Haricots%20noirs-min.jpg",
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 10,
                    Name = "Lentilles",
                    Brand = "Goya",
                    Quantity = 30,
                    Description = "Lentilles vertes séchées",
                    RetailPrice = 10.00,
                    SellingPrice = 20.00,
                    CategoryId = Category.LEGUMINEUSE_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Lentilles-min.jpg",
                    IsDeleted = false,
                },

                // Beverages
                new Product
                {
                    Id = 11,
                    Name = "Jus d'orange",
                    Brand = "Tropicana",
                    Quantity = 25,
                    Description = "Jus d'orange 100% pur",
                    RetailPrice = 20.00,
                    SellingPrice = 30.00,
                    CategoryId = Category.BEVERAGES_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Jus%20dorange-min.jpg",
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 12,
                    Name = "Café",
                    Brand = "Starbucks",
                    Quantity = 30,
                    Description = "Café moulu à torréfaction sombre",
                    RetailPrice = 25.00,
                    SellingPrice = 35.00,
                    CategoryId = Category.BEVERAGES_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Cafe-min.jpg",
                    IsDeleted = false,
                },

                // Snacks
                new Product
                {
                    Id = 13,
                    Name = "Chips classique",
                    Brand = "Lay's",
                    Quantity = 40,
                    Description = "Chips aux pommes de terre classiques salés",
                    RetailPrice = 15.00,
                    SellingPrice = 25.00,
                    CategoryId = Category.SNACKS_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Chips%20classique-min.jpg",
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 14,
                    Name = "Maïs soufflé",
                    Brand = "Orville Redenbacher's",
                    Quantity = 35,
                    Description = "Maïs soufflé au beurre pour micro-ondes",
                    RetailPrice = 10.00,
                    SellingPrice = 20.00,
                    CategoryId = Category.SNACKS_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Mais%20souffle-min.jpg",
                    IsDeleted = false,
                },

                // Canned Goods
                new Product
                {
                    Id = 15,
                    Name = "Soupe à la tomate",
                    Brand = "Campbell's",
                    Quantity = 20,
                    Description = "Soupe crémeuse à la tomate",
                    RetailPrice = 10.00,
                    SellingPrice = 15.00,
                    CategoryId = Category.CANNED_GOODS_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Soupe%20a%20la%20tomate-min.jpg",
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 16,
                    Name = "Thon",
                    Brand = "StarKist",
                    Quantity = 25,
                    Description = "Thon en conserve dans l'eau",
                    RetailPrice = 15.00,
                    SellingPrice = 25.00,
                    CategoryId = Category.CANNED_GOODS_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Thon-min.jpg",
                    IsDeleted = false,
                },

                // Bakery
                new Product
                {
                    Id = 17,
                    Name = "Pain de blé entier",
                    Brand = "Wonder",
                    Quantity = 30,
                    Description = "Pain de blé entier moelleux",
                    RetailPrice = 20.00,
                    SellingPrice = 30.00,
                    CategoryId = Category.BAKERY_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Pain%20de%20ble%20entier-min.jpg",
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 18,
                    Name = "Bagels",
                    Brand = "Thomas'",
                    Quantity = 25,
                    Description = "Bagels nature",
                    RetailPrice = 15.00,
                    SellingPrice = 25.00,
                    CategoryId = Category.BAKERY_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Bagels-min.jpg",
                    IsDeleted = false,
                },

                // Meat
                new Product
                {
                    Id = 19,
                    Name = "Poitrine de poulet",
                    Brand = "Tyson",
                    Quantity = 20,
                    Description = "Poitrine de poulet sans os et sans peau",
                    RetailPrice = 50.00,
                    SellingPrice = 70.00,
                    CategoryId = Category.MEAT_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Poitrine-de-poulet-min.jpg",
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 20,
                    Name = "Viande hachée",
                    Brand = "Angus",
                    Quantity = 25,
                    Description = "Viande hachée à 80% de maigreur",
                    RetailPrice = 40.00,
                    SellingPrice = 60.00,
                    CategoryId = Category.MEAT_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Viande%20hachee-min.jpg",
                    IsDeleted = false,
                },

                // Seafood
                new Product
                {
                    Id = 21,
                    Name = "Filet de saumon",
                    Brand = "SeaBest",
                    Quantity = 15,
                    Description = "Filet de saumon de l'Atlantique frais",
                    RetailPrice = 60.00,
                    SellingPrice = 80.00,
                    CategoryId = Category.SEAFOOD_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Filet%20de%20saumon-min.jpg",
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 22,
                    Name = "Crevettes",
                    Brand = "Aqua Star",
                    Quantity = 20,
                    Description = "Crevettes décortiquées et déveinées congelées",
                    RetailPrice = 50.00,
                    SellingPrice = 70.00,
                    CategoryId = Category.SEAFOOD_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Crevettes-min.jpg",
                    IsDeleted = false,
                },

                // Condiments
                new Product
                {
                    Id = 23,
                    Name = "Ketchup",
                    Brand = "Heinz",
                    Quantity = 40,
                    Description = "Ketchup classique à la tomate",
                    RetailPrice = 10.00,
                    SellingPrice = 15.00,
                    CategoryId = Category.CONDIMENTS_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Ketchup-min.jpg",
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 24,
                    Name = "Mayonnaise",
                    Brand = "Hellmann's",
                    Quantity = 35,
                    Description = "Mayonnaise réelle",
                    RetailPrice = 15.00,
                    SellingPrice = 25.00,
                    CategoryId = Category.CONDIMENTS_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Mayonnaise-min.jpg",
                    IsDeleted = false,
                },

                // Pasta
                new Product
                {
                    Id = 25,
                    Name = "Spaghetti",
                    Brand = "Barilla",
                    Quantity = 30,
                    Description = "Pâtes spaghetti classiques",
                    RetailPrice = 10.00,
                    SellingPrice = 20.00,
                    CategoryId = Category.PASTA_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Spaghetti-min.jpg",
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 26,
                    Name = "Penne",
                    Brand = "De Cecco",
                    Quantity = 25,
                    Description = "Pâtes penne complètes",
                    RetailPrice = 15.00,
                    SellingPrice = 25.00,
                    CategoryId = Category.PASTA_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Penne-min.jpg",
                    IsDeleted = false,
                },

                // Cereal
                new Product
                {
                    Id = 27,
                    Name = "Flocons de maïs",
                    Brand = "Kellogg's",
                    Quantity = 30,
                    Description = "Les flocons de maïs sont faits à partir des grains de maïs jaune, dont on a enlevé le germe et le son, c'est-à-dire l'écorce.",
                    RetailPrice = 20.00,
                    SellingPrice = 30.00,
                    CategoryId = Category.CEREAL_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Flocons%20de%20mais-min.jpg",
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 28,
                    Name = "Granola",
                    Brand = "Nature Valley",
                    Quantity = 25,
                    Description = "Le granola est un aliment composé de noix et de céréales mélangées avec du miel et d’autres ingrédients naturels.",
                    RetailPrice = 15.00,
                    SellingPrice = 25.00,
                    CategoryId = Category.CEREAL_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Granola-min.jpg",
                    IsDeleted = false,
                },

                // Fruits ------------------
                new Product
                {
                    Id = 29,
                    Name = "Pommes",
                    Brand = "Washington",
                    Quantity = 50,
                    Description = "T'es sérieux? Quand même une pomme.",
                    RetailPrice = 10.00,
                    SellingPrice = 20.00,
                    CategoryId = Category.FRUITS_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Pommes_2-min.jpg",
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 30,
                    Name = "Bananes",
                    Brand = "Chiquita",
                    Quantity = 60,
                    Description = "Les bananes sont des fruits très généralement stériles issus de variétés domestiquées.",
                    RetailPrice = 15.00,
                    SellingPrice = 25.00,
                    CategoryId = Category.FRUITS_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Bananes_shop-min.jpg",
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 31,
                    Name = "Biscuits Oreo",
                    Brand = "Oreo",
                    Quantity = 25,
                    Description = "Biscuits aux pépites de chocolat classiques",
                    RetailPrice = 20.00,
                    SellingPrice = 30.00,
                    CategoryId = Category.BUISCUIT_ID,
                    Photo = "https://jaewoltprrassmozmehq.supabase.co/storage/v1/object/public/images/uploads/Biscuits%20Oreo-min.jpg",
                    IsDeleted = false,
                },

            };
        }



        public static Category[] SeedCategories()
        {
            return new Category[]
            {
                new Category
                {
                    Id = 17,
                    Name = "Non Spécifiés",
                },
                new Category
                {
                    Id = 1,
                    Name = "Congelés",
                },
                new Category
                {
                    Id = 2,
                    Name = "Produits Laitiers",
                },
                new Category
                {
                    Id = 3,
                    Name = "Biscuits Salés",
                },
                new Category
                {
                    Id = 4,
                    Name = "Biscuits",
                },
                new Category
                {
                    Id = 5,
                    Name = "Légumineuses",
                },
                new Category
                {
                    Id = 6,
                    Name = "Boissons",
                },
                new Category
                {
                    Id = 7,
                    Name = "En-cas",
                },
                new Category
                {
                    Id = 8,
                    Name = "Produits en Conserve",
                },
                new Category
                {
                    Id = 9,
                    Name = "Boulangerie",
                },
                new Category
                {
                    Id = 10,
                    Name = "Viande",
                },
                new Category
                {
                    Id = 11,
                    Name = "Fruits de Mer",
                },
                new Category
                {
                    Id = 12,
                    Name = "Sauces",
                },
                new Category
                {
                    Id = 13,
                    Name = "Pâtes",
                },
                new Category
                {
                    Id = 14,
                    Name = "Céréales",
                },
                new Category
                {
                    Id = 15,
                    Name = "Fruits",
                },
                new Category
                {
                    Id = 16,
                    Name = "Légumes",
                }
            };
        }




        public static Cart[] SeedCarts()
        {
            return new Cart[]
            {
                new Cart
                {
                    Id = 1,
                    ClientId = 1,
                },
                new Cart
                {
                    Id = 2,
                    ClientId = 2,
                },
                new Cart
                {
                    Id = 3,
                    ClientId = 3,
                },
            };
        }


        public static IdentityRole[] SeedRoles()
        {
            IdentityRole adminRole = new IdentityRole
            {
                Id = "11111111-1111-1111-1111-111111111112",
                Name = ApplicationDbContext.ADMIN_ROLE,
                NormalizedName = ApplicationDbContext.ADMIN_ROLE.ToUpper()
            };
            IdentityRole deliveryManRole = new IdentityRole
            {
                Id = "11111111-1111-1111-1111-111111111113",
                Name = ApplicationDbContext.DELIVERYMAN_ROLE,
                NormalizedName = ApplicationDbContext.DELIVERYMAN_ROLE.ToUpper()
            };

            return new IdentityRole[] { adminRole, deliveryManRole };
        }

        public static IdentityUserRole<string>[] SeedUserRoles()
        {
            IdentityUserRole<string> userAdmin = new IdentityUserRole<string>
            {
                RoleId = "11111111-1111-1111-1111-111111111112",
                UserId = "UserAdmin1Id",
            };
            IdentityUserRole<string> userAdmin2 = new IdentityUserRole<string>
            {
                RoleId = "11111111-1111-1111-1111-111111111112",
                UserId = "UserAdmin2Id",
            };

            return new IdentityUserRole<string>[] { userAdmin, userAdmin2 };
        }  
        
        public static IdentityUser[] SeedUsers()
        {
            var hasher = new PasswordHasher<IdentityUser>();
            return new IdentityUser[]
            {
                new IdentityUser()
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
                    Id = "UserAdmin1Id",
                    UserName = "Admin",
                    Email = "admin@admin.com",
                    // La comparaison d'identity se fait avec les versions normalisés
                    NormalizedEmail = "ADMIN@ADMIN.COM",
                    NormalizedUserName = "ADMIN",
                    EmailConfirmed = true,
                        // On encrypte le mot de passe
                    PasswordHash = hasher.HashPassword(null, "Passw0rd!"),
                    LockoutEnabled = true,
                },
                new IdentityUser
                {
                    Id = "UserAdmin2Id",
                    UserName = "Admin2",
                    Email = "admin2@admin.com",
                    // La comparaison d'identity se fait avec les versions normalisés
                    NormalizedEmail = "ADMIN2@ADMIN.COM",
                    NormalizedUserName = "ADMIN2",
                    EmailConfirmed = true,
                            // On encrypte le mot de passe
                    PasswordHash = hasher.HashPassword(null, "Passw0rd!"),
                    LockoutEnabled = true,

                },
            };
        }

        public static Client[] SeedClients()
        {
            
            return new Client[] {
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
                    FirstName = "Koomba",
                    LastName = "Ya",
                    UserId = "UserAdmin1Id",
                    IsAdmin = true,
                    IsBanned = false,
                    CartId = 2,
                    Username = "Admin",
                },
                new Client
                {
                    Id = 3,
                    FirstName = "Wooga",
                    LastName = "Bogga",
                    UserId = "UserAdmin2Id",
                    IsBanned = false,
                    IsAdmin = true,
                    CartId = 3,
                    Username = "Admin2",
                },
            };
        }
    }
}
