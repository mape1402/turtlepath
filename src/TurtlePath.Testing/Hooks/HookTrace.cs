namespace TurtlePath.Testing.Hooks
{
    /// <summary>
    /// Captures TurtlePath hook stage executions for tests.
    /// </summary>
    public sealed class HookTrace
    {
        private readonly List<HookTraceEntry> entries = [];

        /// <summary>
        /// Gets captured hook entries.
        /// </summary>
        public IReadOnlyList<HookTraceEntry> Entries => entries;

        internal void Add(HookTraceEntry entry)
            => entries.Add(entry);

        /// <summary>
        /// Clears all captured hook entries.
        /// </summary>
        public void Clear()
            => entries.Clear();
    }
}
