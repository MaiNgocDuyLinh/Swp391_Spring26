using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Models.TempShopModels;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Shopping;
using Microsoft.EntityFrameworkCore;

namespace Group3_SWP391_PetMedical.Repository.Implementations
{
    public class CusShoppingRepository : ICusShoppingRepository
    {
        private readonly PetClinicContext _context;

        public CusShoppingRepository(PetClinicContext context)
        {
            _context = context;
        }

        public async Task<List<CusShoppingCategoryVM>> GetCategoriesAsync()
        {
            return await _context.ProductCategories
                .AsNoTracking()
                .Where(x => x.Status)
                .OrderBy(x => x.CategoryName)
                .Select(x => new CusShoppingCategoryVM
                {
                    CategoryId = x.CategoryId,
                    CategoryName = x.CategoryName
                })
                .ToListAsync();
        }

        public async Task<PagedResult<CusShoppingProductCardVM>> GetProductsAsync(CusShoppingQuery query)
        {
            query ??= new CusShoppingQuery();

            var q =
                from p in _context.Products.AsNoTracking()
                join c in _context.ProductCategories.AsNoTracking()
                    on p.CategoryId equals c.CategoryId
                select new CusShoppingProductCardVM
                {
                    ProductId = p.ProductId,
                    CategoryId = p.CategoryId,
                    Name = p.Name,
                    CategoryName = c.CategoryName,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    Status = p.Status,
                    ImageUrl = p.ImageUrl
                };

            if (query.CategoryId.HasValue)
            {
                q = q.Where(x => x.CategoryId == query.CategoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                var keyword = query.Keyword.Trim();
                q = q.Where(x => x.Name.Contains(keyword) || x.CategoryName.Contains(keyword));
            }

            var totalItems = await q.CountAsync();

            var page = query.Page <= 0 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 9 : query.PageSize;

            var items = await q
                .OrderBy(x =>
                    x.Status == "Đang bán" && x.StockQuantity > 0 ? 0 :
                    x.Status == "Hết hàng" ? 1 :
                    x.Status == "Dừng bán" ? 2 : 3)
                .ThenByDescending(x => x.ProductId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<CusShoppingProductCardVM>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        public async Task<CusShoppingDetailVM?> GetProductDetailAsync(int productId)
        {
            var product =
                await (from p in _context.Products.AsNoTracking()
                       join c in _context.ProductCategories.AsNoTracking()
                           on p.CategoryId equals c.CategoryId
                       where p.ProductId == productId
                       select new CusShoppingDetailVM
                       {
                           ProductId = p.ProductId,
                           CategoryId = p.CategoryId,
                           Name = p.Name,
                           CategoryName = c.CategoryName,
                           Description = p.Description,
                           Price = p.Price,
                           StockQuantity = p.StockQuantity,
                           Status = p.Status,
                           ImageUrl = p.ImageUrl
                       })
                       .FirstOrDefaultAsync();

            if (product == null) return null;

            product.Variants = await _context.ProductVariants
                .AsNoTracking()
                .Where(x => x.ProductId == productId)
                .OrderBy(x =>
                    x.Status == "Đang bán" && x.StockQuantity > 0 ? 0 :
                    x.Status == "Hết hàng" ? 1 :
                    x.Status == "Dừng bán" ? 2 : 3)
                .ThenBy(x => x.VariantName)
                .Select(x => new CusShoppingVariantVM
                {
                    VariantId = x.VariantId,
                    VariantName = x.VariantName,
                    Color = x.Color,
                    Size = x.Size,
                    Material = x.Material,
                    PriceOverride = x.PriceOverride,
                    StockQuantity = x.StockQuantity,
                    Status = x.Status
                })
                .ToListAsync();

            return product;
        }

        public async Task<CusCartVM> GetCartAsync(int customerId)
        {
            var cart = await _context.Carts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CustomerId == customerId && x.Status == "Đang hoạt động");

            if (cart == null)
            {
                return new CusCartVM();
            }

            var items =
                await (from ci in _context.CartItems.AsNoTracking()
                       join p in _context.Products.AsNoTracking()
                           on ci.ProductId equals p.ProductId
                       join pv in _context.ProductVariants.AsNoTracking()
                           on ci.VariantId equals pv.VariantId into pvJoin
                       from pv in pvJoin.DefaultIfEmpty()
                       where ci.CartId == cart.CartId
                       orderby ci.CartItemId descending
                       select new CusCartItemVM
                       {
                           CartItemId = ci.CartItemId,
                           ProductId = ci.ProductId,
                           VariantId = ci.VariantId,
                           ProductName = p.Name,
                           VariantName = pv != null ? pv.VariantName : null,
                           ImageUrl = pv != null && !string.IsNullOrEmpty(pv.ImageUrl) ? pv.ImageUrl : p.ImageUrl,
                           UnitPrice = ci.UnitPrice,
                           Quantity = ci.Quantity
                       })
                       .ToListAsync();

            return new CusCartVM
            {
                Items = items
            };
        }

        public async Task AddToCartAsync(int customerId, int productId, int? variantId, int quantity)
        {
            if (quantity <= 0)
                throw new Exception("Số lượng phải lớn hơn 0.");

            var product = await _context.Products
                .FirstOrDefaultAsync(x => x.ProductId == productId);

            if (product == null)
                throw new Exception("Sản phẩm không tồn tại.");

            if (product.Status != "Đang bán" || product.StockQuantity <= 0)
                throw new Exception("Sản phẩm hiện không thể thêm vào giỏ hàng.");

            decimal unitPrice = product.Price;
            int availableStock = product.StockQuantity;

            if (variantId.HasValue)
            {
                var variant = await _context.ProductVariants
                    .FirstOrDefaultAsync(x => x.VariantId == variantId.Value && x.ProductId == productId);

                if (variant == null)
                    throw new Exception("Phân loại không tồn tại.");

                if (variant.Status != "Đang bán" || variant.StockQuantity <= 0)
                    throw new Exception("Phân loại hiện không thể thêm vào giỏ hàng.");

                availableStock = variant.StockQuantity;
                unitPrice = variant.PriceOverride ?? product.Price;
            }

            var cart = await GetOrCreateCartAsync(customerId);

            var existing = await _context.CartItems.FirstOrDefaultAsync(x =>
                x.CartId == cart.CartId &&
                x.ProductId == productId &&
                x.VariantId == variantId);

            int newQuantity = quantity;
            if (existing != null)
            {
                newQuantity = existing.Quantity + quantity;
            }

            if (newQuantity > availableStock)
                throw new Exception("Số lượng vượt quá tồn kho.");

            if (existing != null)
            {
                existing.Quantity = newQuantity;
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = productId,
                    VariantId = variantId,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    CreatedAt = DateTime.Now
                });
            }

            cart.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCartItemAsync(int customerId, int cartItemId, int quantity)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(x => x.CustomerId == customerId && x.Status == "Đang hoạt động");

            if (cart == null)
                throw new Exception("Giỏ hàng không tồn tại.");

            var item = await _context.CartItems
                .FirstOrDefaultAsync(x => x.CartItemId == cartItemId && x.CartId == cart.CartId);

            if (item == null)
                throw new Exception("Không tìm thấy sản phẩm trong giỏ.");

            if (quantity <= 0)
            {
                _context.CartItems.Remove(item);
                cart.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return;
            }

            int availableStock;
            if (item.VariantId.HasValue)
            {
                var variant = await _context.ProductVariants
                    .FirstOrDefaultAsync(x => x.VariantId == item.VariantId.Value);

                if (variant == null || variant.Status != "Đang bán" || variant.StockQuantity <= 0)
                    throw new Exception("Phân loại hiện không khả dụng.");

                availableStock = variant.StockQuantity;
                item.UnitPrice = variant.PriceOverride ?? item.UnitPrice;
            }
            else
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(x => x.ProductId == item.ProductId);

                if (product == null || product.Status != "Đang bán" || product.StockQuantity <= 0)
                    throw new Exception("Sản phẩm hiện không khả dụng.");

                availableStock = product.StockQuantity;
                item.UnitPrice = product.Price;
            }

