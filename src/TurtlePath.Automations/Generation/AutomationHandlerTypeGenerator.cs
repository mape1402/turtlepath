namespace TurtlePath.Automations.Generation
{
    using DynaBee.FluentApi;
    using System.Reflection;
    using System.Reflection.Emit;
    using TurtlePath.Automations.Descriptors;

    internal sealed class AutomationHandlerTypeGenerator
    {
        public IReadOnlyDictionary<AutomationDescriptor, Type> Generate(IReadOnlyCollection<AutomationDescriptor> descriptors)
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
                var baseType = AutomationHandlerRegistration.ResolveBaseHandlerType(descriptor);
                var className = CreateClassName(descriptor, ++index);
                classNames.Add(descriptor, className);

                assemblyBuilder.AddClass(className, generatedClass => generatedClass
                    .Inherits(baseType)
                    .AddConstructor(constructor => constructor
                        .WithParameter<IServiceProvider>("serviceProvider")
                        .Emits(il => EmitServiceProviderConstructor(il, baseType))));
            }

            var assemblyContext = assemblyBuilder.Build();
            return classNames.ToDictionary(pair => pair.Key, pair => assemblyContext.GetClrType(pair.Value));
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

        private static string CreateClassName(AutomationDescriptor descriptor, int index)
            => $"Generated{descriptor.OperationKind}Handler_{Sanitize(descriptor.RequestType.Name)}_{Sanitize(descriptor.EntityType.Name)}_{index}";

        private static string Sanitize(string value)
        {
            var chars = value.Select(character => char.IsLetterOrDigit(character) ? character : '_').ToArray();
            return new string(chars);
        }
    }
}
