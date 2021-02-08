using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rocky.Data;
using Rocky.Models;
using Rocky.Models.ViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Rocky.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {


            IEnumerable<Product> objList = _db.Product.Include(u=>u.Category).Include(u=>u.ApplicationType);

            //IEnumerable<Product> objList = _db.Product;

            //not eager loading
            //foreach (var obj in objList)
            //{
            //    obj.Category = _db.Category.FirstOrDefault(u => u.Id == obj.CategoryId);
            //    obj.ApplicationType = _db.ApplicationType.FirstOrDefault(u => u.Id == obj.ApplicationTypeId);
            //}

            //Console.WriteLine(objList);

            return View(objList);
        }

        // GET: Create
        public IActionResult Upsert(int? id)
        {

            //IEnumerable<SelectListItem> CategoryDropdown = _db.Category.Select(i => new SelectListItem
            //{
            //    Text = i.Name,
            //    Value = i.Id.ToString()

            //});

            //a view bag transfers data from controller to the view (and NOT vice versa), it is useful for temporary data that is not in a model
            //any number of properties can be passed to a viewbag,
            //the viewbags values will be null if any redirection occurs, so the data only persists between the controller and view
            //a viewBag is a wrapper around viewData
            //ViewBag.CategoryDropdown = CategoryDropdown;

            //doing similar thing with viewData, they are both loosely typed views
            // ViewData["CategoryDropdown"] = CategoryDropdown;

            //viewmodel better than the above two in this application:
            //viewmodel contains fields that are represented in the view
            //viewmodel can have specific validation rules using data annotations
            //viewmodel can have multiple entities or objects from different data models or data source
            //viewmodel helps to implement strongly typed views

            // Product product = new Product();

            //now, we have a strongly typed version of the previous code using view models.
            ProductViewModel productVM = new ProductViewModel()
            {
                Product = new Product(),
                CategorySelectlist = _db.Category.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()

                }),
                ApplicationTypeSelectlist = _db.ApplicationType.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()

                })

            }
            ;
            if(id == null)
            {
                //create
                return View(productVM);
            }
            else
            {
                productVM.Product = _db.Product.Find(id);
                if(productVM.Product == null)
                {
                    return NotFound();
                }
                return View(productVM);
            }

            
        }

        // POST: Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductViewModel productVM)
        {

            //server side validation
            if (ModelState.IsValid)
            {

                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;

                if (productVM.Product.Id == 0)
                {
                    //create
                    string upload = webRootPath + WebConst.IMGPath;
                    string fileName = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);

                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    productVM.Product.Image = fileName + extension;

                    _db.Product.Add(productVM.Product);
                    
                }
                else
                {
                    //update

                    //entity framework cannot trace two objects of the same instance, therefore we make this no tracking,
                    //so that we can call update on productVM.Product later on down below
                    var objFromDb = _db.Product.AsNoTracking().FirstOrDefault(u => u.Id == productVM.Product.Id);

                    if(files.Count > 0)
                    {
                        string upload = webRootPath + WebConst.IMGPath;
                        string fileName = Guid.NewGuid().ToString();
                        string extension = Path.GetExtension(files[0].FileName);

                        var oldFile = Path.Combine(upload, objFromDb.Image);

                        if (System.IO.File.Exists(oldFile))
                            System.IO.File.Delete(oldFile);

                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }
                        productVM.Product.Image = fileName + extension;
                    }
                    else
                    {
                        productVM.Product.Image = objFromDb.Image;
                    }
                    //here is the other instance that was being tracked before we added AsNoTracking(), so now only productVm.Product is being tracked
                    //so update method gives no errors
                    _db.Product.Update(productVM.Product);

                }
                _db.SaveChanges();
                return RedirectToAction("Index");

            } 
            else
            {
                productVM.CategorySelectlist = _db.Category.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()

                });
                productVM.ApplicationTypeSelectlist = _db.ApplicationType.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()

                });
                return View();
            }
            
            
        }


        //GET - Delete
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            //eager loading with join
            Product product = _db.Product.Include(u => u.Category).Include(u => u.ApplicationType).FirstOrDefault(u => u.Id == id);
                
            //product.Category = _db.Category.Find(product.CategoryId);
            //product.ApplicationType = _db.ApplicationType.Find(product.ApplicationTypeId);
            var obj = _db.Product.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        //POST - Delete
        [HttpPost, ActionName("Delete")]//defining custom action name for the form in html
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _db.Product.Find(id);



            if (obj == null)
                return NotFound();


            string upload = _webHostEnvironment.WebRootPath + WebConst.IMGPath;
            var oldFile = Path.Combine(upload, obj.Image);

            if (System.IO.File.Exists(oldFile))
                System.IO.File.Delete(oldFile);

            _db.Product.Remove(obj);
            _db.SaveChanges();

             return RedirectToAction("Index");

        }



    }
}
