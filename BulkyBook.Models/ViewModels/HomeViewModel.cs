using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models.ViewModels;

public class HomeViewModel
{
	public IEnumerable<Product> Products { get; set; }
	public IEnumerable<Category> Categories { get; set; }
	public string BookName { get; set; } = string.Empty;
	public string? CategoryFilter { get; set; } = null;
}
