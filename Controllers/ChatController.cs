using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DACN.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using DACN.Hubs;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace DACN.Controllers
{
    // 🔓 Gỡ bỏ Authorize ở cấp Controller để ai cũng truy cập được
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private static readonly HttpClient _httpClient = new HttpClient();

        public ChatController(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // --- LẤY HOẶC TẠO PHIÊN CHAT CHO CẢ KHÁCH VÀ USER ---
        [HttpGet]
        public async Task<IActionResult> GetCurrentConversation()
        {
            // 1. Xác định định danh (Nếu đã login dùng MaKhachHang, nếu chưa dùng SessionId)
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string anonymousId = HttpContext.Session.Id;

            CuocTroChuyen conversation;

            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                // Trường hợp đã đăng nhập
                conversation = await _context.CuocTroChuyens
                    .FirstOrDefaultAsync(c => c.KhachHangId == userId && c.KetThucLuc == null);

                if (conversation == null)
                {
                    conversation = new CuocTroChuyen
                    {
                        KhachHangId = userId,
                        TenKhachHang = User.Identity?.Name ?? "Thành viên",
                        BatDauLuc = DateTime.UtcNow
                    };
                    _context.CuocTroChuyens.Add(conversation);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                // 🕵️ Trường hợp KHÁCH VÃNG LAI (Dùng SessionId để nhận diện)
                // Lưu ý: Lưu SessionId vào TenKhachHang để tìm lại trong phiên làm việc
                conversation = await _context.CuocTroChuyens
                    .FirstOrDefaultAsync(c => c.TenKhachHang == "Guest_" + anonymousId && c.KetThucLuc == null);

                if (conversation == null)
                {
                    conversation = new CuocTroChuyen
                    {
                        TenKhachHang = "Guest_" + anonymousId, // Định danh tạm thời
                        BatDauLuc = DateTime.UtcNow
                    };
                    _context.CuocTroChuyens.Add(conversation);
                    await _context.SaveChangesAsync();
                }
            }

            return Json(new { success = true, conversationId = conversation.Id });
        }

        // --- LẤY TIN NHẮN (KHÔNG CẦN LOGIN) ---
        [HttpGet]
        public async Task<IActionResult> GetMessages(int conversationId)
        {
            var msgs = await _context.TinNhans
                .Where(t => t.CuocTroChuyenId == conversationId)
                .OrderBy(t => t.ThoiGian)
                .Select(t => new {
                    guiBoiName = t.GuiBoiName.StartsWith("Guest_") ? "Khách" : t.GuiBoiName,
                    noiDung = t.NoiDung,
                    guiBoiRole = t.GuiBoiRole,
                    thoiGian = t.ThoiGian.ToLocalTime().ToString("HH:mm")
                })
                .ToListAsync();
            return Json(msgs);
        }

        // --- GỬI TIN NHẮN (KHÔNG CẦN LOGIN) ---
        [HttpPost]
        public async Task<IActionResult> SendMessage(int conversationId, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return Json(new { success = false });

            // Xác định vai trò dựa trên Identity
            string role = "KhachHang";
            string senderName = "Khách";

            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin") || User.IsInRole("NhanVien"))
                {
                    role = "NhanVien";
                    senderName = User.Identity.Name;
                }
                else
                {
                    senderName = User.Identity.Name;
                }
            }
            else
            {
                senderName = "Khách_" + (HttpContext.Session.Id.Length > 4 ? HttpContext.Session.Id.Substring(0, 4) : "User");
            }

            var timeNow = DateTime.Now.ToString("HH:mm");

            // 💾 Lưu tin nhắn
            var userMsg = new TinNhan
            {
                CuocTroChuyenId = conversationId,
                GuiBoiRole = role,
                GuiBoiName = senderName,
                NoiDung = message,
                ThoiGian = DateTime.UtcNow
            };
            _context.TinNhans.Add(userMsg);

            // Nếu nhân viên trả lời khách vãng lai, gán tên nhân viên vào cuộc chat
            var chat = await _context.CuocTroChuyens.FindAsync(conversationId);
            if (role == "NhanVien" && chat != null && string.IsNullOrEmpty(chat.TenNhanVien))
            {
                chat.TenNhanVien = senderName;
                _context.Update(chat);
            }
            await _context.SaveChangesAsync();

            // 📡 Phát SignalR
            await _hubContext.Clients.Group(conversationId.ToString())
                .SendAsync("ReceiveMessage", conversationId.ToString(), role, senderName, message, timeNow);

            // 🤖 AI Gemini phản hồi nếu là khách (vãng lai hoặc đã login) nhắn và chưa có nhân viên
            if (role == "KhachHang" && chat != null && string.IsNullOrEmpty(chat.TenNhanVien))
            {
                string aiReply = await GetGeminiResponse(message);
                var aiMsg = new TinNhan
                {
                    CuocTroChuyenId = conversationId,
                    GuiBoiRole = "AI",
                    GuiBoiName = "SeaHotel AI",
                    NoiDung = aiReply,
                    ThoiGian = DateTime.UtcNow
                };
                _context.TinNhans.Add(aiMsg);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group(conversationId.ToString())
                    .SendAsync("ReceiveMessage", conversationId.ToString(), "AI", "SeaHotel AI", aiReply, DateTime.Now.ToString("HH:mm"));
            }

            return Json(new { success = true });
        }

        // --- 🗑️ XÓA TIN NHẮN CỦA MỘT CUỘC TRÒ CHUYỆN ---
        [HttpPost]
        public async Task<IActionResult> ClearConversation(int conversationId)
        {
            try
            {
                var messages = _context.TinNhans.Where(t => t.CuocTroChuyenId == conversationId);
                _context.TinNhans.RemoveRange(messages);
                await _context.SaveChangesAsync();

                // Thông báo qua SignalR để các bên xóa giao diện chat hiện tại
                await _hubContext.Clients.Group(conversationId.ToString()).SendAsync("ChatCleared", conversationId.ToString());

                return Json(new { success = true, message = "Đã xóa toàn bộ lịch sử tin nhắn." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa tin nhắn: " + ex.Message });
            }
        }

        // --- 🧹 DỌN DẸP TIN NHẮN CŨ (Dành cho Admin) ---
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteOldMessages(int days = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-days);
                var oldMessages = _context.TinNhans.Where(t => t.ThoiGian < cutoffDate);

                int count = await oldMessages.CountAsync();
                _context.TinNhans.RemoveRange(oldMessages);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Đã dọn dẹp {count} tin nhắn cũ hơn {days} ngày." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        private async Task<string> GetGeminiResponse(string prompt)
        {
            const string apiKey = "";
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-preview-09-2025:generateContent?key={apiKey}";
            string context = "Bạn là trợ lý ảo khách sạn SeaHotel. Trả lời ngắn gọn, lịch sự bằng tiếng Việt.";
            var payload = new { contents = new[] { new { parts = new[] { new { text = prompt } } } }, systemInstruction = new { parts = new[] { new { text = context } } } };
            try
            {
                var response = await _httpClient.PostAsync(url, new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    return doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? "...";
                }
            }
            catch { }
            return "Tôi đã nhận được tin nhắn. Nhân viên sẽ sớm hỗ trợ bạn!";
        }

        // Giữ nguyên các view quản lý cũ nếu cần
        [Authorize(Roles = "Admin,NhanVien")]
        public IActionResult DanhSachChat() => View();

        [HttpGet]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> GetConversations()
        {
            var chats = await _context.CuocTroChuyens
               .Select(c => new {
                   id = c.Id,
                   tenKhachHang = c.TenKhachHang.StartsWith("Guest_") ? "Khách vãng lai" : c.TenKhachHang,
                   lastMessage = c.TinNhans.OrderByDescending(t => t.ThoiGian).Select(t => t.NoiDung).FirstOrDefault(),
                   lastMessageTime = c.TinNhans.OrderByDescending(t => t.ThoiGian).Select(t => t.ThoiGian.ToLocalTime().ToString("HH:mm")).FirstOrDefault()
               })
               .OrderByDescending(c => c.id)
               .ToListAsync();
            return Json(chats);
        }
    }
}