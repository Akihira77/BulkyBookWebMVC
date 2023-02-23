using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
	private readonly ApplicationDbContext _db;
	internal DbSet<T> dbSet;

	public Repository(ApplicationDbContext db)
	{
		_db = db;
		//_db.Products.Include(p => p.Category).Include(p => p.CoverType);
		this.dbSet = _db.Set<T>();
	}

	public void Add(T entity)
	{
		dbSet.Add(entity);
	}

	public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
	{
		IQueryable<T> query = dbSet;
		if (filter != null)
		{
			query = query.Where(filter);
		}
		if (includeProperties != null)
		{
			foreach (var property in includeProperties.Split(",", StringSplitOptions.RemoveEmptyEntries))
			{
				query = query.Include(property);
			}
		}
		return query.ToList();
	}

	public T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null, bool noTrack = false)
	{
		IQueryable<T> query;

		//if (noTrack)
		//{
		//	query = dbSet.AsNoTracking();
		//} else
		//{
		//	query = dbSet;
		//}
		query = noTrack ? dbSet.AsNoTracking() : dbSet;

		query = query.Where(filter);
		if (includeProperties != null)
		{
			foreach (var property in includeProperties.Split(",", StringSplitOptions.RemoveEmptyEntries))
			{
				query = query.Include(property);
			}
		}
		return query.FirstOrDefault();
	}
	//public T GetFirstOrDefaultNoTrack(Expression<Func<T, bool>> filter)
	//{
	//	IQueryable<T> query = dbSet;

	//	//query = query.Where(filter).AsNoTracking();
	//	return query.AsNoTracking().FirstOrDefault(filter);
	//}

	public void Remove(T entity)
	{
		dbSet.Remove(entity);
	}

	public void RemoveRange(IEnumerable<T> entity)
	{
		dbSet.RemoveRange(entity);
	}
}
