namespace TurtlePath.Automations.Generation
{
    using DynaBee.FluentApi;
    using DynaBee.FluentApi.DependencyInjection;
    using System.Reflection;
    using TurtlePath.Automations.Descriptors;

    internal sealed class AutomationHandlerTypeGenerator : IAutomationHandlerTypeGenerator
    {
        private readonly IDynaBeeAssemblyBuilderFactory assemblyBuilderFactory;
        private readonly AutomationHandlerGenerationOptions options;
        private readonly IAutomationHandlerBaseTypeResolver baseTypeResolver;
        private readonly IAutomationHandlerTypeNamePolicy typeNamePolicy;

        public AutomationHandlerTypeGenerator(
            IDynaBeeAssemblyBuilderFactory assemblyBuilderFactory,
            AutomationHandlerGenerationOptions options,
            IAutomationHandlerBaseTypeResolver baseTypeResolver,
            IAutomationHandlerTypeNamePolicy typeNamePolicy)
        {
            this.assemblyBuilderFactory = assemblyBuilderFactory ?? throw new ArgumentNullException(nameof(assemblyBuilderFactory));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.baseTypeResolver = baseTypeResolver ?? throw new ArgumentNullException(nameof(baseTypeResolver));
            this.typeNamePolicy = typeNamePolicy ?? throw new ArgumentNullException(nameof(typeNamePolicy));
        }

        public AutomationHandlerGenerationResult Generate(IReadOnlyCollection<AutomationDescriptor> descriptors)
        {
            if (descriptors == null)
                throw new ArgumentNullException(nameof(descriptors));

            var assemblyBuilder = assemblyBuilderFactory
                .Create(options.GeneratedAssemblyName)
                .WithVersion(Guid.NewGuid().ToString("N"));

            var classNames = new Dictionary<AutomationDescriptor, string>();
            var index = 0;

            foreach (var descriptor in descriptors)
            {
                var baseType = baseTypeResolver.Resolve(descriptor);
                var className = typeNamePolicy.CreateName(descriptor, ++index);
                classNames.Add(descriptor, className);

                assemblyBuilder.AddClass(className, generatedClass => generatedClass
                    .Inherits(baseType)
                    .AddConstructor(constructor => constructor
                        .WithParameter<IServiceProvider>("serviceProvider")
                        .CallsBase(GetServiceProviderConstructor(baseType), "serviceProvider")));
            }

            var assemblyContext = assemblyBuilder.Build();
            var handlers = classNames.Select(pair => new AutomationGeneratedHandler(
                pair.Key,
                pair.Value,
                assemblyContext.GetClrType(pair.Value)));

            return new AutomationHandlerGenerationResult(handlers);
        }

        private static ConstructorInfo GetServiceProviderConstructor(Type baseType)
        {
            var baseConstructor = baseType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(IServiceProvider) },
                null);

            if (baseConstructor == null)
                throw new InvalidOperationException($"Handler base type '{baseType.FullName}' must expose a constructor that receives IServiceProvider.");

            return baseConstructor;
        }
    }
}
