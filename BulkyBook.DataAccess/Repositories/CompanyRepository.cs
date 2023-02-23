using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repositories.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repositories;

public class CompanyRepository : Repository<Company>, ICompanyRepository
{
	private readonly ApplicationDbContext _db;

	public CompanyRepository(ApplicationDbContext db) : base(db)
	{
		_db = db;
	}

	public void Update(Company obj)
	{
		_db.Companies.Update(obj);
	}
}
