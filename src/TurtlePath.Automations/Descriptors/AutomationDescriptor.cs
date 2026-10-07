namespace TurtlePath.Automations.Descriptors
{
    using System.Linq.Expressions;

    /// <summary>
    /// Normalized automation metadata consumed by handler registration.
    /// </summary>
    public sealed class AutomationDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationDescriptor"/> class.
        /// </summary>
        /// <param name="operationKind">The automated operation kind.</param>
        /// <param name="requestType">The request type handled by the operation.</param>
        /// <param name="entityType">The entity type handled by the operation.</param>
        /// <param name="keyType">The entity key type.</param>
        /// <param name="returnMode">Whether the operation returns a response.</param>
        /// <param name="responseType">The response type returned by the operation.</param>
        /// <param name="sourceKind">The declaration source.</param>
        /// <param name="keySelector">The optional request key selector.</param>
        /// <param name="defaultSortProperty">The optional default sort expression.</param>
        /// <param name="notFoundMessage">The optional not-found message.</param>
        /// <param name="validateRequest">The optional validation override.</param>
        /// <param name="reloadBeforeResponse">Whether response mapping should reload the saved entity from storage.</param>
        /// <param name="responseIncludeExpressions">Navigation expressions used when reloading the response projection.</param>
        /// <param name="handlerType">An optional explicit handler implementation type.</param>
        /// <param name="metadata">Integration metadata owned by extension packages.</param>
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
            bool? validateRequest = null,
            bool reloadBeforeResponse = false,
            IReadOnlyCollection<LambdaExpression> responseIncludeExpressions = null,
            Type handlerType = null,
            IReadOnlyDictionary<string, object> metadata = null)
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
            ValidateRequest = validateRequest;
            ReloadBeforeResponse = reloadBeforeResponse;
            ResponseIncludeExpressions = responseIncludeExpressions ?? [];
            HandlerType = handlerType;
            Metadata = metadata ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets the automated operation kind.
        /// </summary>
        public AutomationOperationKind OperationKind { get; }

        /// <summary>
        /// Gets the request type handled by the operation.
        /// </summary>
        public Type RequestType { get; }

        /// <summary>
        /// Gets the entity type handled by the operation.
        /// </summary>
        public Type EntityType { get; }

        /// <summary>
        /// Gets the entity key type.
        /// </summary>
        public Type KeyType { get; }

        /// <summary>
        /// Gets whether the operation returns a response.
        /// </summary>
        public AutomationReturnMode ReturnMode { get; }

        /// <summary>
        /// Gets the response type returned by the operation.
        /// </summary>
        public Type ResponseType { get; }

        /// <summary>
        /// Gets the declaration source.
        /// </summary>
        public AutomationSourceKind SourceKind { get; }

        /// <summary>
        /// Gets the optional request key selector.
        /// </summary>
        public LambdaExpression KeySelector { get; }

        /// <summary>
        /// Gets the optional default sort expression.
        /// </summary>
        public string DefaultSortProperty { get; }

        /// <summary>
        /// Gets the optional not-found message.
        /// </summary>
        public string NotFoundMessage { get; }

        /// <summary>
        /// Gets the optional validation override.
        /// </summary>
        public bool? ValidateRequest { get; }

        /// <summary>
        /// Gets a value indicating whether response mapping should reload the saved entity from storage.
        /// </summary>
        public bool ReloadBeforeResponse { get; }

        /// <summary>
        /// Gets navigation expressions used when reloading the response projection.
        /// </summary>
        public IReadOnlyCollection<LambdaExpression> ResponseIncludeExpressions { get; }

        /// <summary>
        /// Gets the optional explicit handler implementation type.
        /// </summary>
        public Type HandlerType { get; }

        /// <summary>
        /// Gets integration metadata owned by extension packages.
        /// </summary>
        public IReadOnlyDictionary<string, object> Metadata { get; }

        /// <summary>
        /// Gets a value indicating whether the operation returns a response.
        /// </summary>
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