            if (quantity > availableStock)
                throw new Exception("Số lượng vượt quá tồn kho.");

            item.Quantity = quantity;
            cart.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task RemoveCartItemAsync(int customerId, int cartItemId)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(x => x.CustomerId == customerId && x.Status == "Đang hoạt động");

            if (cart == null) return;

            var item = await _context.CartItems
                .FirstOrDefaultAsync(x => x.CartItemId == cartItemId && x.CartId == cart.CartId);

            if (item == null) return;

            _context.CartItems.Remove(item);
            cart.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task<CusCheckoutVM> GetCheckoutAsync(int customerId, List<int> selectedCartItemIds)
        {
            var cart = await GetCartAsync(customerId);

            var selectedSet = selectedCartItemIds?
                .Distinct()
                .ToHashSet() ?? new HashSet<int>();

            var selectedItems = cart.Items
                .Where(x => selectedSet.Contains(x.CartItemId))
                .ToList();

            return new CusCheckoutVM
            {
                Items = selectedItems,
                PaymentMethod = "Thanh toán tại quầy",
                SelectedCartItemIds = selectedItems.Select(x => x.CartItemId).ToList()
            };
        }

        public async Task<int> PlaceOrderAsync(
            int customerId,
            List<int> selectedCartItemIds,
            string? pickupNote,
            DateTime? pickupDate,
            string? paymentMethod)
        {
            var selectedSet = selectedCartItemIds?
                .Distinct()
                .ToHashSet() ?? new HashSet<int>();

            if (!selectedSet.Any())
                throw new Exception("Vui lòng chọn ít nhất 1 sản phẩm.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            var cart = await GetOrCreateCartAsync(customerId);

            var cartItems =
                await (from ci in _context.CartItems
                       join p in _context.Products
                           on ci.ProductId equals p.ProductId
                       join pv in _context.ProductVariants
                           on ci.VariantId equals pv.VariantId into pvJoin
                       from pv in pvJoin.DefaultIfEmpty()
                       where ci.CartId == cart.CartId && selectedSet.Contains(ci.CartItemId)
                       select new
                       {
                           CartItem = ci,
                           Product = p,
                           Variant = pv
                       })
                       .ToListAsync();

            if (!cartItems.Any())
                throw new Exception("Không tìm thấy sản phẩm đã chọn trong giỏ hàng.");

            foreach (var x in cartItems)
            {
                if (x.Product.Status != "Đang bán" || x.Product.StockQuantity <= 0)
                    throw new Exception($"Sản phẩm '{x.Product.Name}' hiện không thể mua.");

                if (x.CartItem.VariantId.HasValue)
                {
                    if (x.Variant == null || x.Variant.Status != "Đang bán" || x.Variant.StockQuantity <= 0)
                        throw new Exception($"Phân loại của '{x.Product.Name}' hiện không thể mua.");

                    if (x.CartItem.Quantity > x.Variant.StockQuantity)
                        throw new Exception($"Sản phẩm '{x.Product.Name}' không đủ trong kho.");
                }
                else
                {
                    if (x.Product.StockQuantity <= 0)
                        throw new Exception($"Sản phẩm '{x.Product.Name}' hiện không thể mua.");

                    if (x.CartItem.Quantity > x.Product.StockQuantity)
                        throw new Exception($"Sản phẩm '{x.Product.Name}' không đủ trong kho.");
                }
            }

            var order = new ProductOrder
            {
                CustomerId = customerId,
                OrderCode = $"PO{DateTime.Now:yyyyMMddHHmmssfff}",
                TotalAmount = cartItems.Sum(x => x.CartItem.UnitPrice * x.CartItem.Quantity),
                PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? "Thanh toán tại quầy" : paymentMethod,
                PaymentStatus = "Chưa thanh toán",
                OrderStatus = "Chờ xác nhận",
                PickupMethod = "Nhận tại phòng khám",
                PickupNote = pickupNote,
                PickupDate = pickupDate?.Date,
                CreatedAt = DateTime.Now
            };

            _context.ProductOrders.Add(order);
            await _context.SaveChangesAsync();

            var orderItems = new List<ProductOrderItem>();

            foreach (var x in cartItems)
            {
                orderItems.Add(new ProductOrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = x.Product.ProductId,
                    VariantId = x.CartItem.VariantId,
                    ProductName = x.Product.Name,
                    VariantName = x.Variant != null ? x.Variant.VariantName : null,
                    UnitPrice = x.CartItem.UnitPrice,
                    Quantity = x.CartItem.Quantity,
                    LineTotal = x.CartItem.UnitPrice * x.CartItem.Quantity
                });

                if (x.CartItem.VariantId.HasValue && x.Variant != null)
                {
                    x.Variant.StockQuantity -= x.CartItem.Quantity;
                    if (x.Variant.StockQuantity <= 0)
                    {
                        x.Variant.StockQuantity = 0;
                        x.Variant.Status = "Hết hàng";
                    }
                }
                else
                {
                    x.Product.StockQuantity -= x.CartItem.Quantity;
                    if (x.Product.StockQuantity <= 0)
                    {
                        x.Product.StockQuantity = 0;
                        x.Product.Status = "Hết hàng";
                    }
                }
            }

            var hasRemainingItems = await _context.CartItems
                .AnyAsync(x => x.CartId == cart.CartId && !selectedSet.Contains(x.CartItemId));

            _context.ProductOrderItems.AddRange(orderItems);
            _context.CartItems.RemoveRange(cartItems.Select(x => x.CartItem));

            cart.Status = hasRemainingItems ? "Đang hoạt động" : "Đã đặt hàng";
            cart.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return order.OrderId;
        }

