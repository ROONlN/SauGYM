using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using SauGYM.Services;
using SauGYM.Data;
using SauGYM.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı Bağlantısı
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// --- HATALI OLAN "AddDefaultIdentity" SATIRI SİLİNDİ ---

// 2. Identity (Üyelik) Ayarları (BİZİM YAZDIĞIMIZ KALIYOR)
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Şifre kuralları (Ödev için gevşek kurallar)
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3;

    // Hatalı girişte kilitleme ayarı
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false; // Mail onayı zorunluluğunu kaldırdık
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


// 3. MVC ve Razor Pages Servisleri
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // <-- BUNU EKLEDİK (Login sayfaları için şart)

builder.Services.AddSingleton<IEmailSender, EmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Önce Kimlik Doğrulama, Sonra Yetkilendirme
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Login/Register sayfalarının çalışması için bunu da ekliyoruz
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Az önce yazdığımız metodu çağırıyoruz
        await DbSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Seed hatası: " + ex.Message);
    }
}

app.Run();