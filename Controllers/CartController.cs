using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Models;
using Store.Repositories;
using Store.ViewModels;
using System.Security.Claims;

namespace Store.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var shoppingCartVM = new ShoppingCartVM()
            {
                CartList = _unitOfWork.ShoppingCart.GetAll(
                    u => u.ApplicationUserId == userId,
                    includeProperties: "Motorcycle")
            };

            foreach (var cart in shoppingCartVM.CartList)
            {
                shoppingCartVM.OrderTotal += (cart.Motorcycle.Price * cart.Count);
            }

            return View(shoppingCartVM);
        }
        public IActionResult Plus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            cart.Count += 1;
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);

            if (cart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cart);
            }
            else
            {
                cart.Count -= 1;
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;


            var shoppingCartVM = new ShoppingCartVM()
            {
                CartList = _unitOfWork.ShoppingCart.GetAll(
                    u => u.ApplicationUserId == userId,
                    includeProperties: "Motorcycle"),

                InquiryHeader = new InquiryHeader()
            };

            var userEmail = claimsIdentity.FindFirst(ClaimTypes.Email)?.Value;
            shoppingCartVM.InquiryHeader.Email = userEmail;

            foreach (var item in shoppingCartVM.CartList)
            {
                shoppingCartVM.OrderTotal += (item.Motorcycle.Price * item.Count);
            }

            return View(shoppingCartVM);
        }
        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPost(ShoppingCartVM shoppingCartVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ModelState.Remove("CartList");
            ModelState.Remove("InquiryHeader.ApplicationUserId");
            ModelState.Remove("InquiryHeader.InquiryStatus");

            if (!ModelState.IsValid)
            {
                shoppingCartVM.CartList = _unitOfWork.ShoppingCart.GetAll(
                    u => u.ApplicationUserId == userId,
                    includeProperties: "Motorcycle"
                );

                return View("Summary", shoppingCartVM);
            }

            var cartList = _unitOfWork.ShoppingCart.GetAll(
                u => u.ApplicationUserId == userId,
                includeProperties: "Motorcycle");

            var showroomGroups = cartList.GroupBy(u => u.Motorcycle.ShowroomId);

            foreach (var group in showroomGroups)
            {
                InquiryHeader header = new InquiryHeader
                {
                    ApplicationUserId = userId,
                    ShowroomId = group.Key,
                    InquiryDate = DateTime.Now,

                    FullName = shoppingCartVM.InquiryHeader.FullName,
                    PhoneNumber = shoppingCartVM.InquiryHeader.PhoneNumber,
                    Email = shoppingCartVM.InquiryHeader.Email,

                };

                _unitOfWork.InquiryHeader.Add(header);
                _unitOfWork.Save();

                foreach (var item in group)
                {
                    InquiryDetail detail = new InquiryDetail
                    {
                        InquiryHeaderId = header.Id,
                        MotorcycleId = item.MotorcycleId,
                        Count = item.Count
                    };
                    _unitOfWork.InquiryDetail.Add(detail);
                }
            }

            _unitOfWork.Save();

            _unitOfWork.ShoppingCart.RemoveRange(cartList);
            _unitOfWork.Save();

            return RedirectToAction(nameof(OrderConfirmation));
        }

        public IActionResult OrderConfirmation()
        {
            return View();
        }
    }
}