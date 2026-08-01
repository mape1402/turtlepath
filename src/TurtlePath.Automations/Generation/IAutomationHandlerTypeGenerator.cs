namespace TurtlePath.Automations.Generation
{
    using TurtlePath.Automations.Descriptors;

    internal interface IAutomationHandlerTypeGenerator
    {
        AutomationHandlerGenerationResult Generate(IReadOnlyCollection<AutomationDescriptor> descriptors);
    }
}
