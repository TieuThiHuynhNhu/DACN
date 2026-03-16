using DACN.Models;
using DACN.Models.MoMo;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using RestSharp;
using Newtonsoft.Json;

namespace DACN.Services.Momo
{
    public class MomoService : IMomoService
    {
        private readonly IOptions<MomoOptionModel> _options;
        public MomoService(IOptions<MomoOptionModel> options)
        {
            _options = options;
        }
        public async Task<MomoCreatePaymentResponseModel> CreatePaymentMoMo(OrderInfoModel model)
        {
            // Tạo orderId và requestId duy nhất
           
            var requestId = Guid.NewGuid().ToString();

            // Làm sạch thông tin mô tả (MoMo không chấp nhận ký tự đặc biệt)
            string sanitizedOrderInfo = string.IsNullOrEmpty(model.OrderInfo)
                ? ""
                : model.OrderInfo.Replace("&", " ").Replace("=", " ").Replace(":", " ").Replace(".", " ");

            model.OrderInfo = $"Khách hàng: {model.FullName}. Nội dung: {sanitizedOrderInfo}";

            // ✅ Đảm bảo amount là số nguyên không có dấu chấm
            int amount = (int)Math.Round(Convert.ToDouble(model.Amount));
            string amountString = amount.ToString();

            // ✅ Tạo rawData đúng thứ tự tài liệu MoMo
            var rawData =
    "accessKey=" + _options.Value.AccessKey +
    "&amount=" + amountString +
    "&extraData=" +
    "&ipnUrl=" + _options.Value.NotifyUrl +
    "&orderId=" + model.OrderId +
    "&orderInfo=" + model.OrderInfo +
    "&partnerCode=" + _options.Value.PartnerCode +
    "&redirectUrl=" + _options.Value.ReturnUrl +
    "&requestId=" + requestId +
    "&requestType=" + _options.Value.RequestType;


            // ✅ Tạo signature đúng chuẩn HMACSHA256
            var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);

            // ✅ Dùng cùng giá trị amount và orderInfo như rawData
            var requestBody = new
            {
                partnerCode = _options.Value.PartnerCode,
                partnerName = "MoMo Payment",
                storeId = "Hotel",
                requestId = requestId,
                amount = amountString,
                orderId = model.OrderId,
                orderInfo = model.OrderInfo,
                redirectUrl = _options.Value.ReturnUrl,
                ipnUrl = _options.Value.NotifyUrl,
                requestType = _options.Value.RequestType,
                extraData = "",
                lang = "vi",
                signature = signature
            };



            // === LOG ===
            Console.WriteLine("=== RAW DATA ===");
            Console.WriteLine(rawData);
            Console.WriteLine("=== SIGNATURE ===");
            Console.WriteLine(signature);
            Console.WriteLine("=== REQUEST BODY ===");
            Console.WriteLine(JsonConvert.SerializeObject(requestBody));

            // === GỬI YÊU CẦU ===
            using var http = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            var resp = await http.PostAsync(_options.Value.MomoApiUrl, content);
            var respContent = await resp.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(respContent))
                throw new Exception("Không nhận được phản hồi từ MoMo API.");

            var momoResponse = JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(respContent);

            if (momoResponse == null || string.IsNullOrEmpty(momoResponse.payUrl))
                throw new Exception($"Phản hồi MoMo không hợp lệ: {respContent}");

            return momoResponse;
        }



        public MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
        {
            var amount = collection.First(s => s.Key == "amount").Value;
            var orderInfo = collection.First(s => s.Key == "orderInfo").Value;
            var orderId = collection.First(s => s.Key == "orderId").Value;

            return new MomoExecuteResponseModel()
            {
                Amount = amount,
                OrderId = orderId,
                OrderInfo = orderInfo

            };
        }

        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            byte[] hashBytes;

            using (var hmac = new HMACSHA256(keyBytes))
            {
                hashBytes = hmac.ComputeHash(messageBytes);
            }

            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            return hashString;
        }


    }

}
