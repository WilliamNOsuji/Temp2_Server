using LapinCouvert.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Models;
using MVC_LapinCouvert.Data;

namespace API_LapinCouvert.Services
{
    public class CartService
    {
        private ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }
        public CartService()
        {
        }

        public async Task<List<CartProducts>> GetCartProductFromClientId(int clientId)
        {
            var cartProducts = await _context.CartProducts.Where(cp => cp.Cart.ClientId == clientId).OrderBy(cp => cp.Product.Name).ToListAsync();
            foreach (CartProducts cp in cartProducts)
            {
                Product product = await _context.Products.Where(p => p.Id == cp.ProductId).SingleOrDefaultAsync();
                cp.MaxQuantity = product.Quantity;

                _context.CartProducts.Update(cp);
                _context.SaveChanges();
            }
            return cartProducts;
        }

        public virtual async Task­<Cart> GetCartFromClientId(int clientId)
        {
            return await _context.Carts.Where(c => c.ClientId == clientId).FirstOrDefaultAsync();
           
        }

        public virtual async Task<Cart> GetCartWithIncludeFromClientId(int clientId)
        {
            Cart cart = await _context.Carts
                               .Include(c => c.CartProducts)
                               .FirstOrDefaultAsync(cart => cart.ClientId == clientId);
            return cart;
        }

        public virtual async Task<Product> GetProductFromProductId(int productId)
        {
            Product product = await _context.Products.Where(c => c.Id == productId).FirstOrDefaultAsync();
            return product;
        }

        public virtual async Task<CartProducts?> GetCartProductIfAlready(int productId, int cartId)
        {
            CartProducts? cartProduct = await _context.CartProducts.Where(cp => cp.CartId == cartId && cp.ProductId == productId).FirstOrDefaultAsync();

            return cartProduct;
        }

        public virtual async Task<CartProducts> AddCartProductDB(CartProducts cart)
        {
            await _context.CartProducts.AddAsync(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        public virtual async Task<CartProducts> IncreaseQuantity(CartProducts cartProduct)
        {
            _context.CartProducts.Update(cartProduct);
            await _context.SaveChangesAsync();
            return cartProduct;
        }

        public virtual async Task<CartProducts> IncreaseQuantityCartProduct(CartProducts cartProduct, Product product)
        {
            _context.CartProducts.Update(cartProduct);
            await _context.SaveChangesAsync();
            return cartProduct;
        }
        public virtual async Task<CartProducts> DecreaseQuantityCartProduct(CartProducts cartProduct, Product product)
        {
            _context.CartProducts.Update(cartProduct);
            await _context.SaveChangesAsync();
            return cartProduct;
        }

        public virtual async Task<CartProducts> DeleteCartProduct(CartProducts cartProduct)
        {
            _context.CartProducts.Remove(cartProduct);
            await _context.SaveChangesAsync();
            return cartProduct;
        }

        public virtual async Task<bool> isCartvalid(List<CartProducts> cartProducts)
        {
            foreach (CartProducts cp in cartProducts)
            {
                Product? product = await GetProductFromProductId(cp.ProductId);
                if (cp.isOutofStock == true || cp.Quantity > cp.MaxQuantity || product.IsDeleted)
                {
                    return false;
                }
            }

            return true;
        }

    }
}
