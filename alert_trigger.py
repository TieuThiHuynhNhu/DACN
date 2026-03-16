import requests
import json
import time
import sys
import urllib3 # Thư viện cần thiết để quản lý cảnh báo SSL

# Tắt cảnh báo khi dùng verify=False (chỉ dùng cho môi trường dev/localhost)
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

# =========================================================================
# 1. CẤU HÌNH API
# =========================================================================
# LƯU Ý: THAY THẾ <PORT_CUA_BAN> BẰNG PORT CHÍNH XÁC CỦA BẠN (Ví dụ: 7049, 5001)
API_URL = "https://localhost:7049/api/Security/TriggerAlert" 
MAX_RETRIES = 3 # Số lần thử lại tối đa

# =========================================================================
# 2. HÀM GỬI CẢNH BÁO
# =========================================================================

def send_security_alert(event_type: str, location: str, camera_id: str, snapshot_url: str = None) -> bool:
    """
    Gửi yêu cầu HTTP POST đến API C# để kích hoạt cảnh báo SignalR.
    
    Hàm này được gọi bởi logic AI khi phát hiện sự cố.
    """
    
    # Payload phải KHỚP CHÍNH XÁC với AlertRequest DTO trong SecurityController.cs
    payload = {
        "EventType": event_type,
        "Location": location,
        "CameraId": camera_id,
        "SnapshotUrl": snapshot_url if snapshot_url else "https://placehold.co/400x200/cccccc/333333?text=NO+IMAGE"
    }
    
    headers = {
        "Content-Type": "application/json"
    }

    print(f"--- Đang cố gắng kích hoạt Cảnh báo: {event_type} ---")
    
    # Thực hiện gọi API với cơ chế thử lại (retry logic)
    for attempt in range(MAX_RETRIES):
        try:
            # 'verify=False' bỏ qua lỗi chứng chỉ SSL tự ký của localhost
            response = requests.post(API_URL, headers=headers, data=json.dumps(payload), verify=False)
            
            if response.status_code == 200:
                print(f">>> [THÀNH CÔNG] Đã gửi cảnh báo qua API. Lần thử: {attempt + 1}")
                return True
            else:
                # Xử lý các mã lỗi HTTP khác 200
                print(f"    [LỖI API] HTTP {response.status_code}. Nội dung lỗi: {response.text[:100]}. Thử lại sau 2 giây...")
                time.sleep(2) 
        
        except requests.exceptions.RequestException as e:
            # Xử lý lỗi kết nối (ví dụ: server C# chưa chạy)
            print(f"    [LỖI KẾT NỐI] {e}. Thử lại sau 2 giây...")
            time.sleep(2) 

    print("!!! [THẤT BẠI] Không thể gửi cảnh báo sau nhiều lần thử. Vui lòng kiểm tra Server C# và URL.")
    return False

# =========================================================================
# 3. VÍ DỤ CHẠY THỬ
#    Phần này sẽ chạy khi bạn thực thi file bằng lệnh `python alert_trigger.py`
# =========================================================================

if __name__ == "__main__":
    # --- BẮT ĐẦU CHẠY THỬ ---
    print("--- CHẠY THỬ NGHIỆM TÍNH NĂNG GỬI CẢNH BÁO ---")
    
    # Mô phỏng sự cố: Phát hiện hành vi đánh nhau
    success = send_security_alert(
        event_type="Đánh nhau/Bạo lực",
        location="Hành lang tầng 2",
        camera_id="HAL-2C-C05",
        snapshot_url="https://placehold.co/800x400/ff0000/ffffff?text=FIGHTING+DETECTED"
    )
    
    if success:
        print("\nKiểm tra trình duyệt: Cảnh báo phải xuất hiện ngay lập tức trên Dashboard Nhân viên.")
    else:
        print("\nThử nghiệm thất bại. Kiểm tra lại API_URL và trạng thái Server C#.")