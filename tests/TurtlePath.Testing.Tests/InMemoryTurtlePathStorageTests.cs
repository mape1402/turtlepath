namespace TurtlePath.Testing.Tests
{
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Models.Responses;
    using TurtlePath.Persistence;
    using TurtlePath.Testing.Mapping;
    using TurtlePath.Testing.Persistence;

    public sealed class InMemoryTurtlePathStorageTests
    {
        [Fact]
        public async Task Storage_reads_seeded_entities_with_filter_sort_and_paging()
        {
            var storage = CreateStorage();

            storage.Seed(
                new Customer { Id = 1, Name = "Ada" },
                new Customer { Id = 2, Name = "Grace" },
                new Customer { Id = 3, Name = "Katherine" });

            var one = await storage.GetOneAsync<Customer, CustomerResponse>(new GetOneCriteria<Customer>
            {
                FiltersExpression = customer => customer.Id == 2
            });
            var many = await storage.GetManyAsync<Customer, CustomerResponse>(new GetManyCriteria<Customer>
            {
                FiltersExpression = customer => customer.Id >= 1,
                SortingExpression = customer => customer.Name,
                AscendentSort = false,
                PageNumber = 1,
                PageSize = 2
            });
            var ascending = await storage.GetManyAsync<Customer, CustomerResponse>(new GetManyCriteria<Customer>
            {
                SortingExpression = customer => customer.Name,
                AscendentSort = true
            });

            Assert.Equal("Grace", one.Name);
            Assert.Equal(["Katherine", "Grace"], many.Results.Select(customer => customer.Name));
            Assert.Equal(["Ada", "Grace", "Katherine"], ascending.Results.Select(customer => customer.Name));
            Assert.Equal(3, many.RowCount);
            Assert.Equal(2, many.PageCount);
        }

        [Fact]
        public async Task Storage_handles_null_criteria_and_fluent_read_set_options()
        {
            var storage = CreateStorage();

            storage.Set<Customer>().Seed(
                new Customer { Id = 1, Name = "Ada" },
                new Customer { Id = 2, Name = "Grace" });

            var batch = await storage.GetManyAsync<Customer, CustomerResponse>(null);
            var first = await storage
                .For<Customer>()
                .Include(customer => customer.Name)
                .FilterBy("Name@=Ada")
                .SortBy("Name")
                .AsTracking()
                .AsNoTracking()
                .Where(null)
                .SortBy((System.Linq.Expressions.Expression<Func<Customer, object>>)null)
                .SortByDescending((System.Linq.Expressions.Expression<Func<Customer, object>>)null)
                .Page(0, 0)
                .FirstOrDefaultAsync<CustomerResponse>();
            var firstEntity = storage.Set<Customer>().FirstOrDefault(customer => customer.Id == 2);
            var containsAda = storage.Set<Customer>().Contains(customer => customer.Name == "Ada");

            Assert.Equal(2, batch.RowCount);
            Assert.Equal(1, batch.PageNumber);
            Assert.Equal(2, batch.PageSize);
            Assert.Equal(1, batch.PageCount);
            Assert.Equal("Ada", first.Name);
            Assert.Equal("Grace", firstEntity.Name);
            Assert.True(containsAda);
            Assert.Equal([], new BatchResult<CustomerResponse>().AsEnumerable());
        }

        [Fact]
        public async Task Storage_records_writer_operations_and_clears_state()
        {
            var storage = CreateStorage();
            var ada = new Customer { Id = 1, Name = "Ada" };
            var grace = new Customer { Id = 2, Name = "Grace" };
            var katherine = new Customer { Id = 3, Name = "Katherine" };

            await storage.AddAsync(ada);
            await storage.AddRangeAsync([grace, katherine]);
            storage.Update(ada);
            storage.UpdateRange([grace, katherine]);
            storage.RemoveRange([grace]);
            var saveResult = await storage.SaveChangesAsync();

            Assert.Equal(1, saveResult);
            Assert.Equal(["Add", "Add", "Add", "Update", "Update", "Update", "Remove", "SaveChanges"], storage.Operations.Select(operation => operation.Action));
            Assert.Equal(["Ada", "Katherine"], storage.Entities<Customer>().Select(customer => customer.Name));

            storage.Clear();

            Assert.Empty(storage.Entities<Customer>());
            Assert.Empty(storage.Operations);
        }

        [Fact]
        public async Task Obsolete_writer_methods_delegate_to_current_operations()
        {
            var storage = CreateStorage();
            var customer = new Customer { Id = 1, Name = "Ada" };

#pragma warning disable CS0618
            await storage.SaveAsync(customer);
            await storage.UpdateAsync(customer);
            await storage.DeleteAsync(customer);
#pragma warning restore CS0618

            Assert.Equal(["Add", "SaveChanges", "Update", "SaveChanges", "Remove", "SaveChanges"], storage.Operations.Select(operation => operation.Action));
            Assert.Empty(storage.Entities<Customer>());
        }

        [Fact]
        public void Storage_validates_required_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => new InMemoryTurtlePathStorage(null));

            var storage = CreateStorage();

            Assert.Throws<ArgumentNullException>(() => storage.Seed<Customer>(null));
        }

        private static InMemoryTurtlePathStorage CreateStorage()
            => new(new DelegateMapperAdapter()
                .WithMap<Customer, CustomerResponse>(customer => new CustomerResponse
                {
                    Id = customer.Id,
                    Name = customer.Name
                }));

        private sealed class Customer : IEntity<int>
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        private sealed class CustomerResponse : IBaseResponse<int>
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
