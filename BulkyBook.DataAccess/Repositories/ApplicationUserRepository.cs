using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repositories.IRepository;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BulkyBook.DataAccess.Repositories;

public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
{
	private readonly ApplicationDbContext _db;

	public ApplicationUserRepository(ApplicationDbContext db) : base(db)
	{
		_db = db;
	}
}
