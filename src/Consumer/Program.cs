namespace Consumer
{
    using MassTransit;
    
    using System;
    using System.Threading.Tasks;
    using Serilog;
    
    using Shared;

    class Program
    {
        static void Main(string[] args)
        {
            var config = new LoggerConfiguration();

            Log.Logger = config.MinimumLevel.Verbose()
                .WriteTo.Console(outputTemplate: "{Level} {Message} {Exception} {NewLine}")
                .CreateLogger();
            
            var bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.UseSerilog();
                var host = cfg.Host(new Uri("rabbitmq://localhost"), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
                cfg.ReceiveEndpoint(host, "ping", ep =>
                {
                    ep.Handler<Ping>(context =>
                    {
                        Log.Information($"Received: {context.Message.Message}");
                        return Task.CompletedTask;
                    });
                });
            });
            
            bus.Start();
            Console.WriteLine("Press ctrl+c to exit");

            Console.CancelKeyPress += (sender, e) =>
            {
                bus.Stop();
            };
        }
    }
}
