using DACN.Models;
using DACN.Models.VNPAY;
using DACN.Models.MoMo;
using DACN.Services.Momo;
using DACN.Services.VNPay;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace DACN.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly IMomoService _momoService;
        private readonly ApplicationDbContext _context;

        public PaymentController(IVnPayService vnPayService, IMomoService momoService, ApplicationDbContext context)
        {
            _vnPayService = vnPayService;
            _momoService = momoService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index() => View();

        // =================== TẠO THANH TOÁN VNPay ===================
        [HttpPost]
        [Route("Payment/CreatePaymentUrlVnpay")]
        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            // Lưu lại hình thức thanh toán trước khi chuyển hướng
            var datPhong = _context.DatPhong.FirstOrDefault(d => d.MaDatPhong == model.OrderId);
            if (datPhong != null)
            {
                datPhong.HinhThucThanhToan = "VNPay";
                _context.SaveChanges();
            }

            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return Redirect(url);
        }

        // =================== TẠO THANH TOÁN MoMo ===================
        [HttpPost]
        [Route("Payment/CreatePaymentMoMo")]
        public async Task<IActionResult> CreatePaymentMoMo(int maDatPhong)
        {
            var datPhong = _context.DatPhong.FirstOrDefault(d => d.MaDatPhong == maDatPhong);
            if (datPhong == null)
                return Content("⚠️ Không tìm thấy đơn đặt phòng.");

            // ✅ Cập nhật hình thức thanh toán trước khi tạo request MoMo
            datPhong.HinhThucThanhToan = "MoMo";
            _context.SaveChanges();

            // Tạo thông tin gửi sang MoMo
            var model = new OrderInfoModel
            {
                OrderId = $"{datPhong.MaDatPhong}_{Guid.NewGuid().ToString().Replace("-", "")}",
                // Gửi mã đơn thật
                OrderInfo = $"Thanh toán đặt phòng #{datPhong.MaDatPhong}",
                Amount = ((long)datPhong.TongTien).ToString(),
                FullName = datPhong.KhachHang?.HoTen ?? "Khách hàng"
            };

            var response = await _momoService.CreatePaymentMoMo(model);

            if (response == null || string.IsNullOrEmpty(response.payUrl))
                return Content("❌ Không tạo được URL thanh toán MoMo. Vui lòng kiểm tra cấu hình hoặc dữ liệu gửi đi.");

            return Redirect(response.payUrl);
        }

        // =================== CALLBACK TỪ VNPay ===================
        [HttpGet]
        public IActionResult PaymentReturn()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response == null)
                return Content("❌ Không nhận được phản hồi từ VNPay.");

            if (response.VnPayResponseCode == "00")
            {
                // Cập nhật đơn với hình thức là VNPay
                return CapNhatDonThanhCong(response.OrderId, "VNPay");
            }
            else
            {
                return Content("❌ Thanh toán VNPay thất bại. Mã lỗi: " + response.VnPayResponseCode);
            }
        }

        // =================== CALLBACK TỪ MoMo ===================
        [HttpGet]
        [Route("Payment/PaymentReturnMomo")]
        public IActionResult PaymentReturnMomo()
        {
            var response = _momoService.PaymentExecuteAsync(Request.Query);

            if (response == null || string.IsNullOrEmpty(response.OrderId))
                return Content("❌ Không nhận được phản hồi từ MoMo.");

            // Cập nhật đơn với hình thức là MoMo
            return CapNhatDonThanhCong(response.OrderId.ToString(), "MoMo");
        }

        // =================== HÀM DÙNG CHUNG CHO CẢ VNPAY & MOMO ===================
        private IActionResult CapNhatDonThanhCong(string orderIdStr, string hinhThuc)
        {
            try
            {
                // ✅ Nếu orderId dạng "64_xxx" → tách ra lấy 64
                var orderIdClean = orderIdStr.Contains("_")
                    ? orderIdStr.Split('_')[0]
                    : orderIdStr;

                if (!int.TryParse(orderIdClean, out int orderId))
                    return Content("⚠️ Mã đơn đặt phòng không hợp lệ: " + orderIdStr);


                var datPhong = _context.DatPhong.FirstOrDefault(d => d.MaDatPhong == orderId);
                if (datPhong == null)
                    return Content($"⚠️ Không tìm thấy đơn đặt phòng có mã {orderId}.");

                // Cập nhật trạng thái và hình thức thanh toán
                datPhong.TrangThai = "DaDat";
                datPhong.HinhThucThanhToan = hinhThuc;

                // Cập nhật trạng thái phòng
                var phong = _context.Phong.FirstOrDefault(p => p.MaPhong == datPhong.MaPhong);
                if (phong != null)
                    phong.TrangThai = "DaDat";

                _context.SaveChanges();

                // Hiển thị giao diện chúc mừng
                return Content($@"
<html>
    <head>
        <meta charset='utf-8' />
        <title>Thanh toán thành công</title>
        <style>
            body {{
                font-family: Arial, sans-serif;
                background-color: #f8f9fa;
                text-align: center;
                padding: 80px;
            }}
            .card {{
                display: inline-block;
                background: white;
                padding: 40px 60px;
                border-radius: 15px;
                box-shadow: 0 4px 10px rgba(0,0,0,0.1);
            }}
            h1 {{
                color: #28a745;
            }}
            p {{
                font-size: 18px;
                color: #333;
            }}
            a.button {{
                display: inline-block;
                margin-top: 20px;
                background-color: #007bff;
                color: white;
                padding: 10px 20px;
                border-radius: 8px;
                text-decoration: none;
                font-weight: bold;
            }}
            a.button:hover {{
                background-color: #0056b3;
            }}
        </style>
    </head>
    <body>
        <div class='card'>
            <h1>🎉 Thanh toán {hinhThuc} thành công!</h1>
            <p>Bạn đã đặt phòng thành công với mã đơn <b>#{orderId}</b>.</p>
            <p>Hình thức thanh toán: <b>{hinhThuc}</b></p>
            <a href='/' class='button'>🏠 Quay lại trang chủ</a>
        </div>
    </body>
</html>
", "text/html");
            }
            catch (Exception ex)
            {
                return Content("⚠️ Lỗi khi cập nhật đơn đặt phòng: " + ex.Message);
            }
        }

        [HttpGet]
        [Route("Payment/PaymentConfirm")]
        public IActionResult PaymentConfirm()
        {
            var response = _momoService.PaymentExecuteAsync(Request.Query);
            if (response == null)
                return Content("❌ Không nhận được phản hồi từ MoMo.");

            return CapNhatDonThanhCong(response.OrderId.ToString(), "MoMo");
        }

        [HttpPost]
        [Route("Payment/SavePayment")]
        public IActionResult SavePayment()
        {
            var response = _momoService.PaymentExecuteAsync(Request.Query);
            if (response == null)
                return Content("❌ Notify từ MoMo không hợp lệ.");

            return CapNhatDonThanhCong(response.OrderId.ToString(), "MoMo");
        }

    }
}
