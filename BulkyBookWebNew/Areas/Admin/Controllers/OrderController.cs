using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
[AutoValidateAntiforgeryToken]
public class OrderController : Controller
{
	private readonly IUnitOfWork _unitOfWork;
	[BindProperty]
	public OrderViewModel orderViewModel { get; set; }

	public OrderController(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public IActionResult Index()
	{
		return View();
	}

	public IActionResult Details(int orderId)
	{
		orderViewModel = new OrderViewModel()
		{
			OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(oh => oh.Id == orderId, includeProperties: "ApplicationUser"),
			OrderDetails = _unitOfWork.OrderDetail.GetAll(od => od.OrderId == orderId, includeProperties: "Product")
		};
		return View(orderViewModel);
	}

	[ActionName("Details")]
	[HttpPost]
	public async Task<IActionResult> DetailsPayNow()
	{
		orderViewModel.OrderHeader = _unitOfWork.OrderHeader
			.GetFirstOrDefault(oh => oh.Id == orderViewModel.OrderHeader.Id, includeProperties: "ApplicationUser");

		orderViewModel.OrderDetails = _unitOfWork.OrderDetail
			.GetAll(od => od.OrderId == orderViewModel.OrderHeader.Id, includeProperties: "Product");

		// stripe settings	
		var domain = "https://localhost:7239/";
		var options = new SessionCreateOptions
		{
			LineItems = new List<SessionLineItemOptions>(),
			Mode = "payment",
			SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderid={orderViewModel.OrderHeader.Id}",
			CancelUrl = domain + $"admin/order/details?orderId={orderViewModel.OrderHeader.Id}",
		};

		foreach (var item in orderViewModel.OrderDetails)
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
		_unitOfWork.OrderHeader.UpdateStripePaymentId(orderViewModel.OrderHeader.Id, sessionId: session.Id);
		await _unitOfWork.Save();

		Response.Headers.Add("Location", session.Url);
		return new StatusCodeResult(303);
	}

	public async Task<IActionResult> PaymentConfirmation(int orderHeaderid)
	{
		OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(oh => oh.Id == orderHeaderid);
		if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
		{
			var service = new SessionService();
			Session session = service.Get(orderHeader.SessionId);

			// check the stripe status
			if (session.PaymentStatus.ToLower() == "paid")
			{
				// update PaymentIntentId in OrderHeader
				//orderHeader.PaymentIntentId = session.PaymentIntentId;
				_unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeaderid, paymentIntentId: session.PaymentIntentId);
				_unitOfWork.OrderHeader.UpdateStatus(orderHeaderid, orderHeader.OrderStatus, SD.PaymentStatusApproved);
			}
		}
		await _unitOfWork.Save();
		return View(orderHeaderid);
	}

	[HttpPost]
	[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
	//[ValidateAntiForgeryToken]
	public async Task<IActionResult> UpdateOrderDetail()
	{
		var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(oh => oh.Id == orderViewModel.OrderHeader.Id, noTrack: true);
		orderHeaderFromDb.Name = orderViewModel.OrderHeader.Name;
		orderHeaderFromDb.PhoneNumber = orderViewModel.OrderHeader.PhoneNumber;
		orderHeaderFromDb.StreetAddress = orderViewModel.OrderHeader.StreetAddress;
		orderHeaderFromDb.City = orderViewModel.OrderHeader.City;
		orderHeaderFromDb.State = orderViewModel.OrderHeader.State;
		orderHeaderFromDb.PostalCode = orderViewModel.OrderHeader.PostalCode;
		if (orderViewModel.OrderHeader.Courier != null) 
		{
			orderHeaderFromDb.Courier = orderViewModel.OrderHeader.Courier;
		}
		if (orderViewModel.OrderHeader.TrackingNumber != null)
		{
			orderHeaderFromDb.TrackingNumber = orderViewModel.OrderHeader.TrackingNumber;
		}
		_unitOfWork.OrderHeader.Update(orderHeaderFromDb);
		await _unitOfWork.Save();
		TempData["success"] = "Order Details Update Successfully";
		return RedirectToAction("Details", "Order", new { orderId = orderHeaderFromDb.Id });
		//return View(orderHeaderFromDb);
	}

	[HttpPost]
	[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
	//[ValidateAntiForgeryToken]
	public async Task<IActionResult> StartProcessing()
	{
		_unitOfWork.OrderHeader.UpdateStatus(orderViewModel.OrderHeader.Id, SD.StatusInProcess);
		await _unitOfWork.Save();
		TempData["success"] = "Order Status Updated Successfully";
		return RedirectToAction("Details", "Order", new { orderId = orderViewModel.OrderHeader.Id });
		//return View(orderHeaderFromDb);
	}

	[HttpPost]
	[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
	//[ValidateAntiForgeryToken]
	public async Task<IActionResult> ShipOrder()
	{
		var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(oh => oh.Id == orderViewModel.OrderHeader.Id, noTrack: true);
		orderHeader.TrackingNumber = orderViewModel.OrderHeader.TrackingNumber;
		orderHeader.Courier = orderViewModel.OrderHeader.Courier;
		orderHeader.OrderStatus = SD.StatusShipped;
		orderHeader.ShippingDate = DateTime.Now;
		if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
		{
			orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
		}
		_unitOfWork.OrderHeader.Update(orderHeader);
		await _unitOfWork.Save();
		
		TempData["success"] = "Order Shipped Successfully";
		return RedirectToAction("Details", "Order", new { orderId = orderViewModel.OrderHeader.Id });
		//return View(orderHeaderFromDb);
	}

	[HttpPost]
	[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
	//[ValidateAntiForgeryToken]
	public async Task<IActionResult> CancelOrder()
	{
		var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(oh => oh.Id == orderViewModel.OrderHeader.Id, noTrack: true);
		if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
		{
			var options = new RefundCreateOptions
			{
				Reason = RefundReasons.RequestedByCustomer,
				PaymentIntent = orderHeader.PaymentIntentId,
			};

			var service = new RefundService();
			Refund refund = service.Create(options);

			_unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
		} else
		{
			_unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
		}
		await _unitOfWork.Save();

		TempData["success"] = "Order Cancelled Successfully";
		return RedirectToAction("Details", "Order", new { orderId = orderViewModel.OrderHeader.Id });
		//return View(orderHeaderFromDb);
	}


	#region API CALLS
	[HttpGet]
	public IActionResult GetAll(string status)
	{
		IEnumerable<OrderHeader> orderHeaders;

		if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee)) {
			orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
		} else
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
			orderHeaders = _unitOfWork.OrderHeader.GetAll(oh => oh.ApplicationUser.Id == claim.Value, includeProperties: "ApplicationUser");
		}
		switch(status)
		{
			case "pending":
				orderHeaders = orderHeaders.Where(oh => oh.PaymentStatus == SD.PaymentStatusDelayedPayment);
				break;
			case "inprocess":
				orderHeaders = orderHeaders.Where(oh => oh.OrderStatus == SD.StatusInProcess);
				break;
			case "completed":
				orderHeaders = orderHeaders.Where(oh => oh.OrderStatus == SD.StatusShipped);
				break;
			case "approved":
				orderHeaders = orderHeaders.Where(oh => oh.OrderStatus == SD.StatusApproved);
				break;
			default:
				break;
		}
		return Json(new { data = orderHeaders });
	}
	#endregion
}
