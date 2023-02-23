using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers;
[Area("Customer")]
[Authorize]
[AutoValidateAntiforgeryToken]
public class CartController : Controller
{
	private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;

    [BindProperty]
	public ShoppingCartViewModel ShoppingCartViewModel { get; set; }
	public double OrderTotal { get; set; }
	public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender)
	{
		_unitOfWork = unitOfWork;
        _emailSender = emailSender;
    }
	//[AllowAnonymous]
	public IActionResult Index()
	{
		var claimsIdentity = (ClaimsIdentity)User.Identity;
		var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

		ShoppingCartViewModel = new ShoppingCartViewModel()
		{
			ListCart = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claim.Value, includeProperties: "Product"),
			OrderHeader = new(),
		};

		foreach(var cart in ShoppingCartViewModel.ListCart)
		{
			cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
			ShoppingCartViewModel.OrderHeader.OrderTotal += (cart.Price * cart.Count);
		}
		//ShoppingCartViewModel.CartTotal = OrderTotal;
		return View(ShoppingCartViewModel);
	}

	public IActionResult Summary()
	{
		var claimsIdentity = (ClaimsIdentity)User.Identity;
		var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

		ShoppingCartViewModel = new ShoppingCartViewModel()
		{
			ListCart = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claim.Value, includeProperties: "Product"),
			OrderHeader = new(),
		};

		ShoppingCartViewModel.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

		ShoppingCartViewModel.OrderHeader.Name = ShoppingCartViewModel.OrderHeader.ApplicationUser.Name;
		ShoppingCartViewModel.OrderHeader.PhoneNumber = ShoppingCartViewModel.OrderHeader.ApplicationUser.PhoneNumber;
		ShoppingCartViewModel.OrderHeader.StreetAddress = ShoppingCartViewModel.OrderHeader.ApplicationUser.StreetAddress;
		ShoppingCartViewModel.OrderHeader.City = ShoppingCartViewModel.OrderHeader.ApplicationUser.City;
		ShoppingCartViewModel.OrderHeader.State = ShoppingCartViewModel.OrderHeader.ApplicationUser.State;
		ShoppingCartViewModel.OrderHeader.PostalCode = ShoppingCartViewModel.OrderHeader.ApplicationUser.PostalCode;

		foreach (var cart in ShoppingCartViewModel.ListCart)
		{
			cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
			ShoppingCartViewModel.OrderHeader.OrderTotal += (cart.Price * cart.Count);
		}

		return View(ShoppingCartViewModel);
	}

	[HttpPost]
	[ActionName("Summary")]
	//[ValidateAntiForgeryToken]
	public async Task<IActionResult> SummaryPOST()
	{
		var claimsIdentity = (ClaimsIdentity)User.Identity;
		var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

		ShoppingCartViewModel.ListCart = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claim.Value, includeProperties: "Product");

		var applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

		// if not Company user it means he/she dont have CompanyId
		if (applicationUser.CompanyId.GetValueOrDefault() == 0)
		{
			ShoppingCartViewModel.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
			ShoppingCartViewModel.OrderHeader.OrderStatus = SD.StatusPending;
		} else
		{
			ShoppingCartViewModel.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
			ShoppingCartViewModel.OrderHeader.OrderStatus = SD.StatusApproved;
		}

		ShoppingCartViewModel.OrderHeader.OrderDate = DateTime.UtcNow;
		ShoppingCartViewModel.OrderHeader.ApplicationId = claim.Value;

		foreach (var cart in ShoppingCartViewModel.ListCart)
		{
			cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
			ShoppingCartViewModel.OrderHeader.OrderTotal += (cart.Price * cart.Count);

			var detail = new OrderDetail()
			{
				ProductId = cart.ProductId,
				//1 OrderId = ShoppingCartViewModel.OrderHeader.Id,
				//1 Using the navigation property instead so that the entire operation is OLTP
				//1 Kalau no1 dijalankan akan dapat error SqlExecption 

				OrderHeader = ShoppingCartViewModel.OrderHeader,
				Price = cart.Price,
				Count = cart.Count,
			};
			_unitOfWork.OrderDetail.Add(detail);
			// await _unitOfWork.Save();
		}

		_unitOfWork.OrderHeader.Add(ShoppingCartViewModel.OrderHeader);
		await _unitOfWork.Save();

		if (applicationUser.CompanyId.GetValueOrDefault() == 0)
		{
			// stripe settings	
			var domain = "https://localhost:7239/";
			var options = new SessionCreateOptions
			{
				LineItems = new List<SessionLineItemOptions>(),
				Mode = "payment",
				SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartViewModel.OrderHeader.Id}",
				CancelUrl = domain + $"customer/cart/index",
			};

			foreach (var item in ShoppingCartViewModel.ListCart)
			{
				var sessionLineItem = new SessionLineItemOptions
				{
					PriceData = new SessionLineItemPriceDataOptions
					{
						UnitAmount = (long)(item.Price * 100),
						Currency = "usd",
						ProductData = new SessionLineItemPriceDataProductDataOptions
						{
							Name = item.Product.Title,
						},
					},
					Quantity = item.Count,
				};
				options.LineItems.Add(sessionLineItem);
			}

			var service = new SessionService();
			Session session = service.Create(options);

			// update sessionId in OrderHeader
			//ShoppingCartViewModel.OrderHeader.SessionId = session.Id;
			_unitOfWork.OrderHeader.UpdateStripePaymentId(ShoppingCartViewModel.OrderHeader.Id, sessionId: session.Id);
			await _unitOfWork.Save();

			Response.Headers.Add("Location", session.Url);
			return new StatusCodeResult(303);
		} else
		{
			return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartViewModel.OrderHeader.Id });
		}
	}
	public async Task<IActionResult> OrderConfirmation(int id)
	{
		OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(oh => oh.Id == id, includeProperties: "ApplicationUser");
		if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
		{
			var service = new SessionService();
			Session session = service.Get(orderHeader.SessionId);

			// check the stripe status
			if (session.PaymentStatus.ToLower() == "paid")
			{
				// update PaymentIntentId in OrderHeader
				//orderHeader.PaymentIntentId = session.PaymentIntentId;
				_unitOfWork.OrderHeader.UpdateStripePaymentId(id, paymentIntentId: session.PaymentIntentId);
				_unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
			}
		}

		await _emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - Bulky Book", "<p>New Order Created<p>");
		List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == orderHeader.ApplicationId).ToList();
		_unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
		HttpContext.Session.Clear();
		await _unitOfWork.Save();
		return View(id);
	}
	public async Task<IActionResult> Increment(int id)
	{
		var obj = _unitOfWork.ShoppingCart.GetFirstOrDefault(sc => sc.Id == id);
		_unitOfWork.ShoppingCart.IncrementCount(obj, 1);
		TempData["success"] = "Incrementing Count cart successfully";
		await _unitOfWork.Save();
		return RedirectToAction("Index");
	}	
	public async Task<IActionResult> Decrement(int id)
	{
		var obj = _unitOfWork.ShoppingCart.GetFirstOrDefault(sc => sc.Id == id);
		if (obj.Count > 1)
		{
			_unitOfWork.ShoppingCart.DecrementCount(obj, 1);
			TempData["success"] = "Decrementing Count cart successfully";
			await _unitOfWork.Save();
		} else
		{
			TempData["unsuccess"] = "Dear Customer, You can't buy 0 book";
		}
		return RedirectToAction("Index");
	}	
	public async Task<IActionResult> Remove(int id)
	{
		var obj = _unitOfWork.ShoppingCart.GetFirstOrDefault(sc => sc.Id == id);
		_unitOfWork.ShoppingCart.Remove(obj);
		await _unitOfWork.Save();
		//var count = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == obj.ApplicationUserId).ToList().Count - 1;

        HttpContext.Session
			.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCart
				.GetAll(sc => sc.ApplicationUserId == obj.ApplicationUserId)
					.ToList().Count);

        return RedirectToAction("Index");
	}
	private double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
	{
		if (quantity > 100)
		{
			return price100;
		} else if (quantity > 50)
		{
			return price50;
		} else
		{
			return price;
		}
	}
}
