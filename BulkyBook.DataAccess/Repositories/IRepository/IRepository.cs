using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Data;

public interface IRepository<T> where T : class
{
	// T - Category
	T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null, bool noTrack = false);
	//T GetFirstOrDefaultNoTrack(Expression<Func<T, bool>> filter);
	IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
	void Add(T entity);
	void Remove(T entity);	
	void RemoveRange(IEnumerable<T> entity);
}
