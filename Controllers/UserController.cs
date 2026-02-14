using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Store.Models;
using Store.Repositories;
using Store.Utility;
using Store.ViewModels;

namespace Store.Controllers
{
    [Authorize(Roles = StaticDetails.Role_Admin)]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var currentUserId = _userManager.GetUserId(User);
            var users = _unitOfWork.ApplicationUser.GetAll(u => u.Id != currentUserId);

            var userList = new List<UserVM>();

            foreach (var user in users)
            {
                var roles = _userManager.GetRolesAsync(user).Result;
                userList.Add(new UserVM
                {
                    Id = user.Id,
                    Name = user.FirstName + " " + user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    LockoutEnd = user.LockoutEnd,
                    Role = roles.FirstOrDefault() ?? "Customer"
                });
            }

            return View(userList);
        }

        public IActionResult LockUnlock(string id)
        {
            var user = _unitOfWork.ApplicationUser.Get(u => u.Id == id);
            if (user == null) return NotFound();

            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
                user.LockoutEnd = DateTime.Now;
            else
                user.LockoutEnd = DateTime.Now.AddYears(1000);

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var user = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            if (user == null) return Json(new { success = false, message = "User not found" });

            var currentRoles = await _userManager.GetRolesAsync(user);
            var oldRole = currentRoles.FirstOrDefault();

            if (oldRole == StaticDetails.Role_Vendor && newRole != StaticDetails.Role_Vendor)
            {
                CleanUpVendorData(userId);
            }

            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            return Json(new { success = true, message = $"Role changed from {oldRole} to {newRole}" });
        }

        [HttpDelete]
        public IActionResult Delete(string id)
        {
            var user = _unitOfWork.ApplicationUser.Get(u => u.Id == id);
            if (user == null) return Json(new { success = false, message = "Error while deleting" });

            CleanUpVendorData(id);
            CleanUpCustomerData(id);

            _unitOfWork.ApplicationUser.Remove(user);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }

        private void CleanUpVendorData(string userId)
        {
            var showroom = _unitOfWork.Showroom.Get(u => u.VendorId == userId);
            if (showroom != null)
            {
                var motorcycles = _unitOfWork.Motorcycle.GetAll(u => u.ShowroomId == showroom.Id);
                foreach (var bike in motorcycles)
                {
                    if (!string.IsNullOrEmpty(bike.ImageUrl))
                    {
                        var bikePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "motorcycles", bike.ImageUrl);
                        if (System.IO.File.Exists(bikePath)) System.IO.File.Delete(bikePath);
                    }
                    var relatedInquiries = _unitOfWork.InquiryDetail.GetAll(u => u.MotorcycleId == bike.Id);
                    _unitOfWork.InquiryDetail.RemoveRange(relatedInquiries);
                    _unitOfWork.Motorcycle.Remove(bike);
                }
                var showroomOrders = _unitOfWork.InquiryHeader.GetAll(u => u.ShowroomId == showroom.Id);
                _unitOfWork.InquiryHeader.RemoveRange(showroomOrders);

                if (!string.IsNullOrEmpty(showroom.LogoUrl))
                {
                    var logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "showrooms", showroom.LogoUrl);
                    if (System.IO.File.Exists(logoPath)) System.IO.File.Delete(logoPath);
                }
                _unitOfWork.Showroom.Remove(showroom);
            }
        }

        private void CleanUpCustomerData(string userId)
        {
            var userOrders = _unitOfWork.InquiryHeader.GetAll(u => u.ApplicationUserId == userId);
            foreach (var order in userOrders)
            {
                var details = _unitOfWork.InquiryDetail.GetAll(u => u.InquiryHeaderId == order.Id);
                _unitOfWork.InquiryDetail.RemoveRange(details);
            }
            _unitOfWork.InquiryHeader.RemoveRange(userOrders);
        }
    }
}