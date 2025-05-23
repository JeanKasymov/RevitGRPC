using GrpcServerConsole.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RevitOutOfContext_gRPC_ProtosF;
namespace GrpcServerConsole
{
    class Program
    {
        static void Main()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddGrpc();
            builder.Services.AddGrpcClient<Greeter.GreeterClient>(o =>
            {
                o.Address = new Uri("https://localhost:5064");

            });
            builder.Services.AddGrpcHealthChecks()
                .AddCheck("Sample", () => HealthCheckResult.Healthy());
            var app = builder.Build();
            new Thread(() =>
            {
                app.UseRouting();
                app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
                // Configure the HTTP request pipeline.
                app.MapGrpcService<GreeterService>();
                app.MapGrpcHealthChecksService();
                app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. " +
                            "To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                app.Run();
            }).Start();

        }
    }
}