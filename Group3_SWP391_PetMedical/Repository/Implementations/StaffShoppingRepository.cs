using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Models.TempShopModels;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Shopping;
using Microsoft.EntityFrameworkCore;

namespace Group3_SWP391_PetMedical.Repository.Implementations
{
    public class StaffShoppingRepository : IStaffShoppingRepository
    {
        private readonly PetClinicContext _context;

        private const string InternalPendingOrderStatus = "Chờ xác nhận";
        private const string InternalReceivedOrderStatus = "Hoàn thành";
        private const string InternalCancelledOrderStatus = "Đã hủy";

        private static readonly string[] PendingOrderStatuses =
        {
            "Đặt hàng thành công",
            "Chờ xác nhận",
            "Đã xác nhận",
            "Sẵn sàng nhận hàng",
            "Đang xử lý",
            "Đang giao"
        };

        private static readonly string[] ReceivedOrderStatuses =
        {
            "Đã nhận hàng",
            "Đã giao",
            "Hoàn thành"
        };

        private static readonly string[] CancelledOrderStatuses =
        {
            "Đã hủy",
            "Hủy"
        };

        private static readonly string[] PaidStatuses =
        {
            "Đã thanh toán",
            "Thanh toán thành công"
        };

        public StaffShoppingRepository(PetClinicContext context)
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

        public async Task<PagedResult<StaffShoppingProductRowVM>> GetProductsAsync(StaffShoppingQuery query)
        {
            query ??= new StaffShoppingQuery();

            var q =
                from p in _context.Products.AsNoTracking()
                join c in _context.ProductCategories.AsNoTracking()
                    on p.CategoryId equals c.CategoryId
                select new StaffShoppingProductRowVM
                {
                    ProductId = p.ProductId,
                    CategoryId = p.CategoryId,
                    CategoryName = c.CategoryName,
                    Name = p.Name,
                    SKU = p.Sku,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    Status = p.Status,
                    ImageUrl = p.ImageUrl,
                    VariantCount = _context.ProductVariants.Count(v => v.ProductId == p.ProductId),
                    CreatedAt = p.CreatedAt
                };

            if (query.CategoryId.HasValue)
                q = q.Where(x => x.CategoryId == query.CategoryId.Value);

            if (!string.IsNullOrWhiteSpace(query.Status))
                q = q.Where(x => x.Status == query.Status);

            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                var keyword = query.Keyword.Trim();
                q = q.Where(x =>
                    x.Name.Contains(keyword) ||
                    x.SKU.Contains(keyword) ||
                    x.CategoryName.Contains(keyword));
            }

            var page = query.Page <= 0 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 9 : query.PageSize;
            if (pageSize > 50) pageSize = 50;

            var totalItems = await q.CountAsync();

            var items = await q
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.ProductId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<StaffShoppingProductRowVM>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        public async Task<StaffShoppingUpsertVM?> GetProductForEditAsync(int productId)
        {
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProductId == productId);

            if (product == null) return null;

            var vm = new StaffShoppingUpsertVM
            {
                ProductId = product.ProductId,
                CategoryId = product.CategoryId,
                Name = product.Name,
                SKU = product.Sku,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Status = product.Status,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                Categories = await GetCategoriesAsync(),
                Variants = await _context.ProductVariants
                    .AsNoTracking()
                    .Where(x => x.ProductId == productId)
                    .OrderBy(x => x.VariantId)
                    .Select(x => new StaffShoppingVariantInputVM
                    {
                        VariantId = x.VariantId,
                        VariantName = x.VariantName,
                        Color = x.Color,
                        Size = x.Size,
                        Material = x.Material,
                        SKU = x.Sku,
                        PriceOverride = x.PriceOverride,
                        StockQuantity = x.StockQuantity,
                        Status = x.Status,
                        ImageUrl = x.ImageUrl
                    })
                    .ToListAsync()
            };

            return vm;
        }

