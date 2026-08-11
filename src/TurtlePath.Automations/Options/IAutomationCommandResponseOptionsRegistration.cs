namespace TurtlePath.Automations.Options
{
    using Microsoft.Extensions.DependencyInjection;
    using TurtlePath.Automations.Descriptors;

    internal interface IAutomationCommandResponseOptionsRegistration
    {
        void Register(IServiceCollection services, IReadOnlyCollection<AutomationDescriptor> descriptors);
    }
}
