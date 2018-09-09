using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Matcha.Sync.Api;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using SampleApi.Models;

namespace SampleApi
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
            services.AddDbContext<TodoItemContext>(opt => opt.UseInMemoryDatabase("TodoItems"));
            services.AddOData();
            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc(b =>
            {
                //This will automatically register all OData derived from "BaseController"
                //to be registered
                b.MapODataServiceRouteBase("api", "api", builder =>
                {

                    //custom functions should ne registered here
                    builder.Function("GetSalesTaxRate")
                        .Returns<double>()
                        .Parameter<int>("PostalCode");
                });
            });
        }
    }
}
