namespace TurtlePath.Automations.Generation
{
    using TurtlePath.Automations.Descriptors;

    internal interface IAutomationHandlerBaseTypeResolver
    {
        Type Resolve(AutomationDescriptor descriptor);
    }
}
