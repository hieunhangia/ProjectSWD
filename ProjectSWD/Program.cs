using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectSWD.Data;
using ProjectSWD.Data.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>().AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddScoped<ProjectSWD.Services.Customer.FeedbackService>();
builder.Services.AddScoped<ProjectSWD.Services.Customer.RefundService>();
builder.Services.AddScoped<ProjectSWD.Services.MockPaymentService>();
builder.Services.AddScoped<ProjectSWD.Services.Admin.PromotionService>();
builder.Services.AddScoped<ProjectSWD.Services.Admin.RefundService>();
builder.Services.AddScoped<ProjectSWD.Services.Customer.ProfileService>();
builder.Services.AddScoped<ProjectSWD.Services.Admin.RevenueService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.InitializeAsync(); // Comment lại nếu muốn chạy app mà không seed dữ liệu mẫu
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();