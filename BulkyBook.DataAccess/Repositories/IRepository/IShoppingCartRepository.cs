using BulkyBook.Models;
using System.Linq.Expressions;

namespace BulkyBook.DataAccess.Data;

public interface IShoppingCartRepository : IRepository<ShoppingCart>
{
	int IncrementCount(ShoppingCart shoppingCart, int count);
	int DecrementCount(ShoppingCart shoppingCart, int count);
}