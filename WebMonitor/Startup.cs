using System;
using System.Threading;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.DI.Core;
using Akka.DI.Ninject;
using LocationStatusViewer.Actors;
using LocationStatusViewer.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Host;
using Shared;
using Shared.Actors;
using Shared.Repos;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace LocationStatusViewer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
            services.AddSingleton<StatusHubHelper, StatusHubHelper>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseFileServer();


            app.UseSignalR(routes =>
            {
                routes.MapHub<StatusHub>("/statusHub");
            });

            try
            {
                SystemActors.ClusterSystem = SystemHostFactory.Launch();
                
                var pbm = PetabridgeCmd.Get(SystemActors.ClusterSystem);
                pbm.RegisterCommandPalette(ClusterCommands.Instance); // enable cluster management commands
                pbm.Start();


                SystemActors.Mediator = DistributedPubSub.Get(SystemActors.ClusterSystem).Mediator;
                SystemActors.SignalRActor = SystemActors.ClusterSystem.ActorOf(Props.Create(() => new SignalRActor()), "StatusActor");


                app.ApplicationServices.GetService<StatusHubHelper>().StartAsync(CancellationToken.None); //start Akka.NET
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
