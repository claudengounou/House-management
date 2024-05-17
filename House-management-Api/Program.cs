using House_management_Api.Data;
using House_management_Api.Models;
using House_management_Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<Context_identity>(options => 
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Identity_Connection"));
} );


//be able to inject Service iniside our controller
builder.Services.AddScoped<JWTService>();
builder.Services.AddScoped<EmailService>();

//Defining our identityCore service
builder.Services.AddIdentityCore<User>(options =>
{

    //Password configuration
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;

    //For email confirmation
    options.SignIn.RequireConfirmedEmail = true;

})
    .AddRoles<IdentityRole>() //be able to add roles
    .AddRoleManager<RoleManager<IdentityRole>>() //be able to make use of RoleManager
    .AddEntityFrameworkStores<Context_identity>() //providing our context
    .AddSignInManager<SignInManager<User>>() //be able to make use of SigninManager
    .AddUserManager<UserManager<User>>() //make use of UserManager to create users
    .AddDefaultTokenProviders(); //be able to creta token for email confirmation


//be able to authenticate user using JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            //Validate token based on the key we have provided in appsettings.development JWT:Key
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
            //the issuer which in here is the api project we are using 
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            //validate the issuer (who ever is issuing the jwt)
            ValidateIssuer = true,
            //don't validate audience (angular side)
            ValidateAudience = false,
        };
    });

builder.Services.AddCors();

//permet de renvoyer les erreurs de validation des champs sous forme de tableau
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var errors = actionContext.ModelState
            .Where(x => x.Value.Errors.Count() > 0)
            .SelectMany(x => x.Value.Errors)
            .Select(x => x.ErrorMessage).ToArray();

        var toReturn = new
        {
            errors = errors
        };

        return new BadRequestObjectResult (toReturn);
    };
});

var app = builder.Build();

app.UseCors(opt =>
{
    opt.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(builder.Configuration["JWT:ClientUrl"]);
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//adding UseAuthentication into our pipeline and this should come before UseAuthorization
//Authentication verifies the identity of a user or service and authorization determines their access right
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
