namespace TurtlePath.Automations.Options
{
    using Microsoft.Extensions.DependencyInjection;
    using TurtlePath.Automations.Descriptors;

    internal interface IAutomationQueryOptionsRegistration
    {
        void Register(IServiceCollection services, IReadOnlyCollection<AutomationDescriptor> descriptors);
    }
}
