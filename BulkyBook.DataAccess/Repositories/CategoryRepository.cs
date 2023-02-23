using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repositories.IRepository;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BulkyBook.DataAccess.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
	private readonly ApplicationDbContext _db;

	public CategoryRepository(ApplicationDbContext db) : base(db)
	{
		_db = db;
	}

	public void Update(Category obj)
	{
		_db.Categories.Update(obj);
	}
}
