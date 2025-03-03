
using API_LapinCouvert.Services;
using LapinCouvert.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using MVC_LapinCouvert.Data;

namespace API_LapinCouvert.Controllers

{

    [Route("api/[controller]/[action]")]

    [ApiController]

    public class CartController : ControllerBase
    {

        private readonly CartService _cartService;

        public CartController(CartService cartService)

        {
            _cartService = cartService;

        }

        [Authorize]
        [HttpGet("{clientId}")]
        public async Task<ActionResult<IEnumerable<CartProducts>>> GetCartProducts(int clientId)
        {
            List<CartProducts> cartProducts = await _cartService.GetCartProductFromClientId(clientId);
            return cartProducts;
        }

        [Authorize]
        [HttpPost("{productId}/{clientId}")]
        public async Task­<ActionResult> AddCartProduct(int productId, int clientId)
        {
            Cart? cart = _cartService.GetCartFromClientId(clientId);
            Product? product = _cartService.GetProductFromProductId(productId);


            if (cart == null || product == null)
            {
                return BadRequest("Le produit ou votre panier est introuvable.");
            }

            if (product.Quantity <= 0)
            {
                return BadRequest("Ce produit est épuisé");
            }

            if (product.IsDeleted)
            {
                return BadRequest("Ce produit n'existe plus");
            }


            CartProducts cartProductFromRequest = new CartProducts(product, cart.Id);

            CartProducts? cartProduct = _cartService.GetCartProductIfAlready(productId, cart.Id);


            if (cartProduct == null)
            {
                CartProducts cp1 = _cartService.AddCartProductDB(cartProductFromRequest);

                return Ok(cartProductFromRequest);
            }
            cartProduct.Quantity++;
            CartProducts cp = _cartService.IncreaseQuantity(cartProduct);
            return Ok(cp);

        }


        [Authorize]
        [HttpPost("{productId}/{clientId}")]
        public async Task­<ActionResult> AddQuantityCartProduct(int productId, int clientId)
        {
            Cart? cart = _cartService.GetCartFromClientId(clientId);
            Product? product = _cartService.GetProductFromProductId(productId);

            if (cart == null || product == null)
            {
                return BadRequest("Le produit ou votre panier est introuvable.");
            }



            CartProducts? cp = _cartService.GetCartProductIfAlready(productId, cart.Id);

            cp.Quantity++;
            cp.MaxQuantity = product.Quantity;

            CartProducts cartProducts = _cartService.IncreaseQuantityCartProduct(cp, product);
            return Ok(cartProducts);

        }

        [Authorize]
        [HttpPost("{productId}/{clientId}")]
        public async Task­<ActionResult> DecreaseQuantityCartProduct(int productId, int clientId)
        {
            Cart? cart = _cartService.GetCartFromClientId(clientId);
            Product? product = _cartService.GetProductFromProductId(productId);

            if (cart == null || product == null)
            {
                return BadRequest("Le produit ou votre panier est introuvable.");
            }



            CartProducts? cartProduct = _cartService.GetCartProductIfAlready(productId, cart.Id);

            cartProduct.Quantity--;
            cartProduct.MaxQuantity = product.Quantity;

            CartProducts cartProducts = _cartService.DecreaseQuantityCartProduct(cartProduct, product);
            return Ok(cartProducts);

        }

        [Authorize]
        [HttpPost("{productId}/{clientId}")]
        public async Task­<ActionResult> DeleteCartProduct(int productId, int clientId)
        {
            Cart? cart = _cartService.GetCartFromClientId(clientId);
            Product? product = _cartService.GetProductFromProductId(productId);

            if (cart == null || product == null)
            {
                return BadRequest("Le produit ou votre panier est introuvable.");
            }



            CartProducts? cartProduct = _cartService.GetCartProductIfAlready(productId, cart.Id);

            _cartService.DeleteCartProduct(cartProduct);
            return Ok("Produit supprimé");

        }

        [Authorize]
        [HttpPost]
        public async Task­<ActionResult> ValidateCartProduct(List<CartProducts> cartProducts)
        {

            if (cartProducts.Count == 0)
            {
                return BadRequest("Le panier est vide");
            }


            if (_cartService.isCartvalid(cartProducts))
            {
                return Ok("Le panier est valide");
            }
            return BadRequest("Votre panier contient des produits invalides");

        }

        private bool isCartvalid(List<CartProducts> cartProducts)
        {
            foreach (CartProducts cp in cartProducts)
            {
                Product? product = _cartService.GetProductFromProductId(cp.ProductId);
                if (cp.isOutofStock == true || cp.Quantity > cp.MaxQuantity || product.IsDeleted)
                {
                    return false;
                }
            }

            return true;
        }

    }

}