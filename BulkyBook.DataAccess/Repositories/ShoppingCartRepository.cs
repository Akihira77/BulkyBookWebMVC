using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repositories.IRepository;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BulkyBook.DataAccess.Repositories;

public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
{
	private readonly ApplicationDbContext _db;

	public ShoppingCartRepository(ApplicationDbContext db) : base(db)
	{
		_db = db;
	}

	public int DecrementCount(ShoppingCart shoppingCart, int count)
	{
		shoppingCart.Count -= count;
		return shoppingCart.Count;
	}

	public int IncrementCount(ShoppingCart shoppingCart, int count)
	{
		shoppingCart.Count += count;
		return shoppingCart.Count;
	}
}
