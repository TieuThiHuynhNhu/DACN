using DACN.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace DACN.Services
{
    public class EmailAnniversaryService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private static readonly HttpClient _httpClient = new HttpClient();

        // ⭐ CHẾ ĐỘ TEST: true để quét đơn hôm nay, false để chạy thật kỷ niệm 1 năm.
        private bool _isTestMode = true;

        public EmailAnniversaryService(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("===============================================================");
            Console.WriteLine("[EMAIL-SERVICE] HỆ THỐNG MARKETING CAO CẤP ĐÃ KHỞI CHẠY.");
            Console.WriteLine("===============================================================");

            await ProcessAnniversaryEmails();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                await ProcessAnniversaryEmails();
            }
        }

        private async Task ProcessAnniversaryEmails()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                try
                {
                    IQueryable<DatPhong> query = context.DatPhong
                        .Include(d => d.KhachHang)
                        .Include(d => d.Phong);

                    DateTime targetDate;
                    if (_isTestMode)
                    {
                        var today = DateTime.Today;
                        query = query.Where(d => d.NgayDat.Date == today);
                        targetDate = today;
                    }
                    else
                    {
                        targetDate = DateTime.Now.AddMonths(-11);
                        query = query.Where(d => d.NgayNhan.HasValue &&
                                                d.NgayNhan.Value.Month == targetDate.Month &&
                                                d.NgayNhan.Value.Day == targetDate.Day);
                    }

                    var allBookings = await query.ToListAsync();

                    var familyBookings = allBookings
                        .Where(d => d.Phong != null &&
                                    (d.Phong.LoaiPhong.ToLower().Contains("gia đình") ||
                                     d.Phong.LoaiPhong.ToLower().Contains("family")))
                        .ToList();

                    if (!familyBookings.Any()) return;

                    foreach (var booking in familyBookings)
                    {
                        var email = booking.KhachHang?.Email;
                        var ten = booking.KhachHang?.HoTen;

                        if (string.IsNullOrEmpty(email)) continue;

                        Console.WriteLine($"[TIẾN TRÌNH] Đang thiết kế Email chuyên nghiệp cho: {ten}...");

                        // 1. Gọi Gemini soạn nội dung thư cá nhân
                        string aiMessage = await GenerateAiPersonalMessage(ten, booking.Phong.LoaiPhong);

                        // 2. Lồng nội dung vào Template HTML cao cấp
                        string finalHtmlBody = BuildProfessionalHtmlEmail(ten, aiMessage);

                        // 3. Gửi Email
                        await SendActualEmail(email, "🌟 Món quà kỷ niệm đặc biệt từ SeaHotel", finalHtmlBody);
                    }
                }
                catch (Exception ex) { Console.WriteLine("[LỖI DB] " + ex.Message); }
            }
        }

        // --- BỘ NÃO AI: SOẠN NỘI DUNG LỜI CHÚC CẢM XÚC ---
        private async Task<string> GenerateAiPersonalMessage(string customerName, string roomType)
        {
            const string apiKey = ""; // ⚠️ Dán API Key Gemini của bạn vào đây ⚠️
            if (string.IsNullOrEmpty(apiKey))
                return $"Thật nhanh khi đã tròn một năm kể từ ngày gia đình mình lưu trú tại phòng {roomType} của SeaHotel. Chúng tôi vẫn luôn trân trọng những kỷ niệm tuyệt vời đó và mong sớm được đón tiếp gia đình trở lại.";

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-preview-09-2025:generateContent?key={apiKey}";
            var prompt = $"Viết một đoạn văn (khoảng 80-100 chữ) gửi khách hàng {customerName}. Nội dung: Chúc mừng kỷ niệm 1 năm họ ở phòng {roomType} tại SeaHotel. Nhắc về sự gắn kết gia đình, không gian biển yên bình và lòng hiếu khách của chúng tôi. Văn phong sang trọng, ấm áp, giàu hình ảnh.";

            try
            {
                var content = new StringContent(JsonSerializer.Serialize(new { contents = new[] { new { parts = new[] { new { text = prompt } } } } }), Encoding.UTF8, "application/json");
                var res = await _httpClient.PostAsync(url, content);
                if (res.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
                    return doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                }
            }
            catch { }
            return "SeaHotel luôn trân trọng từng khoảnh khắc bạn lưu trú tại đây.";
        }

        // --- THIẾT KẾ GIAO DIỆN EMAIL CHI TIẾT (HTML/CSS) ---
        private string BuildProfessionalHtmlEmail(string customerName, string aiMessage)
        {
            string homeUrl = "https://localhost:7049/";

            return $@"
            <html>
            <body style='font-family: Segoe UI, Tahoma, sans-serif; background-color: #f3f6f9; margin: 0; padding: 0;'>
                <table align='center' border='0' cellpadding='0' cellspacing='0' width='600' style='background-color: #ffffff; margin: 20px auto; border-radius: 24px; overflow: hidden; box-shadow: 0 20px 40px rgba(0,0,0,0.08); border: 1px solid #eef2f6;'>
                    
                    <!-- Top Banner -->
                    <tr>
                        <td align='center' style='padding: 0; position: relative;'>
                            <img src='https://images.unsplash.com/photo-1571896349842-33c89424de2d?auto=format&fit=crop&w=600&h=300' alt='SeaHotel Resort' style='width: 100%; display: block; object-fit: cover;'>
                        </td>
                    </tr>
                    
                    <!-- Main Body -->
                    <tr>
                        <td style='padding: 50px 40px;'>
                            <div style='text-align: center; margin-bottom: 30px;'>
                                <span style='background: #e0f2fe; color: #0284c7; padding: 8px 20px; border-radius: 50px; font-size: 10px; font-weight: 800; text-transform: uppercase; letter-spacing: 2px;'>Dành riêng cho gia đình bạn</span>
                            </div>
                            
                            <h1 style='color: #1e3a8a; margin-top: 0; text-align: center; font-size: 28px; font-weight: 900; letter-spacing: -1px;'>Chào {customerName},</h1>
                            
                            <p style='color: #4b5563; line-height: 1.8; font-size: 16px; text-align: justify;'>
                                {aiMessage.Replace("\n", "<br>")}
                            </p>

                            <p style='color: #4b5563; line-height: 1.8; font-size: 16px; margin-top: 20px;'>
                                Đúng ngày này của một năm trước, chúng tôi đã có vinh hạnh được phục vụ gia đình bạn. Thời gian trôi qua thật nhanh, nhưng những nụ cười và khoảnh khắc hạnh phúc của các bạn tại SeaHotel vẫn luôn là động lực để đội ngũ của chúng tôi hoàn thiện hơn mỗi ngày.
                            </p>
                            
                            <!-- Benefits Grid -->
                            <table width='100%' style='margin: 40px 0; border-collapse: separate; border-spacing: 10px;'>
                                <tr>
                                    <td width='33%' style='background: #f8fafc; padding: 20px; border-radius: 15px; text-align: center;'>
                                        <div style='font-size: 24px; margin-bottom: 10px;'>🌊</div>
                                        <div style='font-size: 11px; font-weight: 800; color: #1e3a8a;'>VIEW BIỂN</div>
                                    </td>
                                    <td width='33%' style='background: #f8fafc; padding: 20px; border-radius: 15px; text-align: center;'>
                                        <div style='font-size: 24px; margin-bottom: 10px;'>🍳</div>
                                        <div style='font-size: 11px; font-weight: 800; color: #1e3a8a;'>BUFFET 5⭐</div>
                                    </td>
                                    <td width='33%' style='background: #f8fafc; padding: 20px; border-radius: 15px; text-align: center;'>
                                        <div style='font-size: 24px; margin-bottom: 10px;'>🧖</div>
                                        <div style='font-size: 11px; font-weight: 800; color: #1e3a8a;'>SPA TRỊ LIỆU</div>
                                    </td>
                                </tr>
                            </table>

                            <!-- Voucher Section -->
                            <div style='background: linear-gradient(135deg, #0ea5e9 0%, #2563eb 100%); border-radius: 20px; padding: 35px; text-align: center; color: #ffffff; margin-bottom: 40px; box-shadow: 0 10px 20px rgba(37, 99, 235, 0.2);'>
                                <p style='margin: 0; font-weight: bold; text-transform: uppercase; font-size: 12px; letter-spacing: 3px; opacity: 0.8;'>Đặc quyền kỷ niệm một năm</p>
                                <h2 style='margin: 10px 0; font-size: 40px; font-weight: 900; letter-spacing: -2px;'>GIẢM GIÁ 25%</h2>
                                <p style='margin: 0 0 20px 0; font-size: 14px; opacity: 0.9;'>Áp dụng cho mọi dịch vụ đặt phòng trực tuyến</p>
                                <div style='display: inline-block; background: #ffffff; padding: 12px 30px; border-radius: 12px; color: #1e40af; font-family: Courier, monospace; font-size: 22px; font-weight: 900; border: 2px solid rgba(255,255,255,0.3);'>SEALOVE2025</div>
                            </div>

                            <p style='color: #6b7280; font-size: 14px; text-align: center; margin-bottom: 30px; font-style: italic;'>
                                * Ưu đãi này chỉ dành riêng cho gia đình bạn và có hiệu lực trong vòng 30 ngày tới.
                            </p>

                            <!-- CTA Button -->
                            <div style='text-align: center;'>
                                <a href='{homeUrl}' style='background-color: #1e3a8a; color: #ffffff; padding: 20px 45px; text-decoration: none; border-radius: 15px; font-weight: 800; display: inline-block; font-size: 16px; box-shadow: 0 8px 15px rgba(30, 58, 138, 0.25); text-transform: uppercase; letter-spacing: 1px;'>Trở lại SeaHotel ngay</a>
                            </div>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #0f172a; padding: 45px 40px; text-align: center; color: #94a3b8;'>
                            <p style='margin: 0; color: #ffffff; font-size: 16px; font-weight: 900; letter-spacing: 2px;'>SEAHOTEL LUXURY RESORT</p>
                            <p style='margin: 10px 0 25px 0; font-size: 13px; opacity: 0.7;'>Nơi sóng biển hòa quyện cùng sự phục vụ tận tâm</p>
                            
                            <div style='margin-bottom: 25px;'>
                                <span style='display: inline-block; margin: 0 10px; color: #38bdf8;'>Facebook</span>
                                <span style='display: inline-block; margin: 0 10px; color: #38bdf8;'>Instagram</span>
                                <span style='display: inline-block; margin: 0 10px; color: #38bdf8;'>Website</span>
                            </div>

                            <div style='border-top: 1px solid rgba(255,255,255,0.1); pt-25px; padding-top: 25px; font-size: 11px; line-height: 1.6;'>
                                <p style='margin-bottom: 5px;'>123 Đường Bờ Biển, Phường Lộc Thọ, Nha Trang, Việt Nam</p>
                                <p>© 2025 SeaHotel Smart Management. Được vận hành bởi Trí tuệ nhân tạo Gemini.</p>
                                <p style='margin-top: 15px; opacity: 0.5;'>Nếu bạn không muốn nhận email này, vui lòng <a href='#' style='color: #94a3b8;'>Hủy đăng ký</a>.</p>
                            </div>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
        }

        private async Task<bool> SendActualEmail(string toEmail, string subject, string htmlBody)
        {
            try
            {
                string myEmail = "nhutieu.27032004@gmail.com";
                string myAppPassword = "qixl rniy wyox gddv";

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(myEmail, myAppPassword),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(myEmail, "SeaHotel Resort 🌟"),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine($"[SUCCESS] >>> ĐÃ GỬI EMAIL MARKETING CHI TIẾT TỚI: {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[SMTP ERROR] " + ex.Message);
                return false;
            }
        }
    }
}