using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Models;
using Store.Repositories;
using Store.ViewModels;
using Store.Utility;
using System.Security.Claims;
using System.Collections.Generic;

namespace Store.Controllers
{
    [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Vendor)]
    public class InquiryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public InquiryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var identity = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            IEnumerable<InquiryHeader> objInquiryList;

            if (User.IsInRole(StaticDetails.Role_Admin))
            {
                objInquiryList = _unitOfWork.InquiryHeader.GetAll(includeProperties: "Showroom");
            }
            else
            {
                objInquiryList = _unitOfWork.InquiryHeader.GetAll(
                    u => u.Showroom.VendorId == identity.Value,
                    includeProperties: "Showroom");
            }

            return View(objInquiryList);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            InquiryVM inquiryVM = new InquiryVM()
            {
                InquiryHeader = _unitOfWork.InquiryHeader.Get(u => u.Id == id, includeProperties: "Showroom"),

                InquiryDetails = _unitOfWork.InquiryDetail.GetAll(u => u.InquiryHeaderId == id, includeProperties: "Motorcycle")
            };

            return View(inquiryVM);
        }

        [HttpDelete]
        public IActionResult DeleteInquiry(int id)
        {
            var inquiryHeader = _unitOfWork.InquiryHeader.Get(u => u.Id == id);

            if (inquiryHeader == null)
            {
                return Json(new { success = false, message = "Error: Inquiry not found" });
            }

            var inquiryDetails = _unitOfWork.InquiryDetail.GetAll(u => u.InquiryHeaderId == id);
            if (inquiryDetails != null)
            {
                _unitOfWork.InquiryDetail.RemoveRange(inquiryDetails);
            }

            _unitOfWork.InquiryHeader.Remove(inquiryHeader);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Inquiry deleted successfully" });
        }
    }
}