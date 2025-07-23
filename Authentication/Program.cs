using Microsoft.AspNetCore.Authentication.JwtBearer;
using Game.Authentication.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace Game.Authentication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSingleton<IUserSessionRepository, InMemoryUserSessionRepository>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddControllers();

            // JWT ����
            builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = JwtUtils.Issuer,
                    ValidateAudience = true,
                    ValidAudience = JwtUtils.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = JwtUtils.SymKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapGet("/", () => Results.Ok(new { status = "Auth service is running" }));
            app.Run();
        }
    }
}
