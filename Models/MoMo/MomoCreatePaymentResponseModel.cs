namespace DACN.Models.MoMo
{
    public class MomoCreatePaymentResponseModel
    {
        public int resultCode { get; set; }
        public string message { get; set; }
        public string payUrl { get; set; }
        public string deeplink { get; set; }
        public string qrCodeUrl { get; set; }
        public string orderId { get; set; }
        public string requestId { get; set; }
    }
}
