using Group3_SWP391_PetMedical.Models.Common;
using System.Collections.Generic;

namespace Group3_SWP391_PetMedical.ViewModels.Shopping
{
    public class CusShoppingIndexVM
    {
        public CusShoppingQuery Query { get; set; } = new();
        public PagedResult<CusShoppingProductCardVM> Result { get; set; } = new();
        public List<CusShoppingCategoryVM> Categories { get; set; } = new();
    }

    public class CusShoppingQuery
    {
        public string? Keyword { get; set; }
        public int? CategoryId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 9;
    }

    public class CusShoppingCategoryVM
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
    }

    public class CusShoppingProductCardVM
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Status { get; set; } = "";
        public string? ImageUrl { get; set; }
    }
}