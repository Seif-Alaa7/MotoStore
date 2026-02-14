using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Store.Models;
using Store.Repositories;
using Store.Utility;
using Store.ViewModels;
using System.Security.Claims;

namespace Store.Controllers
{
    [Authorize]
    public class ShowroomController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShowroomController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            IEnumerable<Showroom> showrooms;

            if (User.IsInRole(StaticDetails.Role_Admin))
            {
                showrooms = _unitOfWork.Showroom.GetAll();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                showrooms = _unitOfWork.Showroom.GetAll(u => u.VendorId == userId);
            }

            return View(showrooms);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShowroomFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                string fileName = null;

                if (model.LogoFile != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images", "showrooms");

                    if (!Directory.Exists(uploadDir))
                        Directory.CreateDirectory(uploadDir);

                    fileName = Guid.NewGuid().ToString() + "-" + model.LogoFile.FileName;
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.LogoFile.CopyToAsync(fileStream);
                    }
                }

                var showroom = new Showroom
                {
                    Name = model.Name,
                    Address = model.Address,
                    VendorId = userId,
                    LogoUrl = fileName
                };

                _unitOfWork.Showroom.Add(showroom);
                _unitOfWork.Save();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var showroom = _unitOfWork.Showroom.Get(u => u.Id == id);

            if (showroom == null)
            {
                return NotFound();
            }

            if (!User.IsInRole(StaticDetails.Role_Admin))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (showroom.VendorId != userId)
                {
                    return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
                }
            }
            return View(showroom);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Showroom model, IFormFile? file)
        {
            if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.Address))
            {
                ModelState.AddModelError("", "Name and Address are required");
                return View(model);
            }

            var showroomFromDb = _unitOfWork.Showroom.Get(u => u.Id == model.Id);

            if (showroomFromDb == null)
            {
                return NotFound();
            }

            if (!User.IsInRole(StaticDetails.Role_Admin))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (showroomFromDb.VendorId != userId)
                {
                    return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
                }
            }

            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string uploadPath = Path.Combine(wwwRootPath, @"images\showrooms");

                if (!string.IsNullOrEmpty(showroomFromDb.LogoUrl))
                {
                    var oldImagePath = Path.Combine(wwwRootPath, "images", "showrooms", showroomFromDb.LogoUrl);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using (var fileStream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                showroomFromDb.LogoUrl = fileName;
            }
            showroomFromDb.Name = model.Name;
            showroomFromDb.Address = model.Address;

            _unitOfWork.Save();

            return RedirectToAction("Index");
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var showroom = _unitOfWork.Showroom.Get(u => u.Id == id);
            if (showroom == null)
            {
                return Json(new { success = false, message = "Error: Showroom not found" });
            }

            var motorcycles = _unitOfWork.Motorcycle.GetAll(u => u.ShowroomId == id);
            foreach (var bike in motorcycles)
            {
                if (!string.IsNullOrEmpty(bike.ImageUrl))
                {
                    var bikePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "motorcycles", bike.ImageUrl);
                    if (System.IO.File.Exists(bikePath)) System.IO.File.Delete(bikePath);
                }
                _unitOfWork.Motorcycle.Remove(bike);
            }

            var showroomOrders = _unitOfWork.InquiryHeader.GetAll(u => u.ShowroomId == id);
            foreach (var order in showroomOrders)
            {
                var orderDetails = _unitOfWork.InquiryDetail.GetAll(u => u.InquiryHeaderId == order.Id);
                _unitOfWork.InquiryDetail.RemoveRange(orderDetails);

                _unitOfWork.InquiryHeader.Remove(order);
            }

            if (!string.IsNullOrEmpty(showroom.LogoUrl))
            {
                var logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "showrooms", showroom.LogoUrl);
                if (System.IO.File.Exists(logoPath)) System.IO.File.Delete(logoPath);
            }

            _unitOfWork.Showroom.Remove(showroom);

            _unitOfWork.Save();

            return Json(new { success = true, message = "Showroom and all related data deleted successfully" });
        }
    }
}