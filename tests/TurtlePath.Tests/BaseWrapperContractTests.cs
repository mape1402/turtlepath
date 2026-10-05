using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Pelican.Mediator;
using TurtlePath.Commands;
using TurtlePath.Domain.Contracts;
using TurtlePath.Domain.Identifier;
using TurtlePath.Exceptions;
using TurtlePath.Models.Requests;
using TurtlePath.Models.Responses;
using TurtlePath.Queries;

namespace TurtlePath.Tests;

public sealed class BaseWrapperContractTests
{
    [Fact]
    public async Task Base_cid_handler_wrappers_construct_with_turtlepath_services()
    {
        var services = new ServiceCollection();
        services
            .AddTurtlePath();
        services
            .AddSingleton<TurtlePath.Persistence.IStorageReaderAdapter, EmptyStorageAdapter>()
            .AddSingleton<TurtlePath.Persistence.IStorageWriterAdapter, EmptyStorageAdapter>()
            .AddSingleton<TurtlePath.Mapping.IMapperAdapter, EmptyMapperAdapter>()
            .AddSingleton<TurtlePath.Validation.IValidatorAdapter, EmptyValidatorAdapter>();
        await using var provider = services.BuildServiceProvider();

        var handlers = new object[]
        {
            new CreateWithResponseHandler(provider),
            new CreateWithoutResponseHandler(provider),
            new UpdateWithResponseHandler(provider),
            new UpdateWithoutResponseHandler(provider),
            new DeleteWithResponseHandler(provider),
            new DeleteWithoutResponseHandler(provider),
            new PatchWithResponseHandler(provider),
            new PatchWithoutResponseHandler(provider),
            new GetByIdHandler(provider),
            new GetManyHandler(provider),
            new GetOneHandler(provider),
            new GetPagedHandler(provider)
        };

        Assert.All(handlers, Assert.NotNull);
    }

