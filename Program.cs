using System.Net.Http.Headers;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// Obtener el puerto de la variable de entorno PORT (para Railway)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5270";
builder.WebHost.UseUrls($"http://*:{port}");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

// Handler que inyecta el Bearer token desde Session (solo si haces login y usas token)
builder.Services.AddTransient<AuthTokenHandler>();

// HttpClient “limpio” para login/u otras llamadas sin token
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["API_BASE_URL"]!);
});

// HttpClient con token automático (añade Authorization: Bearer {token})
builder.Services.AddHttpClient("ApiClientWithToken", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["API_BASE_URL"]!);
})
.AddHttpMessageHandler<AuthTokenHandler>();


builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<TokenCheckFilter>();
});

var defaultCulture = new CultureInfo("es-UY");
var localizationOptions = new RequestLocalizationOptions {
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture }
};
builder.Services.AddScoped<TokenCheckFilter>();

var app = builder.Build();

app.UseRequestLocalization(localizationOptions);

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
// Para ver la traza completa del 500 en el navegador
app.UseDeveloperExceptionPage();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();

public class AuthTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _ctx;
    public AuthTokenHandler(IHttpContextAccessor ctx) => _ctx = ctx;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _ctx.HttpContext?.Session.GetString("JWToken");
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}