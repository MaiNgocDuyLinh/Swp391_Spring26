using System.Security.Claims;
using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.ViewModels.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Group3_SWP391_PetMedical.Controllers
{
    [Authorize(Roles = "Customer")]
    
    public class OrderHistoryController : Controller
    {
        private readonly PetClinicContext _context;

        public OrderHistoryController(PetClinicContext petClinicContext)
        {
            _context = petClinicContext;
        }


        public async Task<IActionResult> OrderHistory()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Forbid();

            var orders = await _context.RetailOrders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.medicine)
                .Where(o => o.user_id == userId.Value)
                .OrderByDescending(o => o.created_at) // dơn mới nhất lên đầu
                .Select(o => new OrderHistoryVm
                {
                    OrderId = o.id,
                    CreatedAt = o.created_at,
                    TotalAmount = o.total_amount,
                    Status = o.status,
                    PickupSlot = o.pickup_slot,
                    Note = o.note,
                    MedicineNames = o.OrderDetails.Select(od => od.medicine.name).ToList()
                })
                .ToListAsync();

            return View("~/Views/Orders/OrderHistory.cshtml", orders);
        }
        private int? GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return int.TryParse(raw, out var id) ? id : null;
        }
    }
}
