using DACN.Hubs;
using DACN.Models;
using DACN.Models.MoMo;
using DACN.Repositories;
using DACN.Services;
using DACN.Services.Momo;
using DACN.Services.VNPay;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;
var builder = WebApplication.CreateBuilder(args);
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
// =============================
// 🔹 Kết nối Database
// =============================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// =============================
// 🔹 Cấu hình MoMo & VNPay
// =============================
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();

// =============================
// 🔹 SignalR & CORS (chat realtime)
// =============================
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(origin => true);
    });
});

// =============================
// 🔹 Repository
// =============================
builder.Services.AddScoped<IPhongRepository, PhongRepository>();
builder.Services.AddScoped<IDatPhongRepository, DatPhongRepository>();
builder.Services.AddScoped<IDichVuRepository, DichVuRepository>();
builder.Services.AddScoped<INhanVienRepository, NhanVienRepository>();
builder.Services.AddScoped<IThanhToanRepository, ThanhToanRepository>();
builder.Services.AddScoped<IKhachHangRepository, KhachHangRepository>();
builder.Services.AddScoped<IChamCongRepository, ChamCongRepository>();
builder.Services.AddScoped<IPhieuHuHaiRepository, PhieuHuHaiRepository>();
builder.Services.AddScoped<IDatDichVuRepository, DatDichVuRepository>();
builder.Services.AddScoped<IChiTietThanhToanRepository, ChiTietThanhToanRepository>();

builder.Services.AddScoped<IBaoCaoRepository, BaoCaoRepository>();
builder.Services.AddScoped<IKhuyenMaiRepository, KhuyenMaiRepository>();

// =============================
// 🔹 Session
// =============================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

// =============================
// 🔹 Authentication + Authorization
// =============================
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/TaiKhoan/DangNhap";
        options.AccessDeniedPath = "/TaiKhoan/KhongCoQuyen";
    });
builder.Services.AddAuthorization();

// =============================
// 🔹 MVC (Controllers + Views)
// =============================
builder.Services.AddControllersWithViews();
builder.Services.AddHostedService<EmailAnniversaryService>();
var app = builder.Build();

// =============================
// 🔹 Middleware Pipeline
// =============================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Đặt đúng vị trí (trước Auth)
app.UseCors("AllowAll");

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// =============================
// 🔹 Định tuyến
// =============================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
    );

// ✅ Đặt MapHub NGAY SAU route controller
app.MapHub<ChatHub>("/chatHub");

app.Run();
