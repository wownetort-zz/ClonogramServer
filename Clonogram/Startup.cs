using System;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clonogram.Helpers;
using Clonogram.Repositories;
using Clonogram.Services;
using Clonogram.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Clonogram
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddMemoryCache();

            AddMapper(services);
            ConfigureSettings(services);
            AddServices(services);
            AddRepositories(services);
            ConfigureJWT(services);
        }

        private static void AddMapper(IServiceCollection services)
        {
            var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile(new AutoMapperProfile()); });
            var mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);
        }

        public void AddServices(IServiceCollection services)
        {
            services.AddSingleton<IUsersService, UsersService>();
            services.AddSingleton<ICryptographyService, CryptographyService>();
            services.AddSingleton<IPhotosService, PhotosService>();
            services.AddSingleton<IJWTService, JWTService>();
            services.AddSingleton<ICommentsService, CommentsService>();
            services.AddSingleton<IHashtagsService, HashtagsService>();
            services.AddSingleton<IStoriesService, StoriesService>();
            services.AddSingleton<IFeedService, FeedService>();
        }

        public void AddRepositories(IServiceCollection services)
        {
            services.AddSingleton<IUsersRepository, UsersRepository>();
            services.AddSingleton<IPhotosRepository, PhotosRepository>();
            services.AddSingleton<ICommentsRepository, CommentsRepository>();
            services.AddSingleton<IHashtagsRepository, HashtagsRepository>();
            services.AddSingleton<IStoriesRepository, StoriesRepository>();
            services.AddSingleton<IAmazonS3Repository, AmazonS3Repository>();
            services.AddSingleton<IRedisRepository, RedisRepository>();
        }

        public void ConfigureJWT(IServiceCollection services)
        {
            var key = Encoding.ASCII.GetBytes(Configuration.GetSection(nameof(JWTSettings)).GetSection("Secret").Value);
            services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            var userService = context.HttpContext.RequestServices.GetRequiredService<IUsersService>();
                            var userId = Guid.Parse(context.Principal.Identity.Name);
                            var user = userService.GetById(userId);
                            if (user == null)
                            {
                                // return unauthorized if user no longer exists
                                context.Fail("Unauthorized");
                            }
                            return Task.CompletedTask;
                        }
                    };
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });
        }

        public void ConfigureSettings(IServiceCollection services)
        {
            services.Configure<CacheSettings>(Configuration.GetSection(nameof(CacheSettings)));
            services.Configure<JWTSettings>(Configuration.GetSection(nameof(JWTSettings)));
            services.Configure<S3Settings>(Configuration.GetSection(nameof(S3Settings)));
            services.Configure<ConnectionStrings>(Configuration.GetSection(nameof(ConnectionStrings)));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseMiddleware(typeof(ErrorHandlingMiddleware));
            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}