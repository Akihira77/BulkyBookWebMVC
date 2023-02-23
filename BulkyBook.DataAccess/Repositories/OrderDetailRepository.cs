using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repositories.IRepository;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BulkyBook.DataAccess.Repositories;

public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
{
	private readonly ApplicationDbContext _db;

	public OrderDetailRepository(ApplicationDbContext db) : base(db)
	{
		_db = db;
	}


	public void Update(OrderDetail obj)
	{
		_db.OrderDetails.Update(obj);
	}
}
