using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers;
[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]

public class ProductController : Controller
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IWebHostEnvironment _webHostEnvironment;

	public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
	{
		_unitOfWork = unitOfWork;
		_webHostEnvironment = webHostEnvironment;
	}
	public IActionResult Index()
	{
		return View();
	}
	public IActionResult Upsert(int? id)
	{
		var productVM = new ProductViewModel()
		{
			Product = new(),
			CategoryList = _unitOfWork.Category.GetAll()
			.Select(c =>
			new SelectListItem
			{
				Text = c.Name,
				Value = c.Id.ToString()
			}),
			CoverTypeList = _unitOfWork.CoverType.GetAll()
			.Select(c =>
			new SelectListItem
			{
				Text = c.Name,
				Value = c.Id.ToString()
			})
		};
		if (id == 0 || id == null)
		{
			// Create Product
			//ViewData["CoverTypeList"] = CoverTypeList;
			//ViewBag.CategoryList = CategoryList;
			return View(productVM);

		} else
		{
			// Update Product
			productVM.Product = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == id);
			return View(productVM);
		}

	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Upsert(ProductViewModel obj, IFormFile? file)
	{
		if (ModelState.IsValid)
		{
			string wwwRootPath = _webHostEnvironment.WebRootPath;
			if (file != null)
			{
				string fileName = Guid.NewGuid().ToString();
				var uploads = Path.Combine(wwwRootPath, @"images\products");
				var extension = Path.GetExtension(file.FileName);

				// delete image
				if (obj.Product.ImageUrl != null)
				{
					var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));
					if (System.IO.File.Exists(oldImagePath))
					{
						System.IO.File.Delete(oldImagePath);
					}
				}

				using var fileStream = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create);
				file.CopyTo(fileStream);

				obj.Product.ImageUrl = @"\images\products\" + fileName + extension;
			}

			if (obj.Product.Id == 0)
			{
				_unitOfWork.Product.Add(obj.Product);
				TempData["success"] = "Product created succesfully";
			} else
			{
				_unitOfWork.Product.Update(obj.Product);
				TempData["success"] = "Product updated succesfully";
			}
			await _unitOfWork.Save();
			return RedirectToAction("Index");
		}
		return View(obj);
	}

	#region API CALLS
	[HttpGet]
	public IActionResult GetAll()
	{
		var productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
		
		return Json(new {data = productList});
	}

	[HttpDelete]
	public async Task<IActionResult> Delete(int? id)
	{
		var obj = _unitOfWork.Product.GetFirstOrDefault(e => e.Id == id);
		if (obj == null)
		{
			return Json(new { success = false, message = "Error while deleting" });
		}

		var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
		if (System.IO.File.Exists(oldImagePath))
		{
			System.IO.File.Delete(oldImagePath);
		}
		_unitOfWork.Product.Remove(obj);
		await _unitOfWork.Save();
		return Json(new { 
			success = true, 
			message = "Delete Successful" 
		});
	}
	#endregion
}
