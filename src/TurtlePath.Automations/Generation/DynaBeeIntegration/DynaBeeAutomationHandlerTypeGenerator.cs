namespace TurtlePath.Automations.Generation.DynaBeeIntegration
{
    using DynaBee.FluentApi;
    using DynaBee.FluentApi.DependencyInjection;
    using System.Reflection;
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Automations.Generation;
    using TurtlePath.Mapping;
    using TurtlePath.Queries;

    internal sealed class DynaBeeAutomationHandlerTypeGenerator : IAutomationHandlerTypeGenerator
    {
        private readonly IDynaBeeAssemblyBuilderFactory assemblyBuilderFactory;
        private readonly AutomationHandlerGenerationOptions options;
        private readonly IAutomationHandlerBaseTypeResolver baseTypeResolver;
        private readonly IAutomationHandlerTypeNamePolicy typeNamePolicy;

        public DynaBeeAutomationHandlerTypeGenerator(
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

                assemblyBuilder.AddClass(className, generatedClass => ConfigureGeneratedClass(generatedClass, descriptor, baseType));
            }

            var assemblyContext = assemblyBuilder.Build();
            var handlers = classNames.Select(pair => new AutomationGeneratedHandler(
                pair.Key,
                pair.Value,
                assemblyContext.GetClrType(pair.Value)));

            return new AutomationHandlerGenerationResult(handlers);
        }

        private static void ConfigureGeneratedClass(BeeClassBuilder generatedClass, AutomationDescriptor descriptor, Type baseType)
        {
            generatedClass
                .Inherits(baseType)
                .AddConstructor(constructor => constructor
                    .WithParameter<IServiceProvider>("serviceProvider")
                    .CallsBase(GetServiceProviderConstructor(baseType), "serviceProvider"));

            if (descriptor.OperationKind == AutomationOperationKind.Delete && descriptor.HasResponse)
                OverrideBuildResponse(generatedClass, descriptor, baseType);

            if (descriptor.OperationKind == AutomationOperationKind.GetOne)
                OverrideGetOneFilter(generatedClass, descriptor, baseType);

            if (descriptor.OperationKind == AutomationOperationKind.GetPaged)
                OverrideDefaultSorts(generatedClass, descriptor, baseType);
        }

        private static void OverrideBuildResponse(BeeClassBuilder generatedClass, AutomationDescriptor descriptor, Type baseType)
        {
            var method = GetVirtualMethod(
                baseType,
                "BuildResponseAsync",
                descriptor.RequestType,
                descriptor.EntityType,
                typeof(CancellationToken));
            var mapMethod = typeof(IMapperAdapter)
                .GetMethod(nameof(IMapperAdapter.MapAsync))
                ?.MakeGenericMethod(descriptor.EntityType, descriptor.ResponseType);

            if (mapMethod == null)
                throw new InvalidOperationException($"Mapper adapter method '{nameof(IMapperAdapter.MapAsync)}' could not be resolved.");

            generatedClass.OverrideMethod(method, methodBuilder => methodBuilder.EmitsBody(body =>
                body.Return(body.Call(
                    body.Property(body.Self(), "MapperAdapter"),
                    mapMethod,
                    body.Parameter("entity"),
                    body.Parameter<CancellationToken>("cancellationToken")))));
        }

        private static void OverrideGetOneFilter(BeeClassBuilder generatedClass, AutomationDescriptor descriptor, Type baseType)
        {
            var method = GetVirtualMethod(baseType, "GetFilterExpression", descriptor.RequestType);
            var optionsType = typeof(IGetOneQueryOptions<,>).MakeGenericType(descriptor.RequestType, descriptor.EntityType);
            var getFilterMethod = optionsType.GetMethod(nameof(IGetOneQueryOptions<object, object>.GetFilterExpression));

            if (getFilterMethod == null)
                throw new InvalidOperationException($"Query options method '{nameof(IGetOneQueryOptions<object, object>.GetFilterExpression)}' could not be resolved.");

            generatedClass.OverrideMethod(method, methodBuilder => methodBuilder.EmitsBody(body =>
                body.Return(body.Call(
                    body.Property(body.Self(), "QueryOptions"),
                    getFilterMethod,
                    body.Parameter("request")))));
        }

        private static void OverrideDefaultSorts(BeeClassBuilder generatedClass, AutomationDescriptor descriptor, Type baseType)
        {
            var property = GetVirtualProperty(baseType, "DefaultSorts");
            var optionsType = typeof(IGetPagedInfoQueryOptions<,>).MakeGenericType(descriptor.RequestType, descriptor.EntityType);
            var getDefaultSorts = optionsType.GetProperty(nameof(IGetPagedInfoQueryOptions<object, object>.DefaultSorts))?.GetMethod;

            if (getDefaultSorts == null)
                throw new InvalidOperationException($"Query options property '{nameof(IGetPagedInfoQueryOptions<object, object>.DefaultSorts)}' could not be resolved.");

            generatedClass.OverrideProperty(property, propertyBuilder => propertyBuilder.Getter(getter => getter.EmitsBody(body =>
                body.Return(body.Call(
                    body.Property(body.Self(), "QueryOptions"),
                    getDefaultSorts)))));
        }

        private static MethodInfo GetVirtualMethod(Type baseType, string name, params Type[] parameterTypes)
        {
            var method = baseType.GetMethod(
                name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                parameterTypes,
                null);

            if (method == null || !method.IsVirtual)
                throw new InvalidOperationException($"Handler base type '{baseType.FullName}' must expose a virtual '{name}' method.");

            return method;
        }

        private static PropertyInfo GetVirtualProperty(Type baseType, string name)
        {
            var property = baseType.GetProperty(
                name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (property?.GetMethod == null || !property.GetMethod.IsVirtual)
                throw new InvalidOperationException($"Handler base type '{baseType.FullName}' must expose a virtual '{name}' property.");

            return property;
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
