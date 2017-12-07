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
            services.AddDbContext<SatochatContext>(options =>
                    options.UseMySql(Configuration.GetConnectionString("SatochatContextMySQL")));

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
    }
}
