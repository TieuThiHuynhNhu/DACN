using Microsoft.AspNetCore.SignalR;
using DACN.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DACN.Hubs
{
    // DTO Cảnh báo An ninh dùng để truyền dữ liệu từ AI sang Dashboard
    public class SecurityAlert
    {
        public string EventType { get; set; }
        public string Location { get; set; }
        public string CameraId { get; set; }
        public string SnapshotUrl { get; set; }
        public string Timestamp { get; set; }
    }

    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        // ======================================================
        // LOGIC CHAT THỜI GIAN THỰC
        // ======================================================

        // Khi người dùng mở trang chat, họ join vào Group riêng của cuộc trò chuyện đó
        public async Task JoinConversation(string conversationId)
        {
            if (!string.IsNullOrEmpty(conversationId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
            }
        }

        // ⭐ MỚI: Nhân viên join vào nhóm hỗ trợ chung để nhận thông báo từ mọi khách hàng
        public async Task JoinStaffGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "StaffGroup");
        }

        // Phương thức gửi tin nhắn trực tiếp qua Hub
        public async Task SendMessageToConversation(
            string conversationId,
            string guiBoiRole,
            string guiBoiName,
            string noiDung,
            string thoiGian)
        {
            if (string.IsNullOrWhiteSpace(noiDung) || string.IsNullOrWhiteSpace(conversationId)) return;

            // 💾 Lưu tin nhắn vào Database
            if (int.TryParse(conversationId, out int convoId))
            {
                var tin = new TinNhan
                {
                    CuocTroChuyenId = convoId,
                    GuiBoiRole = guiBoiRole,
                    GuiBoiName = guiBoiName,
                    NoiDung = noiDung,
                    ThoiGian = DateTime.UtcNow
                };
                _context.TinNhans.Add(tin);

                if (guiBoiRole == "NhanVien" || guiBoiRole == "Admin")
                {
                    var convo = await _context.CuocTroChuyens.FindAsync(convoId);
                    if (convo != null && convo.TenNhanVien == null)
                    {
                        convo.TenNhanVien = guiBoiName;
                    }
                }
                await _context.SaveChangesAsync();
            }

            // 1. Gửi tin nhắn đến mọi người trong phòng chat hiện tại
            await Clients.Group(conversationId).SendAsync(
                "ReceiveMessage",
                conversationId,
                guiBoiRole,
                guiBoiName,
                noiDung,
                thoiGian
            );

            // 2. ⭐ MỚI: Nếu là Khách hàng gửi, thông báo cho toàn bộ Nhân viên để họ biết có tin nhắn mới cần hỗ trợ
            if (guiBoiRole == "KhachHang")
            {
                await Clients.Group("StaffGroup").SendAsync("NewMessageAlert", conversationId, guiBoiName, noiDung);
            }
        }

        // ======================================================
        // LOGIC AN NINH AI
        // ======================================================

        // 1. Gửi cảnh báo sự cố từ Camera AI đến Dashboard nhân viên
        public async Task SendAlert(SecurityAlert alert)
        {
            if (string.IsNullOrEmpty(alert.Timestamp))
                alert.Timestamp = DateTime.Now.ToString("HH:mm:ss");

            // Phát sóng cảnh báo đến TẤT CẢ nhân viên
            await Clients.All.SendAsync("ReceiveAlert", alert);
            System.Diagnostics.Debug.WriteLine($"[SignalR] Cảnh báo phát đi: {alert.EventType}");
        }

        // 2. Xác nhận nhân viên đã xử lý sự cố
        public async Task AcknowledgeAlert(string alertId)
        {
            var userId = Context.ConnectionId;
            await Clients.All.SendAsync("ReceiveAcknowledgement", alertId, userId);
        }
    }
}