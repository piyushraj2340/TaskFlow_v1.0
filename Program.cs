using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TaskMonitoringApp.Mappings;
using TaskMonitoringApp.Middleware;
using TaskMonitoringApp.Models.Business;
using TaskMonitoringApp.Models.Data;
using TaskMonitoringApp.Models.DataAccessLayer;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Repositories;
using TaskMonitoringApp.Models.Services;
using TaskMonitoringApp.Repositories;
using TaskMonitoringApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddScoped<IGoalServices, GoalServices>();
builder.Services.AddScoped<IGoalRepository, GoalRepository>();

builder.Services.AddScoped<ITaskServices, TasksServices>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();

builder.Services.AddScoped<ITodoServices, TodoServices>();
builder.Services.AddScoped<ITodoRepository, TodoRepository>();

builder.Services.AddScoped<INotesRepository, NotesRepository>();
builder.Services.AddScoped<INotesServices, NotesServices>();

builder.Services.AddScoped<ITaskSearchRepository, TaskSearchRepository>();
builder.Services.AddScoped<ITaskSearchService, TaskSearchService>();

builder.Services.AddScoped<ICollectionRepository, CollectionRepository>();
builder.Services.AddScoped<ICollectionService, CollectionService>();

builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IItemService, ItemService>();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddSingleton<IEmailService, LoggingEmailService>();
builder.Services.AddScoped<IGuestSeederService, GuestSeederService>();
builder.Services.AddHostedService<GuestCleanupService>();
builder.Services.AddHostedService<DbKeepAliveService>();

// register search services
builder.Services.AddScoped<ISearchRepository, TaskMonitoringApp.Models.DataAccessLayer.SearchRepository>();
builder.Services.AddScoped<ISearchServices, TaskMonitoringApp.Models.Business.SearchServices>();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddMemoryCache(); // Enable In-Memory Caching

// UPDATED: Use "DefaultConnection". 
// In Development, this pulls from appsettings.Development.json ((localdb)).
// In Production, this pulls from appsettings.json (Azure DB).
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 6,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        )
    ));


builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 👇 Add JWT only — DO NOT re-add cookie here
builder.Services.AddAuthentication()
    .AddJwtBearer("JwtBearer", options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]))
        };
    });

builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    //options.ExpireTimeSpan = TimeSpan.FromMinutes(1);

    options.Events.OnRedirectToLogin = context =>
    {
        var isApiOrAjax = context.Request.Path.StartsWithSegments("/api") ||
                          context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                          context.Request.Headers["Accept"].ToString().Contains("application/json");

        if (isApiOrAjax)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsJsonAsync(new { StatusCodes = 401, message = "Unauthorized access." });
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        var isApiOrAjax = context.Request.Path.StartsWithSegments("/api") ||
                          context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                          context.Request.Headers["Accept"].ToString().Contains("application/json");

        if (isApiOrAjax)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsJsonAsync(new { StatusCodes = 403, message = "Access denied (Forbidden)." });
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddAutoMapper(typeof(GoalProfile));
builder.Services.AddAutoMapper(typeof(TaskProfile));
builder.Services.AddAutoMapper(typeof(TodoProfile));
builder.Services.AddAutoMapper(typeof(NotesProfile));

// --- IIS (if hosting behind IIS) ---
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = long.MaxValue; // practically unlimited
});

// --- Kestrel server ---
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = null; // null = unlimited
});


builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = long.MaxValue; // unlimited
});


var app = builder.Build();

// Use custom exception handling middleware for Web API and MVC
app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/api"),
    appBuilder => appBuilder.UseMiddleware<ApiExceptionMiddleware>() // Web API Middleware
);

app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/api"),
    appBuilder => appBuilder.UseMiddleware<MvcExceptionMiddleware>() // MVC Middleware
);

app.Use(async (context, next) =>
{
    var feature = context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpMaxRequestBodySizeFeature>();
    if (feature != null)
    {
        feature.MaxRequestBodySize = null; // remove request size limit
    }
    await next.Invoke();
});


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePages(async statusCodeContext =>
{
    var request = statusCodeContext.HttpContext.Request;
    var response = statusCodeContext.HttpContext.Response;

    var isApiOrAjax = request.Path.StartsWithSegments("/api") ||
                      request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                      request.Headers["Accept"].ToString().Contains("application/json");

    if (isApiOrAjax)
    {
        if (response.StatusCode >= 400 && !response.HasStarted)
        {
            response.ContentType = "application/json";
            var result = new
            {
                StatusCodes = response.StatusCode,
                message = response.StatusCode switch
                {
                    401 => "Unauthorized access",
                    403 => "Forbidden access",
                    404 => "Resource not found",
                    400 => "Bad request",
                    _ => "An error occurred"
                },
                Details = (string)null
            };
            await response.WriteAsJsonAsync(result);
        }
    }
    else
    {
        var message = response.StatusCode switch
        {
            404 => "Page or resource not found.",
            401 => "Unauthorized access.",
            403 => "You do not have permission to access this resource.",
            _ => "An unexpected error occurred."
        };
        response.Redirect($"/Error?statusCode={response.StatusCode}&message={System.Net.WebUtility.UrlEncode(message)}");
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
// Use CORS policy
app.UseCors("AllowAll");

app.UseAuthentication(); // Enable authentication middleware
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();  // API routes

try
{
    Log.Information("Applying Database Migrations");
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
    }

    Log.Information("Application Starting Up");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "The application failed to start correctly");
}
finally
{
    Log.CloseAndFlush();
}
