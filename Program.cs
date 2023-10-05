using Identity_Samples;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var connectionString = "Data Source=LAPTOP-VAHA5KRU\\sql_2022;Initial Catalog=Identity_Samples3;Persist Security Info=True;TrustServerCertificate=True;User ID=sa;Password=sql";
var migrationAssembly=typeof(Program).GetTypeInfo().Assembly.GetName().Name;
builder.Services.AddDbContext<UserDbContext>(
    opt => opt.UseSqlServer(connectionString,sql =>sql.MigrationsAssembly(migrationAssembly))) ;
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    //options.SignIn.RequireConfirmedEmail = true;
})
    .AddEntityFrameworkStores<UserDbContext>()
    .AddDefaultTokenProviders()
    .AddTokenProvider < EmailConfirmationTokenProvider <User>> ("emailconf");
builder.Services.AddScoped<IUserClaimsPrincipalFactory<User>,
    UserClaimPrincipalFactory>();
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
options.TokenLifespan = TimeSpan.FromHours(3));

//builder.Services.AddAuthentication("cookies").AddCookie(options => options.LoginPath="/Home/Login");
builder.Services.ConfigureApplicationCookie(options => options.LoginPath = "/Home/Login");
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
app.UseAuthentication();
app.UseAuthorization();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
