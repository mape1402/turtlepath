namespace TurtlePath.Automations.Descriptors
{
    /// <summary>
    /// Thrown when two automation declarations cannot be registered for the same Pelican request contract.
    /// </summary>
    internal sealed class AutomationDescriptorConflictException : InvalidOperationException
    {
        public AutomationDescriptorConflictException(AutomationDescriptor current, AutomationDescriptor candidate)
            : base(CreateMessage(current, candidate))
        {
            Current = current ?? throw new ArgumentNullException(nameof(current));
            Candidate = candidate ?? throw new ArgumentNullException(nameof(candidate));
        }

        public AutomationDescriptor Current { get; }

        public AutomationDescriptor Candidate { get; }

        private static string CreateMessage(AutomationDescriptor current, AutomationDescriptor candidate)
        {
            if (current == null)
                throw new ArgumentNullException(nameof(current));

            if (candidate == null)
                throw new ArgumentNullException(nameof(candidate));

            var responseName = candidate.ResponseType == null ? "void" : candidate.ResponseType.FullName;
            return $"Automation for request '{candidate.RequestType.FullName}' and response '{responseName}' is already registered.";
        }
    }
}
