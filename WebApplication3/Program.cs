using FluentAssertions.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using WebApplication3.Entities;
using WebApplication3.Helper;
using WebApplication3.Helper.Data;
using WebApplication3.repository.AccountRepository;
using WebApplication3.repository.AuthRepository;
using WebApplication3.repository.GroupsRepository;
using WebApplication3.repository.MemberRepository;
using WebApplication3.repository.SponsorRepository;
using WebApplication3.Service.AccountService;
using WebApplication3.Service.AuthService;
using WebApplication3.Service.GroupsService;
using WebApplication3.Service.MemberService;
using WebApplication3.Service.SponsorService;
using dotenv.net;
using WebApplication3.Service.Cloudinary_image;
using WebApplication3.Service.ProjectService;
using WebApplication3.repository.ProjectReposiotry;
using WebApplication3.Service.MomoService;
using WebApplication3.DTOs.Momo;
using WebApplication3.Service.BlogService;
using WebApplication3.repository.BlogRepository;
using WebApplication3.repository.CloudRepository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();


});
var connectionString = builder.Configuration.GetConnectionString("AppDbContext");
builder.Services.AddDbContext<Datacontext>(options =>
{
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)));
});
builder.Services.Configure<DbSettings>(builder.Configuration.GetSection("DbSettings"));
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));

// configure DI for application services
builder.Services.AddSingleton<AppDbContext>();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
    options =>
    {
        options.Password.RequiredUniqueChars = 0;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 1;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireLowercase = false;
    }).AddEntityFrameworkStores<Datacontext>().AddSignInManager()
    .AddRoles<IdentityRole>().AddDefaultTokenProviders(); 
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.IncludeErrorDetails = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {

        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IGroupsRepository, GroupsRepository>();
builder.Services.AddScoped<ISponsorRepository, SponsorRepository>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IClouRepository, CloudRepository>();
builder.Services.AddScoped<IBlogRepository, BlogRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IGroupsService, GroupsSevice>();
builder.Services.AddScoped<ISponsorService, SponsorService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IMomoService, MomoService>();
builder.Services.AddScoped<IBlogService, BlogService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin",
        builder =>
        {
            builder.WithOrigins("http://localhost:5173") // Thay đổi địa chỉ này thành địa chỉ của trang web của bạn
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials();
        });
});
builder.Services.AddAutoMapper(typeof(Program).Assembly);
/*builder.Services.AddIdentity<ApplicationUser,IdentityRole>()
    .AddEntityFrameworkStores<Datacontext>()
    .AddSignInManager()
    .AddRoles<IdentityRole>();*/

//JWT
builder.Services.AddAuthorization();
//builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
//.AddEntityFrameworkStores<Datacontext>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<WhitelistStorage>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.MapIdentityApi<ApplicationUser>();
app.UseCors("AllowOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.UseErrorHandlingMiddleware();
app.UseMiddleware<WhitelistLogoutMiddleware>();
app.MapControllers();
DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
app.Run();
