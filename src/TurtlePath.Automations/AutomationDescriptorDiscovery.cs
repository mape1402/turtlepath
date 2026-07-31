namespace TurtlePath.Automations
{
    using System.Reflection;
    using TurtlePath.Automations.Attributes;
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Automations.Profiles;

    internal static class AutomationDescriptorDiscovery
    {
        public static IReadOnlyCollection<AutomationDescriptor> Discover(params Assembly[] assemblies)
        {
            var registry = new AutomationDescriptorRegistry();

            foreach (var descriptor in AutomationAttributeDescriptorProvider.GetDescriptors(assemblies))
                registry.Add(descriptor);

            foreach (var profile in DiscoverProfiles(assemblies))
            {
                foreach (var descriptor in AutomationProfileDescriptorBuilder.Build(profile))
                    registry.Add(descriptor);
            }

            return registry.Descriptors;
        }

        private static IEnumerable<TurtlePathAutomationProfile> DiscoverProfiles(params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
                return [];

            return assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(TurtlePathAutomationProfile).IsAssignableFrom(type) && !type.IsAbstract)
                .Select(type => (TurtlePathAutomationProfile)Activator.CreateInstance(type, nonPublic: true));
        }
    }
}
