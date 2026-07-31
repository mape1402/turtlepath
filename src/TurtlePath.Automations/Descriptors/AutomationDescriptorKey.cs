namespace TurtlePath.Automations.Descriptors
{
    internal readonly record struct AutomationDescriptorKey(
        Type RequestType,
        AutomationReturnMode ReturnMode,
        Type ResponseType);
}
