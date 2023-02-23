using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repositories.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repositories;

public class UnitOfWork : IUnitOfWork
{
	private readonly ApplicationDbContext _db;

	public UnitOfWork(ApplicationDbContext db)
	{
		_db = db;
		Category = new CategoryRepository(_db);
		CoverType = new CoverTypeRepository(_db);
		Product = new ProductRepository(_db);
		Company = new CompanyRepository(_db);
		ShoppingCart = new ShoppingCartRepository(_db);
		ApplicationUser = new ApplicationUserRepository(_db);
		OrderHeader = new OrderHeaderRepository(_db);
		OrderDetail = new OrderDetailRepository(_db);
	}
	public ICategoryRepository Category { get; private set; }
	public ICoverTypeRepository CoverType { get; private set; }
	public IProductRepository Product { get; private set; }
	public ICompanyRepository Company { get; private set; }
	public IShoppingCartRepository ShoppingCart { get; private set; }
	public IApplicationUserRepository ApplicationUser { get; private set; }
	public IOrderHeaderRepository OrderHeader { get; set; }
	public IOrderDetailRepository OrderDetail { get; set; }

	public async Task Save()
	{
		await _db.SaveChangesAsync();
	}
}