        public async Task<CusShoppingMyOrdersVM> GetMyOrdersAsync(int customerId, CusShoppingOrderQuery query)
        {
            query ??= new CusShoppingOrderQuery();

            var page = query.Page <= 0 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 6 : query.PageSize;
            if (pageSize > 50) pageSize = 50;

            var keyword = query.Q?.Trim();

            var q = _context.ProductOrders
                .AsNoTracking()
                .Where(x => x.CustomerId == customerId);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                q = q.Where(x =>
                    (x.OrderCode != null && x.OrderCode.Contains(keyword)) ||
                    (x.OrderStatus != null && x.OrderStatus.Contains(keyword)) ||
                    (x.PaymentStatus != null && x.PaymentStatus.Contains(keyword)) ||
                    (x.PaymentMethod != null && x.PaymentMethod.Contains(keyword)));
            }

            var totalItems = await q.CountAsync();
            var totalAmount = await q.SumAsync(x => (decimal?)x.TotalAmount) ?? 0m;

            var items = await q
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.OrderId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CusShoppingOrderListItemVM
                {
                    OrderId = x.OrderId,
                    OrderCode = x.OrderCode,
                    TotalAmount = x.TotalAmount,
                    PaymentStatus = x.PaymentStatus,
                    OrderStatus = x.OrderStatus,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            return new CusShoppingMyOrdersVM
            {
                Filter = new CusShoppingOrderQuery
                {
                    Q = query.Q,
                    Page = page,
                    PageSize = pageSize
                },
                FilteredTotalAmount = totalAmount,
                Page = new PagedResult<CusShoppingOrderListItemVM>
                {
                    Items = items,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems
                }
            };
        }

