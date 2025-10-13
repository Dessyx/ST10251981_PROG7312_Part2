var builder = WebApplication.CreateBuilder(args);

// Services to the container.
builder.Services.AddControllersWithViews();

// Domain services registration
builder.Services.AddSingleton<CityPulse.Services.Abstractions.IReferenceNumberService, CityPulse.Services.ReferenceNumberService>();
builder.Services.AddSingleton<CityPulse.Services.Abstractions.IStorageService, CityPulse.Services.LocalStorageService>();
builder.Services.AddScoped<CityPulse.Services.Abstractions.IIssueReportingService, CityPulse.Services.IssueReportingService>();
builder.Services.AddSingleton<CityPulse.Services.Abstractions.IAnnouncementService, CityPulse.Services.AnnouncementService>();

// Session support for admin authentication
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

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

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
