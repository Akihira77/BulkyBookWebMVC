using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BulkyBookWeb.Areas.Admin.Controllers;
[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]
public class CategoryController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public IActionResult Index(string? sort = null)
    {
        IEnumerable<Category> obj;
        if (sort != null)
        {
            obj = _unitOfWork.Category.GetAll().OrderBy(c => c.DisplayOrder);
            if (sort == "dsc")
            {
                obj = obj.Reverse();
            }
        } else
        {
            obj = _unitOfWork.Category.GetAll();
        }
        return View(obj);
    }
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category obj)
    {

        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("Error", "Something wrong, Check your data");
            return View(obj);
        }

        var isNameAlreadyTaken = _unitOfWork.Category.GetFirstOrDefault(e => e.Name == obj.Name);

        if (isNameAlreadyTaken != null)
        {
            ModelState.AddModelError("name", "Your Name category is already taken");
            return View(obj);
        }
        _unitOfWork.Category.Add(obj);
        await _unitOfWork.Save();
        TempData["success"] = "Category created succesfully";
        return RedirectToAction("Index");
    }

    public IActionResult Edit(int id)
    {
        var obj = _unitOfWork.Category.GetFirstOrDefault(e => e.Id == id);
        if (obj == null)
        {
            TempData["unsuccess"] = "The data is not in our server";
            return RedirectToAction("Index");
        }
        return View(obj);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Category obj)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("Error", "Something wrong, Check your data");
            return View(obj);
        }

        var isNameAlreadyTaken = _unitOfWork.Category.GetFirstOrDefault(e => e.Name == obj.Name, noTrack: true);

        if (isNameAlreadyTaken != null && isNameAlreadyTaken.Id != obj.Id)
        {
            ModelState.AddModelError("name", "Your Name category is already taken");
            return View(obj);
        }
        _unitOfWork.Category.Update(obj);
        await _unitOfWork.Save();
        TempData["success"] = "Category edited succesfully";
        return RedirectToAction("Index");
    }

    public IActionResult Delete(int id)
    {
        var obj = _unitOfWork.Category.GetFirstOrDefault(e => e.Id == id);
        if (obj == null)
        {
            TempData["unsuccess"] = "The data is not in our server";
            return RedirectToAction("Index");
        }
        return View(obj);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Category obj)
    {
        _unitOfWork.Category.Remove(obj);
        await _unitOfWork.Save();
        TempData["success"] = "Category deleted succesfully";
        return RedirectToAction("Index");
    }
}
