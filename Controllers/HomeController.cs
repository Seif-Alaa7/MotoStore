using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Store.Models;
using Store.Repositories;
using System.Diagnostics;
using System.Security.Claims;

namespace Store.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public IActionResult Index(string searchString, decimal? maxPrice)
        {
            var motorcycles = _unitOfWork.Motorcycle.GetAll(includeProperties: "Showroom").ToList();

            decimal trueMaxPrice = motorcycles.Any() ? motorcycles.Max(m => m.Price) : 1000000;

            decimal sliderMax = Math.Ceiling(trueMaxPrice / 1000) * 1000;
            decimal dbMinPrice = motorcycles.Any() ? motorcycles.Min(m => m.Price) : 0;
            decimal sliderMin = Math.Floor(dbMinPrice / 1000) * 1000;

            ViewBag.GlobalMaxPrice = sliderMax;
            ViewBag.GlobalMinPrice = sliderMin;

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim().ToLower();
                motorcycles = motorcycles.Where(b =>
                    b.Brand.ToLower().Contains(searchString) ||
                    b.Model.ToLower().Contains(searchString) ||
                    (b.Description != null && b.Description.ToLower().Contains(searchString)) ||
                    (b.Showroom != null && b.Showroom.Name.ToLower().Contains(searchString))
                ).ToList();
            }

            if (maxPrice.HasValue)
            {
                motorcycles = motorcycles.Where(b => b.Price <= maxPrice.Value).ToList();
            }

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentPrice"] = maxPrice;

            return View(motorcycles);
        }

        [HttpGet]
        public IActionResult Details(int Id)
        {
            var motorcycle = _unitOfWork.Motorcycle.Get(u => u.Id == Id, includeProperties: "Showroom");
            ViewBag.GalleryImages = _unitOfWork.MotorcycleImage.GetAll(u => u.MotorcycleId == Id).ToList();

            ShoppingCart cart = new ShoppingCart()
            {
                Motorcycle = motorcycle,
                Count = 1,
                MotorcycleId = Id
            };
            if (cart.Motorcycle == null)
            {
                return NotFound();
            }
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCart.ApplicationUserId = userId;
            shoppingCart.Id = 0;

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(
                u => u.ApplicationUserId == userId && u.MotorcycleId == shoppingCart.MotorcycleId);

            if (cartFromDb != null)
            {
                cartFromDb.Count += shoppingCart.Count;
            }
            else
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
            }

            _unitOfWork.Save();

            TempData["success"] = "Item added to your inquiry list successfully!";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int motorcycleId)
        {
            var userId = _userManager.GetUserId(User);

            var motorcycleToCheck = _unitOfWork.Motorcycle.Get(
                u => u.Id == motorcycleId,
                includeProperties: "Showroom");

            if (motorcycleToCheck != null && motorcycleToCheck.Showroom.VendorId == userId)
            {
                TempData["error"] = "You cannot buy your own products!";
                return RedirectToAction(nameof(Index));
            }

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(
                u => u.ApplicationUserId == userId && u.MotorcycleId == motorcycleId);

            if (cartFromDb != null)
            {
                cartFromDb.Count += 1;
            }
            else
            {
                ShoppingCart cart = new ShoppingCart
                {
                    ApplicationUserId = userId,
                    MotorcycleId = motorcycleId,
                    Count = 1 
                };
                _unitOfWork.ShoppingCart.Add(cart);
            }

            _unitOfWork.Save();

            TempData["Success"] = "Item added to cart successfully";

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