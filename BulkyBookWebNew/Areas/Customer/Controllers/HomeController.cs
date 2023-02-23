    using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers;
[Area("Customer")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
	private readonly IUnitOfWork _unitOfWork;
    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
		_unitOfWork = unitOfWork;
	}

    public IActionResult Index(string? CategoryFilter, string? BookName)
    {
        HomeViewModel model = new()
        {
            //var productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
            //var categoryList = _unitOfWork.Category.GetAll();

            Products = (CategoryFilter.IsNullOrEmpty() || CategoryFilter == "all" ?
                _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType")
                :
                _unitOfWork.Product.GetAll(filter: (p => p.Category.Name == CategoryFilter), includeProperties: "Category,CoverType")),

            Categories = _unitOfWork.Category.GetAll(),
        };
        if (!BookName.IsNullOrEmpty())
        {
            model.Products = model.Products.Where(p => p.Title.ToLower().Contains(BookName.ToLower())).ToList();
        }
		return View(model);
    }
	public IActionResult Details(int productId)
	{
        ShoppingCart obj = new()
        {
            Count = 1,
            ProductId = productId,
            Product = _unitOfWork.Product.GetFirstOrDefault((p => p.Id == productId), includeProperties: "Category,CoverType"),
        };

		return View(obj);
	}

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Details(ShoppingCart shoppingCart)
    {
        // untuk meretrieve user id 
        // dapat diretrieve dari object User dan elemen Identity dan value dari nameIdentifier
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        shoppingCart.ApplicationUserId = claim.Value;

        ShoppingCart cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(sc => sc.ApplicationUserId == claim.Value && sc.ProductId == shoppingCart.ProductId);
        if (cart == null)
        {
            _unitOfWork.ShoppingCart.Add(shoppingCart);
            await _unitOfWork.Save();

            // menambahkan {key, pair} session
            HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claim.Value).ToList().Count);
        } else
        {
            _unitOfWork.ShoppingCart.IncrementCount(cart, shoppingCart.Count);
            await _unitOfWork.Save();
        }

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