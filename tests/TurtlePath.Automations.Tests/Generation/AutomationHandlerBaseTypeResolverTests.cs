namespace TurtlePath.Automations.Tests.Generation
{
    using System.Linq.Expressions;
    using Pelican.Mediator;
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Automations.Generation;
    using TurtlePath.Commands;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Models.Requests;
    using TurtlePath.Models.Responses;
    using TurtlePath.Queries;

    public sealed class AutomationHandlerBaseTypeResolverTests
    {
        [Theory]
        [InlineData((int)AutomationOperationKind.Create, (int)AutomationReturnMode.Response, typeof(GenericCreateCommandHandler<CreateCommand, CustomerResponse, Customer, int>))]
        [InlineData((int)AutomationOperationKind.Create, (int)AutomationReturnMode.None, typeof(GenericCreateCommandHandler<CreateNoResponseCommand, Customer, int>))]
        [InlineData((int)AutomationOperationKind.Update, (int)AutomationReturnMode.Response, typeof(GenericUpdateCommandHandler<UpdateCommand, CustomerResponse, Customer, int>))]
        [InlineData((int)AutomationOperationKind.Update, (int)AutomationReturnMode.None, typeof(GenericUpdateCommandHandler<UpdateNoResponseCommand, Customer, int>))]
        [InlineData((int)AutomationOperationKind.Delete, (int)AutomationReturnMode.Response, typeof(GenericDeleteCommandHandler<DeleteCommand, CustomerResponse, Customer, int>))]
        [InlineData((int)AutomationOperationKind.Delete, (int)AutomationReturnMode.None, typeof(GenericDeleteCommandHandler<DeleteNoResponseCommand, Customer, int>))]
        [InlineData((int)AutomationOperationKind.Patch, (int)AutomationReturnMode.Response, typeof(GenericPatchCommandHandler<PatchCommand, CustomerResponse, Customer, int>))]
        [InlineData((int)AutomationOperationKind.Patch, (int)AutomationReturnMode.None, typeof(GenericPatchCommandHandler<PatchNoResponseCommand, Customer, int>))]
        public void Resolve_returns_command_handler_base_type(
            int operationKindValue,
            int returnModeValue,
            Type expectedType)
        {
            var operationKind = (AutomationOperationKind)operationKindValue;
            var returnMode = (AutomationReturnMode)returnModeValue;
            var descriptor = new AutomationDescriptor(
                operationKind,
                RequestTypeFor(operationKind, returnMode),
                typeof(Customer),
                typeof(int),
                returnMode,
                returnMode == AutomationReturnMode.Response ? typeof(CustomerResponse) : null);

            var result = new AutomationHandlerBaseTypeResolver().Resolve(descriptor);

            Assert.Equal(expectedType, result);
        }

        [Fact]
        public void Resolve_returns_query_handler_base_types()
        {
            var resolver = new AutomationHandlerBaseTypeResolver();

            Assert.Equal(
                typeof(GenericGetByIdQueryHandler<GetByIdQuery, Customer, CustomerResponse, int>),
                resolver.Resolve(CreateQueryDescriptor(AutomationOperationKind.GetById, typeof(GetByIdQuery), typeof(CustomerResponse))));
            Assert.Equal(
                typeof(GenericGetOneQueryHandler<GetOneQuery, string, Customer, CustomerResponse, int>),
                resolver.Resolve(CreateQueryDescriptor(AutomationOperationKind.GetOne, typeof(GetOneQuery), typeof(CustomerResponse))));
            Assert.Equal(
                typeof(GenericGetManyQueryHandler<GetManyQuery, Customer, CustomerResponse, int>),
                resolver.Resolve(CreateQueryDescriptor(AutomationOperationKind.GetMany, typeof(GetManyQuery), typeof(IEnumerable<CustomerResponse>))));
            Assert.Equal(
                typeof(GenericGetPagedInfoQueryHandler<GetPagedQuery, Customer, CustomerResponse, int>),
                resolver.Resolve(CreateQueryDescriptor(AutomationOperationKind.GetPaged, typeof(GetPagedQuery), typeof(PagedResponse<CustomerResponse>))));
            Assert.Equal(
                typeof(GenericGetManyQueryHandler<GetManyQuery, Customer, CustomerResponse, int>),
                resolver.Resolve(CreateQueryDescriptor(AutomationOperationKind.GetMany, typeof(GetManyQuery), typeof(CustomerResponse))));
            Assert.Equal(
                typeof(GenericGetPagedInfoQueryHandler<GetPagedQuery, Customer, CustomerResponse, int>),
                resolver.Resolve(CreateQueryDescriptor(AutomationOperationKind.GetPaged, typeof(GetPagedQuery), typeof(CustomerResponse))));
        }

        [Fact]
        public void Resolve_validates_descriptor_and_wraps_invalid_generic_contract()
        {
            var resolver = new AutomationHandlerBaseTypeResolver();

            Assert.Throws<ArgumentNullException>(() => resolver.Resolve(null));
            var exception = Assert.Throws<NotSupportedException>(() => resolver.Resolve(new AutomationDescriptor(
                AutomationOperationKind.Create,
                typeof(InvalidCreateCommand),
                typeof(Customer),
                typeof(int),
                AutomationReturnMode.Response,
                typeof(CustomerResponse))));
            var queryException = Assert.Throws<NotSupportedException>(() => resolver.Resolve(new AutomationDescriptor(
                AutomationOperationKind.GetOne,
                typeof(GetOneWithSelectorQuery),
                typeof(Customer),
                typeof(int),
                AutomationReturnMode.Response,
                typeof(CustomerResponse),
                keySelector: (Expression<Func<GetOneWithSelectorQuery, Guid>>)(query => query.ExternalId))));
            var operationException = Assert.Throws<NotSupportedException>(() => resolver.Resolve(new AutomationDescriptor(
                (AutomationOperationKind)999,
                typeof(CreateCommand),
                typeof(Customer),
                typeof(int),
                AutomationReturnMode.Response,
                typeof(CustomerResponse))));

            Assert.Contains(nameof(AutomationOperationKind.Create), exception.Message);
            Assert.Contains(typeof(InvalidCreateCommand).FullName!, exception.Message);
            Assert.Contains(nameof(AutomationOperationKind.GetOne), queryException.Message);
            Assert.Contains("999", operationException.Message);
        }

        private static AutomationDescriptor CreateQueryDescriptor(
            AutomationOperationKind operationKind,
            Type requestType,
            Type responseType)
            => new(
                operationKind,
                requestType,
                typeof(Customer),
                typeof(int),
                AutomationReturnMode.Response,
                responseType);

        private static Type RequestTypeFor(AutomationOperationKind operationKind, AutomationReturnMode returnMode)
            => (operationKind, returnMode) switch
            {
                (AutomationOperationKind.Create, AutomationReturnMode.Response) => typeof(CreateCommand),
                (AutomationOperationKind.Create, AutomationReturnMode.None) => typeof(CreateNoResponseCommand),
                (AutomationOperationKind.Update, AutomationReturnMode.Response) => typeof(UpdateCommand),
                (AutomationOperationKind.Update, AutomationReturnMode.None) => typeof(UpdateNoResponseCommand),
                (AutomationOperationKind.Delete, AutomationReturnMode.Response) => typeof(DeleteCommand),
                (AutomationOperationKind.Delete, AutomationReturnMode.None) => typeof(DeleteNoResponseCommand),
                (AutomationOperationKind.Patch, AutomationReturnMode.Response) => typeof(PatchCommand),
                (AutomationOperationKind.Patch, AutomationReturnMode.None) => typeof(PatchNoResponseCommand),
                _ => throw new NotSupportedException()
            };

        private sealed class Customer : IEntity<int>
        {
            public int Id { get; set; }
        }

        private sealed class CustomerResponse : IBaseResponse<int>
        {
            public int Id { get; set; }
        }

        private sealed class CreateCommand : IRequest<CustomerResponse>
        {
        }

        private sealed class InvalidCreateCommand
        {
        }

        private sealed class CreateNoResponseCommand : IRequest
        {
        }

        private sealed class UpdateCommand : IBaseRequest<int>, IRequest<CustomerResponse>
        {
            public int Id { get; set; }
        }

        private sealed class UpdateNoResponseCommand : IBaseRequest<int>, IRequest
        {
            public int Id { get; set; }
        }

        private sealed class DeleteCommand : IBaseRequest<int>, IRequest<CustomerResponse>
        {
            public int Id { get; set; }
        }

        private sealed class DeleteNoResponseCommand : IBaseRequest<int>, IRequest
        {
            public int Id { get; set; }
        }

        private sealed class PatchCommand : IBaseRequest<int>, IRequest<CustomerResponse>
        {
            public int Id { get; set; }
        }

        private sealed class PatchNoResponseCommand : IBaseRequest<int>, IRequest
        {
            public int Id { get; set; }
        }

        private sealed class GetByIdQuery : GenericGetByIdQuery<Customer, CustomerResponse, int>
        {
            public GetByIdQuery(int id) : base(id)
            {
            }
        }

        private sealed class GetOneQuery : GenericGetOneQuery<string, Customer, CustomerResponse, int>
        {
        }

        private sealed class GetOneWithSelectorQuery
        {
            public Guid ExternalId { get; set; }
        }

        private sealed class GetManyQuery : GenericGetManyQuery<Customer, CustomerResponse, int>
        {
        }

        private sealed class GetPagedQuery : GenericGetPagedInfoQuery<Customer, CustomerResponse, int>
        {
            public GetPagedQuery() : base(new PagedSettings())
            {
            }
        }
    }
}
