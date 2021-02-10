using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rocky.Data;
using Rocky.Models;
using Rocky.Models.ViewModels;
using Rocky.Utility;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Rocky.Controllers
{
    public class CartController : Controller
    {

        private readonly ApplicationDbContext _db;

        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {

            //create a new list of cart items
            List<CartItem> ShoppingCart = new List<CartItem>();

            //if there is a session, get the cartItems saved in the session
            if (HttpContext.Session.Get<IEnumerable<CartItem>>(WebConst.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<CartItem>>(WebConst.SessionCart).Count() > 0)
            {
                //session exists
                ShoppingCart = HttpContext.Session.Get<List<CartItem>>(WebConst.SessionCart);
            }

            //create a list to hold the product ids in the cart
            List<int> prodInCart = ShoppingCart.Select(i => i.ProductId).ToList();
            //turn that list into a IEnumerable of product by selecting product objs from Product model where ids match between cart and db
            IEnumerable<Product> prodList = _db.Product.Include(u => u.Category).Include(u => u.ApplicationType).Where(u => prodInCart.Contains(u.Id));
            //_db.Product.Include(u => u.Category).Include(u => u.ApplicationType).Where(u => prodInCart.Contains(u.Id));

            CartVM cartVM = new CartVM()
            {
                Products = prodList
            };

            

            //return to the view the prodList IEnumerable
            return View(cartVM);
        }

        public IActionResult Details(int id)
        {

            List<CartItem> cartList = new List<CartItem>();
            if (HttpContext.Session.Get<IEnumerable<CartItem>>(WebConst.SessionCart) != null && HttpContext.Session.Get<IEnumerable<CartItem>>(WebConst.SessionCart).Count() > 0)
            {
                cartList = HttpContext.Session.Get<List<CartItem>>(WebConst.SessionCart);
            }


            DetailsVM detailsVM = new DetailsVM()
            {
                Product = _db.Product.Include(u => u.Category).Include(u => u.ApplicationType)
                .Where(u => u.Id == id).FirstOrDefault(),
                ExistsInCart = false

            };
            if (cartList.Where(u => u.ProductId == detailsVM.Product.Id).Count() > 0)
            {
                detailsVM.ExistsInCart = true;
            }
            return View(detailsVM);
        }

        [HttpPost, ActionName("Details")]
        public IActionResult DetailsPost(int id)
        {

            List<CartItem> cartList = new List<CartItem>();
            if (HttpContext.Session.Get<IEnumerable<CartItem>>(WebConst.SessionCart) != null && HttpContext.Session.Get<IEnumerable<CartItem>>(WebConst.SessionCart).Count() > 0)
            {
                cartList = HttpContext.Session.Get<List<CartItem>>(WebConst.SessionCart);
            }

            if (cartList.Where(u => u.ProductId == id).Count() > 0)
            {
                cartList.Remove(cartList.Where(u => u.ProductId == id).FirstOrDefault());
            }
            else
            {
                cartList.Add(new CartItem() { ProductId = id });
            }


            HttpContext.Session.Set(WebConst.SessionCart, cartList);

            return RedirectToAction(nameof(Index));
        }


    }
}
