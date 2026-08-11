namespace TurtlePath.Automations.Descriptors
{
    using System.Linq.Expressions;

    /// <summary>
    /// Normalized automation metadata consumed by handler registration.
    /// </summary>
    internal sealed class AutomationDescriptor
    {
        public AutomationDescriptor(
            AutomationOperationKind operationKind,
            Type requestType,
            Type entityType,
            Type keyType,
            AutomationReturnMode returnMode,
            Type responseType = null,
            AutomationSourceKind sourceKind = AutomationSourceKind.Profile,
            LambdaExpression keySelector = null,
            string defaultSortProperty = null,
            string notFoundMessage = null,
            bool reloadBeforeResponse = false,
            IReadOnlyCollection<LambdaExpression> responseIncludeExpressions = null)
        {
            OperationKind = operationKind;
            RequestType = requestType ?? throw new ArgumentNullException(nameof(requestType));
            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
            KeyType = keyType ?? throw new ArgumentNullException(nameof(keyType));
            ReturnMode = returnMode;
            ResponseType = responseType;
            SourceKind = sourceKind;
            KeySelector = keySelector;
            DefaultSortProperty = defaultSortProperty;
            NotFoundMessage = notFoundMessage;
            ReloadBeforeResponse = reloadBeforeResponse || responseIncludeExpressions?.Count > 0;
            ResponseIncludeExpressions = responseIncludeExpressions ?? [];
        }

        public AutomationOperationKind OperationKind { get; }

        public Type RequestType { get; }

        public Type EntityType { get; }

        public Type KeyType { get; }

        public AutomationReturnMode ReturnMode { get; }

        public Type ResponseType { get; }

        public AutomationSourceKind SourceKind { get; }

        public LambdaExpression KeySelector { get; }

        public string DefaultSortProperty { get; }

        public string NotFoundMessage { get; }

        public bool ReloadBeforeResponse { get; }

        public IReadOnlyCollection<LambdaExpression> ResponseIncludeExpressions { get; }

        public bool HasResponse => ReturnMode == AutomationReturnMode.Response;

        internal AutomationDescriptorKey Key => new(RequestType, ReturnMode, ResponseType);

        internal int SourcePriority => SourceKind == AutomationSourceKind.Profile ? 2 : 1;

        internal bool IsEquivalentTo(AutomationDescriptor other)
        {
            if (other == null)
                return false;

            return OperationKind == other.OperationKind &&
                RequestType == other.RequestType &&
                EntityType == other.EntityType &&
                KeyType == other.KeyType &&
                ReturnMode == other.ReturnMode &&
                ResponseType == other.ResponseType;
        }
    }
}
