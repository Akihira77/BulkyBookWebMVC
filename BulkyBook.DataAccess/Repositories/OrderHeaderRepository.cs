using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repositories.IRepository;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BulkyBook.DataAccess.Repositories;

public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
{
	private readonly ApplicationDbContext _db;

	public OrderHeaderRepository(ApplicationDbContext db) : base(db)
	{
		_db = db;
	}

	public void Update(OrderHeader obj)
	{
		_db.OrderHeaders.Update(obj);
	}

	public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
	{
		var orderFromDb = _db.OrderHeaders.FirstOrDefault(oh => oh.Id == id);

		if (orderFromDb != null)
		{
			orderFromDb.OrderStatus = orderStatus;
			if (paymentStatus != null)
			{
				orderFromDb.PaymentStatus = paymentStatus;
			}
		}
	}
	public void UpdateStripePaymentId(int id, string? sessionId = null, string? paymentIntentId = null)
	{
		var orderFromDb = _db.OrderHeaders.FirstOrDefault(oh => oh.Id == id);

		orderFromDb.PaymentDate = DateTime.Now;
		if (sessionId != null)
		{
			orderFromDb.SessionId = sessionId;
		}
		if (paymentIntentId != null)
		{
			orderFromDb.PaymentIntentId = paymentIntentId;
		}
	}
}
