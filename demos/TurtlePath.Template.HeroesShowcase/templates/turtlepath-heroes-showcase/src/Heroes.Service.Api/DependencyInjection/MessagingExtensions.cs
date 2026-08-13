using Heroes.Service.Persistence;
using Pigeon.Messaging.Azure.ServiceBus;
using Pigeon.Messaging.Outbox;
using Pigeon.Messaging.Producing;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    internal static class MessagingExtensions
    {
        internal static IServiceCollection AddMessagingDefaults(this IServiceCollection services, IConfiguration configuration)
        {
            var serviceBusConnectionString = configuration["Pigeon:MessageBrokers:AzureServiceBus:ConnectionString"];
            var outboxEnabled = !string.Equals(
                configuration["Pigeon:Outbox:Enabled"],
                bool.FalseString,
                StringComparison.OrdinalIgnoreCase);

            services.AddPigeon(configuration, builder =>
            {
                // Uncomment this line to scan for consumers in the current assembly:
                // builder.ScanConsumersFromAssemblies(typeof(Program).Assembly);

                builder.ConfigurePublishing(publishing =>
                {
                    publishing.AmbientTransactionBehavior = AmbientTransactionPublishBehavior.SuppressTransaction;
                });

                // Keep local demo startup frictionless. Add a connection string to enable Service Bus transport.
                if (!string.IsNullOrWhiteSpace(serviceBusConnectionString))
                    builder.UseAzureServiceBus();

                if (outboxEnabled)
                {
                    builder.UseEntityFrameworkOutbox<AppDbContext>(outbox =>
                    {
                        outbox.Enabled = outboxEnabled;
                        outbox.SchemaMode = OutboxSchemaMode.AutoCreate;
                        outbox.DispatchInterval = TimeSpan.FromSeconds(5);
                        outbox.ImmediateDispatch = true;
                        outbox.DispatchQueueCapacity = 1000;
                        outbox.CleanInterval = TimeSpan.FromMinutes(10);
                        outbox.PublishedMessageRetention = TimeSpan.FromDays(1);
                        outbox.DispatchBatchSize = 50;
                        outbox.CleanBatchSize = 100;
                        outbox.MaxRetries = 10;
                        outbox.RetryDelay = TimeSpan.FromSeconds(15);
                        outbox.LockTimeout = TimeSpan.FromMinutes(2);

                        configuration.GetSection("Pigeon:Outbox").Bind(outbox);
                    });
                }
            });

            return services;
        }
    }
}
