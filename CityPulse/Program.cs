var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Domain services registration
builder.Services.AddSingleton<CityPulse.Services.Abstractions.IReferenceNumberService, CityPulse.Services.ReferenceNumberService>();
builder.Services.AddSingleton<CityPulse.Services.Abstractions.IStorageService, CityPulse.Services.LocalStorageService>();
builder.Services.AddScoped<CityPulse.Services.Abstractions.IIssueReportingService, CityPulse.Services.IssueReportingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
