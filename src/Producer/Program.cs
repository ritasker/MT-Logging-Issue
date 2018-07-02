using System;

namespace Producer
{
    using System.Threading;
    using System.Threading.Tasks;
    using MassTransit;
    using Serilog;
    using Shared;

    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new LoggerConfiguration();

            Log.Logger = config.MinimumLevel.Verbose()
                .WriteTo.Console(outputTemplate: "{Level} {Message} {Exception} {NewLine}")
                .CreateLogger();
            
            var bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri("rabbitmq://localhost"), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
                cfg.UseSerilog();
            });
            
            var cancellationTokenSource = new CancellationTokenSource();            

            bus.Start();
            Console.WriteLine("Press ctrl+c to exit");

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                await bus.Publish<Ping>(new {Message = "Hello World!"}, cancellationTokenSource.Token);
                await Task.Delay(2500, cancellationTokenSource.Token);
            }

            Console.CancelKeyPress += (sender, e) =>
            {
                cancellationTokenSource.Cancel();
                bus.Stop();
            };
        }
    }
}
