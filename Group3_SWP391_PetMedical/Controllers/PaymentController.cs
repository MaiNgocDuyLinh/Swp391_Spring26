using System;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using Group3_SWP391_PetMedical.Models; // Dùng cho PetClinicContext
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Group3_SWP391_PetMedical.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PayOSClient _payOSClient;
        private readonly PetClinicContext _db;

        private const string TestBankCode = "BIDV";
        private const string TestAccountNumber = "4880689237";
        private const string TestAccountName = "MAI NGOC DUY LINH";
        private const int TestAmount = 2000;
        private const string TestTransferContent = "TEST PAYOS";

        private readonly string _baseUrl;

        public PaymentController(IConfiguration configuration, PetClinicContext db)
        {
            _db = db;
            //_baseUrl = configuration["BaseUrl"] ?? "https://localhost:7000";
            _baseUrl = "https://tamia-pinkish-denzel.ngrok-free.dev";

            string clientId = configuration["PayOS:ClientId"] ?? throw new InvalidOperationException("Missing PayOS:ClientId");
            string apiKey = configuration["PayOS:ApiKey"] ?? throw new InvalidOperationException("Missing PayOS:ApiKey");
            string checksumKey = configuration["PayOS:ChecksumKey"] ?? throw new InvalidOperationException("Missing PayOS:ChecksumKey");

            _payOSClient = new PayOSClient(new PayOSOptions
            {
                ClientId = clientId,
                ApiKey = apiKey,
                ChecksumKey = checksumKey
            });
        }

        // ==============================================================================
        // 1. CÁC HÀM TEST CỦA LINH (ĐÃ KHÔI PHỤC)
        // ==============================================================================

        [HttpGet("create-test-payment")]
        public async Task<IActionResult> CreateTestPayment()
        {
            try
            {
                long orderCode = long.Parse(DateTimeOffset.Now.ToString("ffffff"));
                int price = TestAmount;

                var request = new CreatePaymentLinkRequest
                {
                    OrderCode = orderCode,
                    Amount = price, // 2000
                    Description = "Thanh toan PET1", // Giả lập khách thanh toán cho đơn hàng có ID = 1
                    CancelUrl = $"{_baseUrl}/api/payment/cancel",
                    ReturnUrl = $"{_baseUrl}/api/payment/success"
                };

                CreatePaymentLinkResponse result = await _payOSClient.PaymentRequests.CreateAsync(request);
                return Ok(new { checkoutUrl = result.CheckoutUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("fixed-qr")]
        public IActionResult GetFixedQr()
        {
            string addInfo = Uri.EscapeDataString(TestTransferContent);
            string accountName = Uri.EscapeDataString(TestAccountName);
            string qrImageUrl =
                $"https://img.vietqr.io/image/{TestBankCode}-{TestAccountNumber}-compact2.png?amount={TestAmount}&addInfo={addInfo}&accountName={accountName}";

            return Ok(new
            {
                bankCode = TestBankCode,
                accountNumber = TestAccountNumber,
                accountName = TestAccountName,
                amount = TestAmount,
                transferContent = TestTransferContent,
                qrImageUrl
            });
        }

        [HttpGet("success")]
        public IActionResult PaymentSuccess()
        {
            return Content(@"
                <html>
                <head><meta charset='utf-8'></head>
                <body style='font-family: Arial; text-align: center; margin-top: 50px; background-color: #f4f4f9;'>
                    <div style='background: white; padding: 40px; border-radius: 10px; box-shadow: 0 4px 8px rgba(0,0,0,0.1); display: inline-block;'>
                        <h1 style='color: #28a745;'>THANH TOÁN THÀNH CÔNG! 🎉</h1>
                        <p style='font-size: 18px; color: #333;'>Cảm ơn bạn đã mua hàng tại hệ thống Pet Medical.</p>
                        <br/>
                        <a href='/' style='text-decoration: none; padding: 10px 20px; background-color: #007bff; color: white; border-radius: 5px;'>Quay lại trang chủ</a>
                    </div>
                </body>
                </html>", "text/html");
        }

        [HttpGet("cancel")]
        public IActionResult PaymentCancel()
        {
            return Content(@"
                <html>
                <head><meta charset='utf-8'></head>
                <body style='font-family: Arial; text-align: center; margin-top: 50px; background-color: #f4f4f9;'>
                    <div style='background: white; padding: 40px; border-radius: 10px; box-shadow: 0 4px 8px rgba(0,0,0,0.1); display: inline-block;'>
                        <h1 style='color: #dc3545;'>ĐÃ HỦY THANH TOÁN ❌</h1>
                        <p style='font-size: 18px; color: #333;'>Bạn đã hủy giao dịch này.</p>
                        <br/>
                        <a href='/' style='text-decoration: none; padding: 10px 20px; background-color: #6c757d; color: white; border-radius: 5px;'>Quay lại trang chủ</a>
                    </div>
                </body>
                </html>", "text/html");
        }

        // ==============================================================================
        // 2. WEBHOOK: XỬ LÝ THANH TOÁN THÀNH CÔNG VÀ TRỪ KHO
        // ==============================================================================

        [HttpPost("payos-webhook")]
        public async Task<IActionResult> ReceivePayOSWebhook([FromBody] JsonElement webhookBody)
        {
            try
            {
                // 1. IN RA TOÀN BỘ DỮ LIỆU THÔ PAYOS GỬI ĐẾN ĐỂ DEBUG
                Console.WriteLine("\n==============================================");
                Console.WriteLine("[WEBHOOK NHẬN ĐƯỢC]: " + webhookBody.GetRawText());
                Console.WriteLine("==============================================\n");

                if (webhookBody.TryGetProperty("data", out JsonElement data))
                {
                    long orderCode = data.GetProperty("orderCode").GetInt64();
                    int amount = data.GetProperty("amount").GetInt32();
                    string description = data.GetProperty("description").GetString() ?? "";

                    Console.WriteLine($"[TING TING] Số tiền: {amount} - Nội dung: {description}");

                    if (description == "Ma giao dich thu nghiem" || description == "VQRIO123")
                    {
                        return Ok(new { success = true });
                    }

                    // TÌM MÃ ĐƠN HÀNG "PET{id}" ĐỂ TRỪ KHO
                    var match = Regex.Match(description, @"PET(?<id>\d+)", RegexOptions.IgnoreCase);
                    if (match.Success && int.TryParse(match.Groups["id"].Value, out var orderId))
                    {
                        var order = await _db.RetailOrders
                            .Include(o => o.OrderDetails)
                                .ThenInclude(od => od.medicine)
                            .FirstOrDefaultAsync(o => o.id == orderId);

                        if (order != null && order.status == "PENDING" && amount >= (int)order.total_amount)
                        {
                            order.status = "PAID";
                            order.transaction_reference = orderCode.ToString();

                            foreach (var detail in order.OrderDetails)
                            {
                                if (detail.medicine != null)
                                {
                                    int currentStock = detail.medicine.stock_quantity ?? 0;
                                    detail.medicine.stock_quantity = Math.Max(0, currentStock - detail.quantity);
                                }
                            }

                            await _db.SaveChangesAsync();
                            Console.WriteLine($"[THÀNH CÔNG] Đã trừ kho cho đơn PET{orderId}");
                        }
                    }
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WEBHOOK LỖI]: {ex.Message}");
                return Ok(new { success = false });
            }
        }


        //[HttpPost("payos-webhook")]
        //public async Task<IActionResult> ReceivePayOSWebhook([FromBody] JsonElement webhookBody)
        //{
        //    try
        //    {
        //        if (webhookBody.TryGetProperty("data", out JsonElement data))
        //        {
        //            long orderCode = data.GetProperty("orderCode").GetInt64();
        //            int amount = data.GetProperty("amount").GetInt32();
        //            string description = data.GetProperty("description").GetString() ?? "";

        //            Console.WriteLine($"\n[TING TING] Có Webhook gọi tới! Số tiền: {amount} - Nội dung: {description}");

        //            if (description == "Ma giao dich thu nghiem" || description == "VQRIO123")
        //            {
        //                Console.WriteLine("-> Đây là giao dịch Test của PayOS, bỏ qua DB.");
        //                return Ok(new { success = true });
        //            }

        //            // TÌM MÃ ĐƠN HÀNG "PET{id}" ĐỂ TRỪ KHO
        //            var match = Regex.Match(description, @"PET(?<id>\d+)", RegexOptions.IgnoreCase);
        //            if (match.Success && int.TryParse(match.Groups["id"].Value, out var orderId))
        //            {
        //                var order = await _db.RetailOrders
        //                    .Include(o => o.OrderDetails)
        //                        .ThenInclude(od => od.medicine)
        //                    .FirstOrDefaultAsync(o => o.id == orderId);

        //                if (order != null && order.status == "PENDING" && amount >= (int)order.total_amount)
        //                {
        //                    order.status = "PAID";
        //                    order.transaction_reference = orderCode.ToString();

        //                    foreach (var detail in order.OrderDetails)
        //                    {
        //                        if (detail.medicine != null)
        //                        {
        //                            int currentStock = detail.medicine.stock_quantity ?? 0;
        //                            detail.medicine.stock_quantity = Math.Max(0, currentStock - detail.quantity);
        //                        }
        //                    }

        //                    await _db.SaveChangesAsync();
        //                    Console.WriteLine($"[THÀNH CÔNG] Đã cập nhật đơn PET{orderId} thành PAID và trừ kho thuốc!");
        //                }
        //            }
        //        }

        //        return Ok(new { success = true });
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[WEBHOOK LỖI]: {ex.Message}");
        //        return Ok(new { success = false });
        //    }
        //}
    }
}