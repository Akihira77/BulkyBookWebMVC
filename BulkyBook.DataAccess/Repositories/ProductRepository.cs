using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repositories.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
	private readonly ApplicationDbContext _db;

	public ProductRepository(ApplicationDbContext db) : base(db)
	{
		_db = db;
	}

	public void Update(Product obj)
	{
		var objFromDb = _db.Products.FirstOrDefault(p => obj.Id == p.Id);
		if (objFromDb != null)
		{
			objFromDb.Title = obj.Title;
			objFromDb.ISBN = obj.ISBN;
			objFromDb.Description = obj.Description;
			objFromDb.CategoryId = obj.CategoryId;
			objFromDb.CoverTypeId = obj.CoverTypeId;
			objFromDb.ListPrice = obj.ListPrice;
			objFromDb.Price = obj.Price;
			objFromDb.Price50 = obj.Price50;
			objFromDb.Price100 = obj.Price100;
			objFromDb.Author = obj.Author;
			if (obj.ImageUrl != null)
			{
				objFromDb.ImageUrl = obj.ImageUrl;
			}
		}
	}
}
