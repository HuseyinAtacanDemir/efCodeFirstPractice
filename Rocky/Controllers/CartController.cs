using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rocky.Data;
using Rocky.Models;
using Rocky.Models.ViewModels;
using Rocky.Utility;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Rocky.Controllers
{
    [Authorize]
    public class CartController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;
        [BindProperty]
        public ProductUserVM ProductUserVM { get; set; }

        public CartController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
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

            //additions for my own solution
            //CartVM cartVM = new CartVM()
            //{
            //    Products = prodList
            //};

            

            //return to the view the prodList IEnumerable
            //return View(cartVM);
            return View(prodList);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost()
        {
            return RedirectToAction(nameof(Summary));
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            List<CartItem> ShoppingCart = new List<CartItem>();
            if (HttpContext.Session.Get<IEnumerable<CartItem>>(WebConst.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<CartItem>>(WebConst.SessionCart).Count() > 0)
            {
                ShoppingCart = HttpContext.Session.Get<List<CartItem>>(WebConst.SessionCart);
            }

            List<int> prodInCart = ShoppingCart.Select(i => i.ProductId).ToList();
            IEnumerable<Product> prodList = _db.Product.Include(u => u.Category).Include(u => u.ApplicationType).Where(u => prodInCart.Contains(u.Id));

            ProductUserVM = new ProductUserVM() {
                ApplicationUser = _db.ApplicationUser.FirstOrDefault(u => u.Id == claim.Value),
                ProductList = prodList.ToList()
            };
            return View(ProductUserVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(ProductUserVM ProductUserVM)//because we binded it we do not need to pass product user vm as a paramnter
        {

            var PathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                +"templates" + Path.DirectorySeparatorChar.ToString() + "Inquiry.html";
            var subject = "New Inquiry";
            string HtmlBody = "";

            using (StreamReader sr = System.IO.File.OpenText(PathToTemplate))
            {
                HtmlBody = sr.ReadToEnd();
            }

            StringBuilder productListSB = new StringBuilder();
            foreach (var prod in ProductUserVM.ProductList)
            {
                productListSB.Append($" - Name: {prod.Name} <span style='font-size: 14px;'> (ID: {prod.Id}) </span><br />");
            }

            string messageBody = string.Format(HtmlBody,
                ProductUserVM.ApplicationUser.FullName,
                ProductUserVM.ApplicationUser.Email,
                ProductUserVM.ApplicationUser.PhoneNumber,
                productListSB.ToString());
                

            await _emailSender.SendEmailAsync(WebConst.AdminEmail, subject, messageBody);

            return RedirectToAction(nameof(InquiryConfirmation));
        }

        public IActionResult InquiryConfirmation()//because we binded it we do not need to pass product user vm as a paramnter
        {
            HttpContext.Session.Clear();
            return View();
        }

        public IActionResult Remove(int id)
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
            

            HttpContext.Session.Set(WebConst.SessionCart, cartList);
            return RedirectToAction(nameof(Index));
        }

        /*
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

        */


    }
}
