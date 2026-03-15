using System;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Models.V2.PaymentRequests;

namespace Group3_SWP391_PetMedical.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PayOSClient _payOSClient;
        private const string TestBankCode = "BIDV";
        private const string TestAccountNumber = "4880689237"; // 4880689237
        private const string TestAccountName = "MAI NGOC DUY LINH";
        private const int TestAmount = 2000;
        private const string TestTransferContent = "TEST PAYOS";

        private readonly string _baseUrl;

        public PaymentController(IConfiguration configuration)
        {
            _baseUrl = configuration["BaseUrl"] ?? "http://localhost:7000";

            // Lấy thông tin từ appsettings.json đã cấu hình
            string clientId = configuration["PayOS:ClientId"]
                ?? throw new InvalidOperationException("Missing PayOS:ClientId in configuration.");
            string apiKey = configuration["PayOS:ApiKey"]
                ?? throw new InvalidOperationException("Missing PayOS:ApiKey in configuration.");
            string checksumKey = configuration["PayOS:ChecksumKey"]
                ?? throw new InvalidOperationException("Missing PayOS:ChecksumKey in configuration.");

            _payOSClient = new PayOSClient(new PayOSOptions
            {
                ClientId = clientId,
                ApiKey = apiKey,
                ChecksumKey = checksumKey
            });
        }

        [HttpGet("create-test-payment")]
        public async Task<IActionResult> CreateTestPayment()
        {
            try
            {
                // 1. Tạo mã đơn hàng duy nhất (dùng Timestamp để không bị trùng)
                long orderCode = long.Parse(DateTimeOffset.Now.ToString("ffffff"));

                // 2. Hardcode giá tiền (Ví dụ: 2000 VNĐ để test tiền thật)
                int price = TestAmount;

                // 3. Cấu hình thông tin thanh toán hardcode để test nhanh
                var request = new CreatePaymentLinkRequest
                {
                    OrderCode = orderCode,
                    Amount = price,
                    Description = "Thanh toan Test",
                    CancelUrl = $"{_baseUrl}/api/payment/cancel",
                    ReturnUrl = $"{_baseUrl}/api/payment/success"
                };

                // 4. Gọi PayOS để tạo Link
                CreatePaymentLinkResponse result = await _payOSClient.PaymentRequests.CreateAsync(request);

                // 5. Trả về URL để bạn click vào thanh toán thử
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

        [HttpPost("payos-webhook")]
        public IActionResult ReceivePayOSWebhook([FromBody] JsonElement webhookBody)
        {
            try
            {
                // PayOS luôn gửi về một JSON chứa thuộc tính "data". 
                // Ta trích xuất nó ra để đọc trực tiếp mà không cần phụ thuộc vào thư viện.
                if (webhookBody.TryGetProperty("data", out JsonElement data))
                {
                    // Lấy mã đơn hàng, số tiền và lời nhắn
                    long orderCode = data.GetProperty("orderCode").GetInt64();
                    int amount = data.GetProperty("amount").GetInt32();
                    string description = data.GetProperty("description").GetString() ?? "";

                    // Bỏ qua các giao dịch PayOS tự động gửi để test link ngrok
                    if (description == "Ma giao dich thu nghiem" || description == "VQRIO123")
                    {
                        return Ok(new { success = true });
                    }

                    // --------------------------------------------------------
                    // 3. CẬP NHẬT DATABASE CỦA BẠN TẠI ĐÂY
                    // Ví dụ: _dbContext.Orders.UpdateStatus(orderCode, "PAID");
                    // --------------------------------------------------------

                    Console.WriteLine($"[WEBHOOK] TUYỆT VỜI! Đã nhận {amount} VNĐ cho đơn hàng {orderCode}");
                }

                // Luôn phải báo OK để PayOS biết ta đã nhận được, nếu không nó sẽ gửi lại liên tục
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WEBHOOK LỖI]: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ==============================================================================
        // 2 HÀM MỚI ĐỂ HIỂN THỊ GIAO DIỆN KHI NGƯỜI DÙNG QUAY VỀ TỪ TRANG THANH TOÁN
        // ==============================================================================

        [HttpGet("success")]
        public IActionResult PaymentSuccess()
        {
            // Trả về một mã HTML đơn giản hiển thị giao diện báo thành công
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
            // Trả về một mã HTML báo đã hủy
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
    }
}