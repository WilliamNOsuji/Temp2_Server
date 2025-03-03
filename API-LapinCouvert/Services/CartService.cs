using LapinCouvert.Models;
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
                Product product = _context.Products.Where(p => p.Id == cp.ProductId).SingleOrDefault();
                cp.MaxQuantity = product.Quantity;

                _context.CartProducts.Update(cp);
                _context.SaveChanges();
            }
            return cartProducts;
        }

        public virtual Cart GetCartFromClientId(int clientId)
        {
            Cart cart = _context.Carts.Where(c => c.ClientId == clientId).FirstOrDefault();
            return cart;
        }

        public virtual Cart GetCartWithIncludeFromClientId(int clientId)
        {
            Cart cart = _context.Carts
                               .Include(c => c.CartProducts)
                               .FirstOrDefault(cart => cart.ClientId == clientId);
            return cart;
        }

        public virtual Product GetProductFromProductId(int productId)
        {
            Product product = _context.Products.Where(c => c.Id == productId).FirstOrDefault();
            return product;
        }

        public virtual CartProducts? GetCartProductIfAlready(int productId, int cartId)
        {
            CartProducts? cartProduct = _context.CartProducts.Where(cp => cp.CartId == cartId && cp.ProductId == productId).FirstOrDefault();

            return cartProduct;
        }

        public virtual CartProducts AddCartProductDB(CartProducts cart)
        {
            _context.CartProducts.Add(cart);
            _context.SaveChanges();
            return cart;
        }

        public virtual CartProducts IncreaseQuantity(CartProducts cartProduct)
        {
            _context.CartProducts.Update(cartProduct);
            _context.SaveChanges();
            return cartProduct;
        }

        public virtual CartProducts IncreaseQuantityCartProduct(CartProducts cartProduct, Product product)
        {


            _context.CartProducts.Update(cartProduct);
            _context.SaveChanges();
            return cartProduct;
        }
        public virtual CartProducts DecreaseQuantityCartProduct(CartProducts cartProduct, Product product)
        {


            _context.CartProducts.Update(cartProduct);
            _context.SaveChanges();
            return cartProduct;
        }

        public virtual CartProducts DeleteCartProduct(CartProducts cartProduct)
        {
            _context.CartProducts.Remove(cartProduct);
            _context.SaveChanges();
            return cartProduct;
        }

        public virtual bool isCartvalid(List<CartProducts> cartProducts)
        {
            foreach (CartProducts cp in cartProducts)
            {
                Product? product = GetProductFromProductId(cp.ProductId);
                if (cp.isOutofStock == true || cp.Quantity > cp.MaxQuantity || product.IsDeleted)
                {
                    return false;
                }
            }

            return true;
        }

    }
}
