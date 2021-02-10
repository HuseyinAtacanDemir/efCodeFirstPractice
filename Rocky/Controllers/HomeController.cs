using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rocky.Models;
using Rocky.Data;
using Rocky.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Rocky.Utility;

namespace Rocky.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {

            HomeVM homeVM = new HomeVM()
            {
                Products = _db.Product.Include(u=>u.Category).Include(u=>u.ApplicationType),
                Categories = _db.Category
            };
            return View(homeVM);
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
            if (cartList.Where(u=>u.ProductId==detailsVM.Product.Id).Count() > 0)
            {
                detailsVM.ExistsInCart = true;
            }
            return View(detailsVM);
        }

        [HttpPost, ActionName("Details")]
        public IActionResult DetailsPost(int id)
        {

            List<CartItem> cartList = new List<CartItem>();
            if (HttpContext.Session.Get<IEnumerable<CartItem>>(WebConst.SessionCart)!=null && HttpContext.Session.Get<IEnumerable<CartItem>>(WebConst.SessionCart).Count() > 0)
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



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
