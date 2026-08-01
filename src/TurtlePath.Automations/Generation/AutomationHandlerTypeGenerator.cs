namespace TurtlePath.Automations.Generation
{
    using DynaBee.FluentApi;
    using System.Reflection;
    using System.Reflection.Emit;
    using TurtlePath.Automations.Descriptors;

    internal sealed class AutomationHandlerTypeGenerator : IAutomationHandlerTypeGenerator
    {
        private readonly IAutomationHandlerBaseTypeResolver baseTypeResolver;
        private readonly IAutomationHandlerTypeNamePolicy typeNamePolicy;

        public AutomationHandlerTypeGenerator(
            IAutomationHandlerBaseTypeResolver baseTypeResolver,
            IAutomationHandlerTypeNamePolicy typeNamePolicy)
        {
            this.baseTypeResolver = baseTypeResolver ?? throw new ArgumentNullException(nameof(baseTypeResolver));
            this.typeNamePolicy = typeNamePolicy ?? throw new ArgumentNullException(nameof(typeNamePolicy));
        }

        public AutomationHandlerGenerationResult Generate(IReadOnlyCollection<AutomationDescriptor> descriptors)
        {
            if (descriptors == null)
                throw new ArgumentNullException(nameof(descriptors));

            var assemblyBuilder = DynaBeeBuilder
                .CreateAssembly("TurtlePath.Automations.Generated")
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
                        .Emits(il => EmitServiceProviderConstructor(il, baseType))));
            }

            var assemblyContext = assemblyBuilder.Build();
            var handlers = classNames.Select(pair => new AutomationGeneratedHandler(
                pair.Key,
                pair.Value,
                assemblyContext.GetClrType(pair.Value)));

            return new AutomationHandlerGenerationResult(handlers);
        }

        private static void EmitServiceProviderConstructor(ILGenerator il, Type baseType)
        {
            var baseConstructor = baseType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(IServiceProvider) },
                null);

            if (baseConstructor == null)
                throw new InvalidOperationException($"Handler base type '{baseType.FullName}' must expose a constructor that receives IServiceProvider.");

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, baseConstructor);
            il.Emit(OpCodes.Ret);
        }

    }
}
