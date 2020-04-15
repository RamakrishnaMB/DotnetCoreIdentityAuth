using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetCoreIdentityAuthDemo.Models;
using DotnetCoreIdentityAuthDemo.Models.ExtendUser;
using DotnetCoreIdentityAuthDemo.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotnetCoreIdentityAuthDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ConnectToDB>(options => options.UseSqlServer(Configuration.GetConnectionString("DBConnection")));
            services.AddIdentity<CustomUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 3;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.SignIn.RequireConfirmedEmail = true;
                options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

            })
            .AddEntityFrameworkStores<ConnectToDB>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<CustomEmailConfirmationTokenProvider<CustomUser>>("CustomEmailConfirmation");
            //set all other type of token lifespan.
            services.Configure<DataProtectionTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromHours(5));
            //change email token expirty to 3 days.
            services.Configure<CustomEmailConfirmationTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromDays(3));


            services.AddMvc(options =>
               {
                   var pilicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                   options.Filters.Add(new AuthorizeFilter(pilicy));
               });
            services.AddControllersWithViews();

            services.AddAuthentication().AddGoogle(optns =>
            {
                optns.ClientId = "520343172682-qgp4tundmcnsq6p3u53n6fc152435v35.apps.googleusercontent.com";
                optns.ClientSecret = "N6Wok5HJ_N5AYuCesC0j0m_z";
            })
            .AddFacebook(options =>
            {
                options.AppId = "2579394669054559";
                options.AppSecret = "28363731d01355e6771e7f797dcfaaed";
            });



            services.AddAuthorization(options =>
            {

                options.AddPolicy("SuperAdminRolePolicy", policy => policy.RequireAssertion(context =>
                    (context.User.IsInRole("admin") &&
                    context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true")) ||
                    context.User.IsInRole("super admin")
                ));
                options.AddPolicy("DeleteClaimPolicy", policy => policy.RequireClaim("Delete Role", "true"));
                options.AddPolicy("ViewOnlyRolePolicy", policy => policy.RequireClaim("View Role", "true"));

                options.AddPolicy("CustomRolePolicy", policy => policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement()));
            });

            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherAdminRolesAndClaimsHandler>();
            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
