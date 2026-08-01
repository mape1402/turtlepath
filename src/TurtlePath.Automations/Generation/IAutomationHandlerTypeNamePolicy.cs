namespace TurtlePath.Automations.Generation
{
    using TurtlePath.Automations.Descriptors;

    internal interface IAutomationHandlerTypeNamePolicy
    {
        string CreateName(AutomationDescriptor descriptor, int index);
    }
}