        public async Task<CusShoppingOrderDetailVM?> GetOrderDetailAsync(int customerId, int orderId)
        {
            var order = await _context.ProductOrders
                .AsNoTracking()
                .Where(x => x.CustomerId == customerId && x.OrderId == orderId)
                .Select(x => new CusShoppingOrderDetailVM
                {
                    OrderId = x.OrderId,
                    OrderCode = x.OrderCode,
                    PaymentMethod = x.PaymentMethod,
                    PaymentStatus = x.PaymentStatus,
                    OrderStatus = x.OrderStatus,
                    PickupMethod = x.PickupMethod,
                    PickupNote = x.PickupNote,
                    PickupDate = x.PickupDate,
                    CreatedAt = x.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (order == null) return null;

            order.Items = await _context.ProductOrderItems
                .AsNoTracking()
                .Where(x => x.OrderId == orderId)
                .Select(x => new CusShoppingOrderDetailItemVM
                {
                    ProductName = x.ProductName,
                    VariantName = x.VariantName,
                    UnitPrice = x.UnitPrice,
                    Quantity = x.Quantity,
                    LineTotal = x.LineTotal
                })
                .ToListAsync();

            return order;
        }

        private async Task<Cart> GetOrCreateCartAsync(int customerId)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(x => x.CustomerId == customerId && x.Status == "Đang hoạt động");

            if (cart != null) return cart;

            cart = new Cart
            {
                CustomerId = customerId,
                Status = "Đang hoạt động",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            return cart;
        }
    }
}