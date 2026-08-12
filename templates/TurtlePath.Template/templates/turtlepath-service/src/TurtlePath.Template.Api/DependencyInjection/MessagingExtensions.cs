using TurtlePath.Template.Persistence;
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
            services.AddPigeon(configuration, builder =>
            {
                // Uncomment this line to scan for consumers in the current assembly:
                // builder.ScanConsumersFromAssemblies(typeof(Program).Assembly);

                builder.ConfigurePublishing(publishing =>
                {
                    publishing.AmbientTransactionBehavior = AmbientTransactionPublishBehavior.SuppressTransaction;
                });

                builder.UseAzureServiceBus();
                builder.UseEntityFrameworkOutbox<AppDbContext>(outbox =>
                {
                    outbox.Enabled = true;
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
            });

            return services;
        }
    }
}