    [Fact]
    public void Base_cid_queries_and_http_exceptions_expose_expected_contracts()
    {
        var id = CId.From("customer-1");
        var entity = new CustomerEntity { Id = id };
        var request = new CustomerRequest { Id = id };
        var response = new CustomerResponse { Id = id };
        var byId = new CustomerByIdQuery(id);
        var paged = new CustomerPagedQuery(new PagedSettings { PageNumber = 2, PageSize = 25 });
        var badRequest = new BadRequestException();
        var customBadRequest = new BadRequestException("Bad input");
        var forbidden = new ForbiddenException("orders", "ada");
        var customForbidden = new ForbiddenException("Nope");
        var unauthorized = new UnauthorizedException("grace");
        var unauthorizedWithMessage = new UnauthorizedException("grace", " Missing token.");

        Assert.Equal(id, entity.Id);
        Assert.Equal(id, request.Id);
        Assert.Equal(id, response.Id);
        Assert.Equal(id, byId.Value);
        Assert.Equal(2, paged.PagedSettings.PageNumber);
        Assert.Equal(25, paged.PagedSettings.PageSize);
        Assert.Equal(HttpStatusCode.BadRequest, badRequest.StatusCode);
        Assert.Contains("BadRequest", badRequest.Message);
        Assert.Equal("Bad input", customBadRequest.Message);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        Assert.Contains("orders", forbidden.Message);
        Assert.Equal("Nope", customForbidden.Message);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);
        Assert.Contains("grace", unauthorized.Message);
        Assert.Contains("Missing token", unauthorizedWithMessage.Message);
    }

    private sealed class CustomerEntity : BaseEntity
    {
        public string Name { get; set; }
    }

    private sealed class CustomerResponse : BaseResponse
    {
        public string Name { get; set; }
    }

    private sealed class CreateCustomerRequest : IRequest<CustomerResponse>
    {
        public string Name { get; set; }
    }

    private sealed class CreateCustomerCommand : IRequest
    {
        public string Name { get; set; }
    }

    private sealed class CustomerRequest : BaseRequest, IRequest<CustomerResponse>
    {
    }

    private sealed class CustomerCommand : BaseRequest, IRequest
    {
    }

    private sealed class CustomerByIdQuery(CId id) : GetByIdQuery<CustomerEntity, CustomerResponse>(id)
    {
    }

    private sealed class CustomerManyQuery : GetManyQuery<CustomerEntity, CustomerResponse>
    {
    }

    private sealed class CustomerOneQuery : GetOneQuery<string, CustomerEntity, CustomerResponse>
    {
        public string Name { get; set; }
    }

    private sealed class CustomerPagedQuery(PagedSettings pagedSettings) : GetPagedInfoQuery<CustomerEntity, CustomerResponse>(pagedSettings)
    {
    }

    private sealed class CreateWithResponseHandler(IServiceProvider serviceProvider)
        : CreateCommandHandler<CreateCustomerRequest, CustomerResponse, CustomerEntity>(serviceProvider)
    {
    }

    private sealed class CreateWithoutResponseHandler(IServiceProvider serviceProvider)
        : CreateCommandHandler<CreateCustomerCommand, CustomerEntity>(serviceProvider)
    {
    }

    private sealed class UpdateWithResponseHandler(IServiceProvider serviceProvider)
        : UpdateCommandHandler<CustomerRequest, CustomerResponse, CustomerEntity>(serviceProvider)
    {
    }

    private sealed class UpdateWithoutResponseHandler(IServiceProvider serviceProvider)
        : UpdateCommandHandler<CustomerCommand, CustomerEntity>(serviceProvider)
    {
    }

    private sealed class DeleteWithResponseHandler(IServiceProvider serviceProvider)
        : DeleteCommandHandler<CustomerRequest, CustomerResponse, CustomerEntity>(serviceProvider)
    {
    }

    private sealed class DeleteWithoutResponseHandler(IServiceProvider serviceProvider)
        : DeleteCommandHandler<CustomerCommand, CustomerEntity>(serviceProvider)
    {
    }

    private sealed class PatchWithResponseHandler(IServiceProvider serviceProvider)
        : PatchCommandHandler<CustomerRequest, CustomerResponse, CustomerEntity>(serviceProvider)
    {
    }

    private sealed class PatchWithoutResponseHandler(IServiceProvider serviceProvider)
        : PatchCommandHandler<CustomerCommand, CustomerEntity>(serviceProvider)
    {
    }

    private sealed class GetByIdHandler(IServiceProvider serviceProvider)
        : GetByIdQueryHandler<CustomerByIdQuery, CustomerEntity, CustomerResponse>(serviceProvider)
    {
    }

    private sealed class GetManyHandler(IServiceProvider serviceProvider)
        : GetManyQueryHandler<CustomerManyQuery, CustomerEntity, CustomerResponse>(serviceProvider)
    {
    }

    private sealed class GetOneHandler(IServiceProvider serviceProvider)
        : GetOneQueryHandler<CustomerOneQuery, string, CustomerEntity, CustomerResponse>(serviceProvider)
    {
    }

    private sealed class GetPagedHandler(IServiceProvider serviceProvider)
        : GetPagedInfoQueryHandler<CustomerPagedQuery, CustomerEntity, CustomerResponse>(serviceProvider)
    {
    }

    private sealed class EmptyStorageAdapter :
        TurtlePath.Persistence.IStorageReaderAdapter,
        TurtlePath.Persistence.IStorageWriterAdapter
    {
        public TurtlePath.Persistence.IStorageReadSet<TEntity> For<TEntity>()
            where TEntity : class, IEntity
            => throw new NotSupportedException();

        public Task<TExpected> GetOneAsync<TEntity, TExpected>(TurtlePath.Persistence.GetOneCriteria<TEntity> criteria, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            where TExpected : class
            => throw new NotSupportedException();

        public Task<TurtlePath.Persistence.BatchResult<TExpected>> GetManyAsync<TEntity, TExpected>(TurtlePath.Persistence.GetManyCriteria<TEntity> criteria, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            where TExpected : class
            => throw new NotSupportedException();

        public ValueTask AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => throw new NotSupportedException();

        public Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => throw new NotSupportedException();

        public void Update<TEntity>(TEntity entity)
            where TEntity : class, IEntity
            => throw new NotSupportedException();

        public void UpdateRange<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class, IEntity
            => throw new NotSupportedException();

        public void Remove<TEntity>(TEntity entity)
            where TEntity : class, IEntity
            => throw new NotSupportedException();

        public void RemoveRange<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class, IEntity
            => throw new NotSupportedException();

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task SaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => throw new NotSupportedException();

        public Task UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => throw new NotSupportedException();

        public Task DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => throw new NotSupportedException();
    }

    private sealed class EmptyMapperAdapter : TurtlePath.Mapping.IMapperAdapter
    {
        public ValueTask<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default)
            where TSource : class
            where TDestination : class
            => throw new NotSupportedException();

        public ValueTask UpdateMapAsync<TSource, TDestination>(TSource source, TDestination destination, CancellationToken cancellationToken = default)
            where TSource : class
            where TDestination : class
            => throw new NotSupportedException();
    }

    private sealed class EmptyValidatorAdapter : TurtlePath.Validation.IValidatorAdapter
    {
        public ValueTask ValidateAsync<TModel>(TModel model, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;
    }
}
