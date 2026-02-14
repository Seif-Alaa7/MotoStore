using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Store.Models;
using Store.Repositories;
using Store.Utility;
using Store.ViewModels;
using System.Security.Claims;

namespace Store.Controllers
{
    [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Vendor)]
    public class MotorcycleController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public MotorcycleController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            IEnumerable<Motorcycle> objMotorcycleList;

            if (User.IsInRole(StaticDetails.Role_Admin))
            {
                objMotorcycleList = _unitOfWork.Motorcycle.GetAll(includeProperties: "Showroom");
            }
            else
            {
                var userId = _userManager.GetUserId(User);
                objMotorcycleList = _unitOfWork.Motorcycle.GetAll(
                    u => u.Showroom.VendorId == userId,
                    includeProperties: "Showroom"
                );
            }

            return View(objMotorcycleList);
        }

        public IActionResult Create()
        {
            var userId = _userManager.GetUserId(User);

            IEnumerable<Showroom> showroomsList;

            if (User.IsInRole(StaticDetails.Role_Admin))
            {
                showroomsList = _unitOfWork.Showroom.GetAll();
            }
            else
            {
                showroomsList = _unitOfWork.Showroom.GetAll(u => u.VendorId == userId);
            }

            var viewModel = new MotorcycleFormVM
            {
                Showrooms = showroomsList.Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
            };

            if (!viewModel.Showrooms.Any())
            {
                TempData["Error"] = "You must create a showroom first!";
                return RedirectToAction("Create", "Showroom");
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MotorcycleFormVM viewModel, List<IFormFile> galleryFiles)
        {
            ModelState.Remove("Showrooms");
            ModelState.Remove("ImageFile");

            if (ModelState.IsValid)
            {
                string mainFileName = null;
                if (viewModel.ImageFile != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images", "motorcycles");
                    if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                    mainFileName = Guid.NewGuid().ToString() + Path.GetExtension(viewModel.ImageFile.FileName);
                    string filePath = Path.Combine(uploadDir, mainFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await viewModel.ImageFile.CopyToAsync(fileStream);
                    }
                }

                List<string> uploadedGalleryNames = new List<string>();
                if (galleryFiles != null && galleryFiles.Count > 0)
                {
                    string galleryDir = Path.Combine(_webHostEnvironment.WebRootPath, "images", "motorcycles", "gallery");
                    if (!Directory.Exists(galleryDir)) Directory.CreateDirectory(galleryDir);

                    foreach (var file in galleryFiles)
                    {
                        if (file.Length > 0)
                        {
                            string gFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            string gPath = Path.Combine(galleryDir, gFileName);

                            using (var stream = new FileStream(gPath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                            uploadedGalleryNames.Add(gFileName);
                        }
                    }
                }

                foreach (var showroomId in viewModel.ShowroomIds)
                {
                    var motorcycle = new Motorcycle
                    {
                        Brand = viewModel.Brand,
                        Model = viewModel.Model,
                        Price = viewModel.Price,
                        Description = viewModel.Description ?? "No Description",
                        StockQuantity = viewModel.StockQuantity,
                        ShowroomId = showroomId,
                        ImageUrl = mainFileName
                    };

                    _unitOfWork.Motorcycle.Add(motorcycle);
                    _unitOfWork.Save();

                    if (uploadedGalleryNames.Any())
                    {
                        foreach (var gName in uploadedGalleryNames)
                        {
                            var motorcycleImage = new MotorcycleImage
                            {
                                ImageUrl = gName,
                                MotorcycleId = motorcycle.Id
                            };
                            _unitOfWork.MotorcycleImage.Add(motorcycleImage);
                        }
                        _unitOfWork.Save();
                    }
                }

                TempData["success"] = "Motorcycles added successfully to selected showrooms.";
                return RedirectToAction(nameof(Index));
            }

            var currentUserId = _userManager.GetUserId(User);
            IEnumerable<Showroom> list;

            if (User.IsInRole(StaticDetails.Role_Admin))
                list = _unitOfWork.Showroom.GetAll();
            else
                list = _unitOfWork.Showroom.GetAll(s => s.VendorId == currentUserId);

            viewModel.Showrooms = list.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var bike = _unitOfWork.Motorcycle.Get(m => m.Id == id, includeProperties: "Showroom");
            if (bike == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (!User.IsInRole(StaticDetails.Role_Admin))
            {
                if (bike.Showroom.VendorId != userId)
                {
                    return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
                }
            }

            var viewModel = new MotorcycleFormVM
            {
                Id = bike.Id,
                Brand = bike.Brand,
                Model = bike.Model,
                Price = bike.Price,
                Description = bike.Description,
                StockQuantity = bike.StockQuantity,
                ShowroomIds = new List<int> { bike.ShowroomId },
                ImageUrl = bike.ImageUrl
            };

            IEnumerable<Showroom> showroomsList;
            if (User.IsInRole(StaticDetails.Role_Admin))
            {
                showroomsList = _unitOfWork.Showroom.GetAll();
            }
            else
            {
                showroomsList = _unitOfWork.Showroom.GetAll(s => s.VendorId == userId);
            }

            viewModel.Showrooms = showroomsList.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MotorcycleFormVM viewModel)
        {
            ModelState.Remove("Showrooms");
            ModelState.Remove("ImageFile");

            if (ModelState.IsValid)
            {
                var bikeFromDb = _unitOfWork.Motorcycle.Get(m => m.Id == viewModel.Id);
                if (bikeFromDb == null) return NotFound();

                if (!User.IsInRole(StaticDetails.Role_Admin))
                {
                    var showroom = _unitOfWork.Showroom.Get(s => s.Id == bikeFromDb.ShowroomId);
                    var userId = _userManager.GetUserId(User);
                    if (showroom.VendorId != userId)
                    {
                        return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
                    }
                }

                if (viewModel.ImageFile != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images", "motorcycles");

                    if (!string.IsNullOrEmpty(bikeFromDb.ImageUrl))
                    {
                        var oldPath = Path.Combine(uploadDir, bikeFromDb.ImageUrl);
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(viewModel.ImageFile.FileName);
                    using (var fileStream = new FileStream(Path.Combine(uploadDir, fileName), FileMode.Create))
                    {
                        await viewModel.ImageFile.CopyToAsync(fileStream);
                    }
                    bikeFromDb.ImageUrl = fileName;
                }

                bikeFromDb.Brand = viewModel.Brand;
                bikeFromDb.Model = viewModel.Model;
                bikeFromDb.Price = viewModel.Price;
                bikeFromDb.Description = viewModel.Description;
                bikeFromDb.StockQuantity = viewModel.StockQuantity;

                if (viewModel.ShowroomIds != null && viewModel.ShowroomIds.Any())
                {
                    bikeFromDb.ShowroomId = viewModel.ShowroomIds.FirstOrDefault();
                }

                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }

            var currentUserId = _userManager.GetUserId(User);
            IEnumerable<Showroom> list;

            if (User.IsInRole(StaticDetails.Role_Admin))
                list = _unitOfWork.Showroom.GetAll();
            else
                list = _unitOfWork.Showroom.GetAll(s => s.VendorId == currentUserId);

            viewModel.Showrooms = list.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });

            return View(viewModel);
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var bike = _unitOfWork.Motorcycle.Get(m => m.Id == id, includeProperties: "Showroom");

            if (bike == null)
            {
                return Json(new { success = false, message = "Error while deleting: Not Found" });
            }

            var relatedInquiryDetails = _unitOfWork.InquiryDetail.GetAll(u => u.MotorcycleId == id);
            if (relatedInquiryDetails.Any())
            {
                _unitOfWork.InquiryDetail.RemoveRange(relatedInquiryDetails);
            }

            var relatedCartItems = _unitOfWork.ShoppingCart.GetAll(u => u.MotorcycleId == id);
            if (relatedCartItems.Any())
            {
                _unitOfWork.ShoppingCart.RemoveRange(relatedCartItems);
            }

            if (!User.IsInRole(StaticDetails.Role_Admin))
            {
                var userId = _userManager.GetUserId(User);
                if (bike.Showroom != null && bike.Showroom.VendorId != userId)
                {
                    return Json(new { success = false, message = "Access Denied: You don't own this item" });
                }
            }

            if (!string.IsNullOrEmpty(bike.ImageUrl))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "motorcycles", bike.ImageUrl);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _unitOfWork.Motorcycle.Remove(bike);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Motorcycle deleted successfully" });
        }
    }
}