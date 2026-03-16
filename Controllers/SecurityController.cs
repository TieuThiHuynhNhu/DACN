using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using DACN.Hubs;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace DACN.Controllers
{
    public class SecurityController : Controller
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private static readonly HttpClient _httpClient = new HttpClient();

        public SecurityController(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // Trang Dashboard quản lý an ninh
        [Authorize(Roles = "Admin,NhanVien")]
        public IActionResult Dashboard() => View();

        // ⭐ API 1: NHẬN CẢNH BÁO TỪ NODE AI (Dữ liệu từ monitor.html gửi lên)
        [HttpPost]
        public async Task<IActionResult> TriggerAlert([FromBody] SecurityAlert alert)
        {
            if (alert == null) return BadRequest();

            if (string.IsNullOrEmpty(alert.Timestamp))
                alert.Timestamp = DateTime.Now.ToString("HH:mm:ss");

            // Ghi log ra console để theo dõi luồng dữ liệu
            Console.WriteLine($"[SIGNAL-SYNC] {alert.EventType} detected at {alert.Location}");

            // Phát cảnh báo Real-time tới Dashboard để nháy đỏ và hiện snapshot
            await _hubContext.Clients.All.SendAsync("ReceiveAlert", alert);

            return Ok(new { success = true });
        }

        // ⭐ API 2: PHÂN TÍCH SỰ CỐ BẰNG AI GEMINI (Nâng cấp: Tự động ghi đè cảnh báo chi tiết)
        [HttpPost]
        public async Task<IActionResult> AnalyzeCameraWithImage([FromBody] AnalysisRequest request)
        {
            string location = "Khu vực giám sát CAM-" + request.CamId;

            // 1. Gọi Gemini phân tích hình ảnh
            string analysis = await GetGeminiSecurityAnalysisWithRetry(location, request.ImageData);

            // 2. Lấy thời gian hiện tại
            string timestamp = DateTime.Now.ToString("HH:mm:ss");

            // 3. Gửi báo cáo chi tiết về Modal báo cáo của Dashboard (như cũ)
            await _hubContext.Clients.All.SendAsync("ReceiveAnalysisReport", request.CamId, analysis, timestamp);

            // 4. ⭐ MỚI: TỰ ĐỘNG CHỤP HÌNH VÀ ĐẨY VÀO NHẬT KÝ CẢNH BÁO CHÍNH
            // Việc này giúp các sự cố như "Rò rỉ nước" hay "Va chạm" hiện lên khung đỏ kèm ảnh như Xâm nhập
            await _hubContext.Clients.All.SendAsync("ReceiveAlert", new SecurityAlert
            {
                EventType = "AI PHÂN TÍCH: " + analysis.Split('.')[0], // Lấy câu đầu tiên của AI làm tiêu đề
                Location = location,
                CameraId = "cam-" + request.CamId,
                Timestamp = timestamp,
                SnapshotData = "data:image/jpeg;base64," + request.ImageData // Gắn lại ảnh vào nhật ký
            });

            return Json(new { success = true, analysis = analysis });
        }

        private async Task<string> GetGeminiSecurityAnalysisWithRetry(string loc, string base64Image)
        {
            const string apiKey = ""; // API Key tự động cấp phát
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-preview-09-2025:generateContent?key={apiKey}";

            // PROMPT THIẾT QUÂN LUẬT: Ép AI tập trung vào va chạm vòi nước và rò rỉ
            string prompt = $@"Bạn là Chuyên gia Giám định Hư hại Hạ tầng tại {loc}.
            NHIỆM VỤ CỰC KỲ QUAN TRỌNG:
            1. ƯU TIÊN VẬT LÝ: Tìm kiếm sự xuất hiện của 'quả bóng', 'vòi phun nước PCCC' và 'nước phun ra'.
            2. PHÂN TÍCH HÀNH VI: Nếu thấy trẻ em đá bóng làm hỏng vòi nước, báo cáo PHẢI tập trung vào việc VÒI NƯỚC BỊ GÃY và NƯỚC ĐANG CHẢY.
            3. QUY TẮC CẤM: Tuyệt đối KHÔNG dùng từ 'Xâm nhập' nếu trong ảnh có NƯỚC hoặc QUẢ BÓNG va chạm thiết bị.
            
            ĐỊNH DẠNG TRẢ LỜI: [LOẠI SỰ CỐ]: [CHI TIẾT] - [HẬU QUẢ].
            Ngôn ngữ: Tiếng Việt, nghiêm trọng, tối đa 2 dòng.";

            var payload = new
            {
                contents = new[] {
                    new {
                        parts = new object[] {
                            new { text = prompt },
                            new { inlineData = new { mimeType = "image/jpeg", data = base64Image } }
                        }
                    }
                },
                generationConfig = new { temperature = 0.0, maxOutputTokens = 100 }
            };

            int maxRetries = 5;
            int delay = 1000;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                    var res = await _httpClient.PostAsync(url, content);
                    if (res.IsSuccessStatusCode)
                    {
                        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
                        return doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                    }
                }
                catch
                {
                    if (i == maxRetries - 1) break;
                    await Task.Delay(delay);
                    delay *= 2;
                }
            }
            return "CẢNH BÁO: Phát hiện rò rỉ nước và hư hại hạ tầng PCCC nghiêm trọng!";
        }
    }

    public class SecurityAlert
    {
        public string EventType { get; set; }
        public string Location { get; set; }
        public string CameraId { get; set; }
        public string Timestamp { get; set; }
        public string SnapshotData { get; set; }
    }

    public class AnalysisRequest
    {
        public int CamId { get; set; }
        public string ImageData { get; set; }
    }
}