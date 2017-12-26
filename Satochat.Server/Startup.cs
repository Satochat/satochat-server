using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Satochat.Server.Models;
using Satochat.Server.Filters;
using Satochat.Server.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Satochat.Shared.Event;
using System.Collections.Generic;
using System;

namespace Satochat.Server {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddResponseCompression(options => {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "text/event-stream" });
            });
            //services.AddServerSentEvents<INotificationsServerSentEventsService, NotificationsServerSentEventsService>();

            services.AddMvc(options => {
                options.Filters.Add<ServiceExceptionHandlingFilter>();
                //options.Filters.Add<RequireHttpsAttribute>();
                options.Filters.Add<AccessTokenFilter>();
                options.Filters.Add<ValidateModelFilter>();
            });
            //services.AddFluentValidation();
            //services.AddEntityFrameworkInMemoryDatabase();
            services.AddLogging();
            addDbContext(services);

            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEventService, EventService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, SatochatContext dbContext) {
            /*if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }*/

            //app.UseServiceExceptionHandling();
            //app.UseAuthentication();
            app.UseStaticFiles(new StaticFileOptions {
                OnPrepareResponse = context => {
                    if (context.Context.Request.Path.StartsWithSegments("/release")) {
                        var headers = context.Context.Response.Headers;
                        headers.Add("Cache-Control", "no-cache");
                    }
                }
            });
            app.UseMvc();
        }

        private void addDbContext(IServiceCollection services) {
            string connectionString = Environment.GetEnvironmentVariable("SATOCHAT_DB_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString)) {
                connectionString = Configuration.GetConnectionString("SatochatContext");
            }

            if (string.IsNullOrEmpty(connectionString)) {
                throw new InvalidOperationException("Cannot add database context when connection string is null/empty");
            }

            string databaseProviderName = Environment.GetEnvironmentVariable("SATOCHAT_DB_PROVIDER");
            if (string.IsNullOrEmpty(databaseProviderName)) {
                databaseProviderName = Configuration.GetValue<string>("DatabaseProvider");
            }

            if (string.IsNullOrEmpty(databaseProviderName)) {
                throw new InvalidOperationException("Cannot add database context when database provider is null/empty");
            }

            var factory = new Dictionary<string, Action<DbContextOptionsBuilder>> {
                { "sqlite", options => options.UseSqlite(connectionString) },
                { "mysql", options => options.UseMySql(connectionString) }
            };

            if (!factory.ContainsKey(databaseProviderName)) {
                throw new InvalidOperationException("Cannot add database context with unknown database provider");
            }

            var func = factory[databaseProviderName];
            services.AddDbContext<SatochatContext>(func);
        }
    }
}
