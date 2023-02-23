using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers;
[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]

public class CoverTypeController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public CoverTypeController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public IActionResult Index()
    {
        var obj = _unitOfWork.CoverType.GetAll();
        return View(obj);
    }
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CoverType obj)
    {

        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("Error", "Something wrong, Check your data");
            return View(obj);
        }

        var isNameAlreadyTaken = _unitOfWork.CoverType.GetFirstOrDefault(e => e.Name == obj.Name);

        if (isNameAlreadyTaken != null)
        {
            ModelState.AddModelError("name", "Your Name Cover Type is already taken");
            return View(obj);
        }
        _unitOfWork.CoverType.Add(obj);
        await _unitOfWork.Save();
        TempData["success"] = "Cover Type created succesfully";
        return RedirectToAction("Index");
    }
    public IActionResult Edit(int id)
    {
        var obj = _unitOfWork.CoverType.GetFirstOrDefault(e => e.Id == id);
        if (obj == null)
        {
            TempData["unsuccess"] = "The data is not in our server";
            return RedirectToAction("Index");
        }
        return View(obj);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CoverType obj)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("Error", "Something wrong, Check your data");
            return View(obj);
        }

        var isNameAlreadyTaken = _unitOfWork.CoverType.GetFirstOrDefault(e => e.Name == obj.Name, noTrack: true);

        if (isNameAlreadyTaken != null && isNameAlreadyTaken.Id != obj.Id)
        {
            ModelState.AddModelError("name", "Your Name Cover Type is already taken");
            return View(obj);
        }
        _unitOfWork.CoverType.Update(obj);
        await _unitOfWork.Save();
        TempData["success"] = "Cover Type edited succesfully";
        return RedirectToAction("Index");
    }

    public IActionResult Delete(int id)
    {
        var obj = _unitOfWork.CoverType.GetFirstOrDefault(e => e.Id == id);
        if (obj == null)
        {
            TempData["unsuccess"] = "The data is not in our server";
            return RedirectToAction("Index");
        }
        return View(obj);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(CoverType obj)
    {
        _unitOfWork.CoverType.Remove(obj);
        await _unitOfWork.Save();
        TempData["success"] = "Cover Type deleted succesfully";
        return RedirectToAction("Index");
    }
}
