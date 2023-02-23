using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers;
[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]

public class CompanyController : Controller
{
	private readonly IUnitOfWork _unitOfWork;

	public CompanyController(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public IActionResult Index()
	{
		return View();
	}

	public IActionResult Upsert(int? id)
	{
		if (id == 0 || id == null)
		{
			return View();
		}
		var obj = _unitOfWork.Company.GetFirstOrDefault(c => c.Id == id);
		return View(obj);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Upsert(Company obj) // update and insert
	{

		if (!ModelState.IsValid)
		{
			ModelState.AddModelError("Error", "Something wrong, Check your data");
			return View(obj);
		}

		if (obj.Id == 0)
		{
			_unitOfWork.Company.Add(obj);
			TempData["success"] = "Company created succesfully";
		} else
		{
			_unitOfWork.Company.Update(obj);
			TempData["success"] = "Company edited succesfully";
		}
		await _unitOfWork.Save();
		return RedirectToAction("Index");
	}

	public async Task<IActionResult> Delete(int id)
	{
		var obj = _unitOfWork.Company.GetFirstOrDefault(e => e.Id == id);
		if (obj == null)
		{
			return Json(new
			{
				success = false,
				message = "Error while deleting"
			});
		}
		_unitOfWork.Company.Remove(obj);
		await _unitOfWork.Save();
		return Json(new { 
			success = true, 
			message = "Delete Successful" 
		});
	}

	//[HttpGet]
	public IActionResult GetAll()
	{
		var companyList = _unitOfWork.Company.GetAll();

		return Json(new { data = companyList });
	}

}
