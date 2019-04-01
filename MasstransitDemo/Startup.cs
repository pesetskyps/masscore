using System;
using GreenPipes;
using MasstransitDemo.Services;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using IApplicationLifetime = Microsoft.AspNetCore.Hosting.IApplicationLifetime;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace MasstransitDemo
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
      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

      // Register MassTransit
      services.AddMassTransit(x =>
      {
        x.AddConsumer<SendMessageConsumer>();

        x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
          var host = cfg.Host(new Uri("rabbitmq://localhost"), h =>
         {
           h.Username("rabbitmq");
           h.Password("rabbitmq");
         });

          cfg.ReceiveEndpoint(host, "submit-order", ep =>
          {
            ep.PrefetchCount = 16;
            ep.UseMessageRetry(y => y.Interval(2, 100));

            ep.ConfigureConsumer<SendMessageConsumer>(provider);
          });
        }));
      });

      services.AddSingleton<IBus>(provider => provider.GetRequiredService<IBusControl>());
      services.AddSingleton<IHostedService, BusService>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
    {
      app.UseMvc(routes =>{routes.MapRoute(name: "default",template: "{controller=Home}/{action=Index}/{id?}");});
    }

    private IBusControl Register()
    {
      var bus = Bus.Factory.CreateUsingRabbitMq(sbc => { sbc.Host(new Uri("rabbitmq://localhost"), h => { }); });
      return bus;
    }
  }
}
