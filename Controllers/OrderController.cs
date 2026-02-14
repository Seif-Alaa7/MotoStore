using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Models;
using Store.Repositories;
using Store.Utility;
using Store.ViewModels; 
using System.Security.Claims;

namespace Store.Controllers
{
    [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Vendor)]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            IEnumerable<InquiryHeader> objInquiryHeaders;

            if (User.IsInRole(StaticDetails.Role_Admin))
            {
                objInquiryHeaders = _unitOfWork.InquiryHeader.GetAll(includeProperties: "ApplicationUser,Showroom");
            }
            else
            {
                objInquiryHeaders = _unitOfWork.InquiryHeader.GetAll(
                    u => u.Showroom.VendorId == userId,
                    includeProperties: "ApplicationUser,Showroom");
            }

            return View(objInquiryHeaders);
        }

        public IActionResult Details(int id)
        {
            InquiryVM inquiryVM = new InquiryVM()
            {
                InquiryHeader = _unitOfWork.InquiryHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser"),
                InquiryDetails = _unitOfWork.InquiryDetail.GetAll(u => u.InquiryHeaderId == id, includeProperties: "Motorcycle")
            };

            return View(inquiryVM);
        }
    }
}