using BulkyBook.Models;
using System.Linq.Expressions;

namespace BulkyBook.DataAccess.Data;

public interface ICategoryRepository : IRepository<Category>
{
	void Update(Category obj);
}
