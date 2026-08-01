namespace TurtlePath.Automations
{
    using TurtlePath.Automations.Descriptors;

    internal interface IAutomationHandlerServiceTypeResolver
    {
        Type Resolve(AutomationDescriptor descriptor);
    }
}