        public async Task<int> CreateProductAsync(StaffShoppingUpsertVM vm)
        {
            if (await _context.Products.AnyAsync(x => x.Sku == vm.SKU))
                throw new Exception("Mã sản phẩm đã tồn tại.");

            var entity = new Product
            {
                CategoryId = vm.CategoryId,
                Name = vm.Name.Trim(),
                Sku = vm.SKU.Trim(),
                Price = vm.Price,
                StockQuantity = vm.StockQuantity,
                Status = vm.Status,
                Description = vm.Description,
                ImageUrl = vm.ImageUrl,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Products.Add(entity);
            await _context.SaveChangesAsync();

            await SaveVariantsAsync(entity.ProductId, vm.Variants);
            await _context.SaveChangesAsync();

            return entity.ProductId;
        }

        public async Task UpdateProductAsync(StaffShoppingUpsertVM vm)
        {
            if (!vm.ProductId.HasValue)
                throw new Exception("Thiếu mã sản phẩm.");

            var entity = await _context.Products
                .FirstOrDefaultAsync(x => x.ProductId == vm.ProductId.Value);

            if (entity == null)
                throw new Exception("Sản phẩm không tồn tại.");

            var duplicateSku = await _context.Products
                .AnyAsync(x => x.ProductId != entity.ProductId && x.Sku == vm.SKU);

            if (duplicateSku)
                throw new Exception("SKU sản phẩm đã tồn tại.");

            entity.CategoryId = vm.CategoryId;
            entity.Name = vm.Name.Trim();
            entity.Sku = vm.SKU.Trim();
            entity.Price = vm.Price;
            entity.StockQuantity = vm.StockQuantity;
            entity.Status = vm.Status;
            entity.Description = vm.Description;
            entity.ImageUrl = vm.ImageUrl;
            entity.UpdatedAt = DateTime.Now;

            await SaveVariantsAsync(entity.ProductId, vm.Variants);
            await _context.SaveChangesAsync();
        }

        public async Task StopSellingProductAsync(int productId)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(x => x.ProductId == productId);

            if (product == null)
                throw new Exception("Sản phẩm không tồn tại.");

            product.Status = "Dừng bán";
            product.UpdatedAt = DateTime.Now;

            var variants = await _context.ProductVariants
                .Where(x => x.ProductId == productId)
                .ToListAsync();

            foreach (var variant in variants)
            {
                variant.Status = "Dừng bán";
                variant.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<StaffShoppingOrderRowVM>> GetOrdersAsync(StaffShoppingOrderQuery query)
        {
            query ??= new StaffShoppingOrderQuery();

            var q =
                from o in _context.ProductOrders.AsNoTracking()
                join u in _context.Users.AsNoTracking()
                    on o.CustomerId equals u.user_id into uJoin
                from u in uJoin.DefaultIfEmpty()
                select new StaffShoppingOrderRowVM
                {
                    OrderId = o.OrderId,
                    OrderCode = o.OrderCode,
                    CustomerId = o.CustomerId,
                    CustomerName = u != null ? u.full_name : null,
                    TotalAmount = o.TotalAmount,
                    PaymentStatus = o.PaymentStatus,
                    OrderStatus = o.OrderStatus,
                    PickupMethod = o.PickupMethod,
                    PickupDate = o.PickupDate,
                    CreatedAt = o.CreatedAt
                };

            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                var keyword = query.Keyword.Trim();
                q = q.Where(x =>
                    x.OrderCode.Contains(keyword) ||
                    (x.CustomerName != null && x.CustomerName.Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(query.OrderStatus))
            {
                var filterOrderStatus = NormalizeOrderStatus(query.OrderStatus);

                if (filterOrderStatus == "Đã hủy")
                {
                    q = q.Where(x => CancelledOrderStatuses.Contains(x.OrderStatus));
                }
                else if (filterOrderStatus == "Đã nhận hàng")
                {
                    q = q.Where(x => ReceivedOrderStatuses.Contains(x.OrderStatus));
                }
                else
                {
                    q = q.Where(x => PendingOrderStatuses.Contains(x.OrderStatus));
                }
            }

            if (!string.IsNullOrWhiteSpace(query.PaymentStatus))
            {
                var filterPaymentStatus = NormalizePaymentStatus(query.PaymentStatus);

                if (filterPaymentStatus == "Đã thanh toán")
                {
                    q = q.Where(x => PaidStatuses.Contains(x.PaymentStatus));
                }
                else
                {
                    q = q.Where(x => !PaidStatuses.Contains(x.PaymentStatus));
                }
            }

            var page = query.Page <= 0 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 8 : query.PageSize;
            if (pageSize > 50) pageSize = 50;

            var totalItems = await q.CountAsync();

            var rawItems = await q
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.OrderId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = rawItems
                .Select(x =>
                {
                    x.OrderStatus = NormalizeOrderStatus(x.OrderStatus);
                    x.PaymentStatus = NormalizePaymentStatus(x.PaymentStatus);
                    return x;
                })
                .ToList();

            return new PagedResult<StaffShoppingOrderRowVM>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        public async Task<StaffShoppingOrderDetailVM?> GetOrderDetailAsync(int orderId)
        {
            var order =
                await (from o in _context.ProductOrders.AsNoTracking()
                       join u in _context.Users.AsNoTracking()
                           on o.CustomerId equals u.user_id into uJoin
                       from u in uJoin.DefaultIfEmpty()
                       where o.OrderId == orderId
                       select new StaffShoppingOrderDetailVM
                       {
                           OrderId = o.OrderId,
                           CustomerId = o.CustomerId,
                           OrderCode = o.OrderCode,
                           CustomerName = u != null ? u.full_name : null,
                           TotalAmount = o.TotalAmount,
                           PaymentMethod = o.PaymentMethod,
                           PaymentStatus = o.PaymentStatus,
                           OrderStatus = o.OrderStatus,
                           PickupMethod = o.PickupMethod,
                           PickupNote = o.PickupNote,
                           PickupDate = o.PickupDate,
                           CreatedAt = o.CreatedAt
                       }).FirstOrDefaultAsync();

            if (order == null) return null;

            order.Items = await _context.ProductOrderItems
                .AsNoTracking()
                .Where(x => x.OrderId == orderId)
                .Select(x => new StaffShoppingOrderItemVM
                {
                    ProductName = x.ProductName,
                    VariantName = x.VariantName,
                    UnitPrice = x.UnitPrice,
                    Quantity = x.Quantity,
                    LineTotal = x.LineTotal
                })
                .ToListAsync();

            order.OrderStatus = NormalizeOrderStatus(order.OrderStatus);
            order.PaymentStatus = NormalizePaymentStatus(order.PaymentStatus);
            order.IsCancelled = order.OrderStatus == "Đã hủy";

            if (order.IsCancelled)
            {
                order.IsStatusUpdateLocked = true;
                order.StatusUpdateLockMessage = "Đơn hàng này đã hủy. Trạng thái hiện tại chỉ được xem, không thể cập nhật thêm.";
            }
            else if (order.OrderStatus == "Đã nhận hàng" && order.PaymentStatus == "Đã thanh toán")
            {
                order.IsStatusUpdateLocked = true;
                order.StatusUpdateLockMessage = "Đơn hàng đã hoàn tất (đã nhận hàng và đã thanh toán), không thể cập nhật thêm.";
            }

            return order;
        }

        public async Task UpdateOrderStatusAsync(StaffShoppingUpdateOrderStatusVM vm)
        {
            var order = await _context.ProductOrders
                .FirstOrDefaultAsync(x => x.OrderId == vm.OrderId);

            if (order == null)
                throw new Exception("Đơn hàng không tồn tại.");

            var currentDisplayOrderStatus = NormalizeOrderStatus(order.OrderStatus);
            var currentDisplayPaymentStatus = NormalizePaymentStatus(order.PaymentStatus);

            var newDisplayOrderStatus = NormalizeOrderStatus(vm.OrderStatus);
            var newDisplayPaymentStatus = NormalizePaymentStatus(vm.PaymentStatus);

            if (currentDisplayOrderStatus == "Đã hủy")
                throw new Exception("Đơn hàng đã hủy, không thể cập nhật thêm.");

            if (currentDisplayOrderStatus == "Đã nhận hàng" && currentDisplayPaymentStatus == "Đã thanh toán")
                throw new Exception("Đơn hàng đã hoàn tất (đã nhận hàng và đã thanh toán), không thể cập nhật thêm.");

            if (currentDisplayOrderStatus == "Đã nhận hàng" && newDisplayOrderStatus == "Đặt hàng thành công")
                throw new Exception("Đơn hàng đã nhận hàng, không thể chuyển về trạng thái trước đó.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (newDisplayOrderStatus == "Đã hủy")
                {
                    if (currentDisplayOrderStatus == "Đã nhận hàng")
                        throw new Exception("Đơn hàng đã nhận hàng, không thể hủy.");

                    await CancelOrderInternalAsync(order);
                }
                else if (newDisplayOrderStatus == "Đã nhận hàng")
                {
                    order.OrderStatus = InternalReceivedOrderStatus;
                }
                else
                {
                    order.OrderStatus = InternalPendingOrderStatus;
                }

                order.PaymentStatus = newDisplayPaymentStatus == "Đã thanh toán"
                    ? "Đã thanh toán"
                    : "Chưa thanh toán";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                throw new Exception("Không thể cập nhật trạng thái đơn hàng. Vui lòng kiểm tra lại dữ liệu trạng thái trong cơ sở dữ liệu.");
            }
        }

        public async Task<int> AutoCancelExpiredOrdersAsync()
        {
            var today = DateTime.Today;

            var expiredOrders = await _context.ProductOrders
                .Where(x =>
                    PendingOrderStatuses.Contains(x.OrderStatus) &&
                    x.PickupDate.HasValue &&
                    x.PickupDate.Value < today)
                .OrderBy(x => x.OrderId)
                .ToListAsync();

            if (!expiredOrders.Any())
                return 0;

            using var transaction = await _context.Database.BeginTransactionAsync();

            foreach (var order in expiredOrders)
            {
                await CancelOrderInternalAsync(order);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return expiredOrders.Count;
        }

        private async Task CancelOrderInternalAsync(ProductOrder order)
        {
            await RestoreStockForOrderAsync(order.OrderId);
            order.OrderStatus = InternalCancelledOrderStatus;
        }

        private async Task RestoreStockForOrderAsync(int orderId)
        {
            var orderItems = await _context.ProductOrderItems
                .Where(x => x.OrderId == orderId)
                .ToListAsync();

            foreach (var item in orderItems)
            {
                if (item.VariantId.HasValue)
                {
                    var variant = await _context.ProductVariants
                        .FirstOrDefaultAsync(x => x.VariantId == item.VariantId.Value);

                    if (variant != null)
                    {
                        variant.StockQuantity += item.Quantity;

                        if (variant.Status == "Hết hàng" && variant.StockQuantity > 0)
                        {
                            variant.Status = "Đang bán";
                        }
                    }
                }
                else
                {
                    var product = await _context.Products
                        .FirstOrDefaultAsync(x => x.ProductId == item.ProductId);

                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;

                        if (product.Status == "Hết hàng" && product.StockQuantity > 0)
                        {
                            product.Status = "Đang bán";
                        }
                    }
                }
            }
        }

        private static string NormalizeOrderStatus(string? status)
        {
            var value = (status ?? string.Empty).Trim();

            if (CancelledOrderStatuses.Contains(value))
                return "Đã hủy";

            if (ReceivedOrderStatuses.Contains(value))
                return "Đã nhận hàng";

            return "Đặt hàng thành công";
        }

        private static string NormalizePaymentStatus(string? status)
        {
            var value = (status ?? string.Empty).Trim();

            if (PaidStatuses.Contains(value))
                return "Đã thanh toán";

            return "Chưa thanh toán";
        }

        private async Task SaveVariantsAsync(int productId, List<StaffShoppingVariantInputVM>? variants)
        {
            variants ??= new List<StaffShoppingVariantInputVM>();

            variants = variants
                .Where(x => !string.IsNullOrWhiteSpace(x.VariantName))
                .ToList();

            var existing = await _context.ProductVariants
                .Where(x => x.ProductId == productId)
                .ToListAsync();

            var submittedIds = variants
                .Where(x => x.VariantId.HasValue)
                .Select(x => x.VariantId!.Value)
                .ToHashSet();

            foreach (var item in variants)
            {
                if (!string.IsNullOrWhiteSpace(item.SKU))
                {
                    var duplicated = await _context.ProductVariants.AnyAsync(x =>
                        x.ProductId == productId &&
                        x.Sku == item.SKU &&
                        (!item.VariantId.HasValue || x.VariantId != item.VariantId.Value));

                    if (duplicated)
                        throw new Exception($"SKU biến thể '{item.SKU}' đã tồn tại.");
                }

                if (item.VariantId.HasValue)
                {
                    var entity = existing.FirstOrDefault(x => x.VariantId == item.VariantId.Value);
                    if (entity == null) continue;

                    entity.VariantName = item.VariantName.Trim();
                    entity.Color = item.Color;
                    entity.Size = item.Size;
                    entity.Material = item.Material;
                    entity.Sku = string.IsNullOrWhiteSpace(item.SKU) ? null : item.SKU.Trim();
                    entity.PriceOverride = item.PriceOverride;
                    entity.StockQuantity = item.StockQuantity;
                    entity.Status = item.Status;
                    entity.ImageUrl = item.ImageUrl;
                    entity.UpdatedAt = DateTime.Now;
                }
                else
                {
                    _context.ProductVariants.Add(new ProductVariant
                    {
                        ProductId = productId,
                        VariantName = item.VariantName.Trim(),
                        Color = item.Color,
                        Size = item.Size,
                        Material = item.Material,
                        Sku = string.IsNullOrWhiteSpace(item.SKU) ? null : item.SKU.Trim(),
                        PriceOverride = item.PriceOverride,
                        StockQuantity = item.StockQuantity,
                        Status = item.Status,
                        ImageUrl = item.ImageUrl,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }
            }

            foreach (var old in existing.Where(x => !submittedIds.Contains(x.VariantId)))
            {
                old.Status = "Dừng bán";
                old.UpdatedAt = DateTime.Now;
            }
        }
    }
}