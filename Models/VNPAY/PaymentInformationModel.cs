namespace DACN.Models.VNPAY
{
    public class PaymentInformationModel
    {
        public int OrderId { get; set; } // 🔹 Mã đặt phòng
        public string OrderType { get; set; }
        public double Amount { get; set; }
        public string OrderDescription { get; set; }
        public string Name { get; set; }
        public string ReturnUrl { get; set; } // Nếu cần custom return
    }
}
