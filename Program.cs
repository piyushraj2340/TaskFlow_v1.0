using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskMonitoringApp.Mappings;
using TaskMonitoringApp.Middleware;
using TaskMonitoringApp.Models.Business;
using TaskMonitoringApp.Models.Data;
using TaskMonitoringApp.Models.DataAccessLayer;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Repositories;
using TaskMonitoringApp.Models.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddScoped<IGoalServices, GoalServices>();
builder.Services.AddScoped<IGoalRepository, GoalRepository>();

builder.Services.AddScoped<ITaskServices, TasksServices>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();

builder.Services.AddScoped<ITodoServices, TodoServices>();
builder.Services.AddScoped<ITodoRepository, TodoRepository>();

builder.Services.AddScoped<INotesRepository, NotesRepository>();
builder.Services.AddScoped<INotesServices, NotesServices>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache(); // Enable In-Memory Caching

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("TaskMonitoringApplication")));

builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true; // Ensure email is unique
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    //options.ExpireTimeSpan = TimeSpan.FromMinutes(1);
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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Enable authentication middleware
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();  // API routes

app.Run();
