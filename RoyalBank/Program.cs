using Microsoft.EntityFrameworkCore;
using RoyalBank.Data;
using RoyalBank.Interfaces;
using RoyalBank.Repositories;
using RoyalBank.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<ICustomerRepository,   CustomerRepository>();
builder.Services.AddScoped<IUserRepository,       UserRepository>();
builder.Services.AddScoped<IKycRepository,        KycRepository>();
builder.Services.AddScoped<IRiskRepository,       RiskRepository>();
builder.Services.AddScoped<IAccountRepository,    AccountRepository>();
builder.Services.AddScoped<IComplianceRepository, ComplianceRepository>();

// Services
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<KycService>();
builder.Services.AddScoped<RiskService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<ComplianceService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<AuthService>();
// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly    = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name        = "RoyalBank.Session";
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name:    "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db);
}

app.Run();
