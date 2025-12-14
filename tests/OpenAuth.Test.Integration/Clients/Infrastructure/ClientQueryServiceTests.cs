using Microsoft.Extensions.Time.Testing;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Infrastructure.Clients.Persistence;
using OpenAuth.Infrastructure.Persistence;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Integration.Infrastructure.Fixtures;

namespace OpenAuth.Test.Integration.Clients.Infrastructure;

[Collection("sqlserver")]
public class ClientQueryServiceTests : IAsyncLifetime
{
    private SqlServerFixture _fx;
    private AppDbContext _context;
    private FakeTimeProvider _time;
    private ClientQueryService _sut;

    public ClientQueryServiceTests(SqlServerFixture fx)
    {
        _fx = fx;
        _context = _fx.CreateContext();
        _time = new FakeTimeProvider();
        _sut = new ClientQueryService(_context);
    }
    
    public async Task InitializeAsync()
        => await _fx.ResetAsync();
    
    public Task DisposeAsync()
        => Task.CompletedTask;

    
    public class GetByIdAsync(SqlServerFixture fx) : ClientQueryServiceTests(fx)
    {
        [Fact]
        public async Task ReturnsClientSummary_WhenClientExists()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("Client")
                .Build();
            
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _sut.GetByIdAsync(client.Id);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(client.Id.ToString(), result.Id);
            Assert.Equal(client.Name.ToString(), result.Name);
        }
        
        [Fact]
        public async Task ReturnsNull_WhenClientDoesNotExist()
        {
            // Arrange & Act
            var result = await _sut.GetByIdAsync(ClientId.New());
            
            // Assert
            Assert.Null(result);
        }
    }
    
    public class GetByNameAsync(SqlServerFixture fx) : ClientQueryServiceTests(fx)
    {
        [Fact]
        public async Task ReturnsClientSummary_WhenClientExists()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("Client")
                .Build();
            
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _sut.GetByNameAsync(client.Name);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(client.Id.ToString(), result.Id);
            Assert.Equal(client.Name.ToString(), result.Name);
        }
        
        [Fact]
        public async Task ReturnsNull_WhenClientDoesNotExist()
        {
            // Act
            var result = await _sut.GetByNameAsync(new ClientName("Missing-Client"));
            
            // Assert
            Assert.Null(result);
        }
    }

    public class GetDetailsAsync(SqlServerFixture fx) : ClientQueryServiceTests(fx)
    {
        [Fact]
        public async Task ReturnsClientDetails_WhenClientExists()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _sut.GetDetailsAsync(client.Id);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(client.Id, result.Id);
            Assert.Equal(client.Name, result.Name);
            Assert.Equal(client.CreatedAt, result.CreatedAt);
        }
        
        [Fact]
        public async Task ReturnsNull_WhenClientDoesNotExist()
        {
            // Act
            var result = await _sut.GetDetailsAsync(ClientId.New());
            
            // Assert
            Assert.Null(result);
        }
        
        [Fact]
        public async Task ReturnsEmptyCollections_WhenClientHasNoRelatedData()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _sut.GetDetailsAsync(client.Id);
            
            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Audiences);
            Assert.NotNull(result.RedirectUris);
            Assert.NotNull(result.GrantTypes);
        }
    }

    public class GetPagedAsync(SqlServerFixture fx) : ClientQueryServiceTests(fx)
    {
        [Fact]
        public async Task ReturnsCorrectPage()
        {
            // Arrange
            for (var i = 1; i <= 10; i++)
            {
                var client = new ClientBuilder()
                    .WithName($"client-{i:D2}")
                    .Build();
                
                _context.Clients.Add(client);
            }
            
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _sut.GetPagedAsync(page: 2, pageSize: 3);
            
            // Assert
            Assert.Equal(3, result.Items.Count());
            Assert.Equal(10, result.TotalCount);
            Assert.Equal(2, result.Page);
            Assert.Equal(3, result.PageSize);
        }
        
        [Fact]
        public async Task ReturnsOrderedByName()
        {
            // Arrange
            var alpha = new ClientBuilder().WithName("alpha").Build();
            var beta = new ClientBuilder().WithName("beta").Build();
            var zebra = new ClientBuilder().WithName("zebra").Build();
            
            _context.Clients.AddRange(alpha, beta, zebra);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _sut.GetPagedAsync(page: 1, pageSize: 10);
            var items = result.Items.ToArray();
            
            // Assert
            Assert.Equal(3, items.Length);
            Assert.Equal(alpha.Name.ToString(), items[0].Name);
            Assert.Equal(beta.Name.ToString(), items[1].Name);
            Assert.Equal(zebra.Name.ToString(), items[2].Name);
        }
        
        [Fact]
        public async Task ReturnsEmptyList_WhenNoClients()
        {
            // Act
            var result = await _sut.GetPagedAsync(page: 1, pageSize: 10);
            
            // Assert
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }
        
        [Fact]
        public async Task CalculatesPaginationCorrectly()
        {
            // Arrange
            for (var i = 1; i <= 25; i++)
            {
                _context.Clients.Add(new ClientBuilder()
                    .WithName($"Client-{i:D2}")
                    .Build());
            }
            
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _sut.GetPagedAsync(page: 3, pageSize: 10);
            
            // Assert
            Assert.Equal(5, result.Items.Count()); // Last page has 5 items
            Assert.Equal(25, result.TotalCount);
            // Assert.Equal(3, result.TotalPages);
            // Assert.False(result.HasNextPage);
            // Assert.True(result.HasPreviousPage);
        }
    }

    // ============================================
    // ExistsAsync
    // ============================================
    
    public class ExistsAsync(SqlServerFixture fx) : ClientQueryServiceTests(fx)
    {
        [Fact]
        public async Task ReturnsTrue_WhenClientExists()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _sut.ExistsAsync(client.Id);
            
            // Assert
            Assert.True(result);
        }
        
        [Fact]
        public async Task ReturnsFalse_WhenClientDoesNotExist()
        {
            // Act
            var result = await _sut.ExistsAsync(ClientId.New());
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public async Task DoesNotLoadEntity()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            
            _context.ChangeTracker.Clear();
            
            // Act
            await _sut.ExistsAsync(client.Id);
            
            // Assert
            var trackedClient = _context.Clients.Local.FirstOrDefault();
            Assert.Null(trackedClient); // Nothing should be tracked
        }
    }
    
    public class NameExistsAsync(SqlServerFixture fx) : ClientQueryServiceTests(fx)
    {
        [Fact]
        public async Task ReturnsTrue_WhenNameExists()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("Client")
                .Build();
            
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _sut.NameExistsAsync(client.Name);
            
            // Assert
            Assert.True(result);
        }
        
        [Fact]
        public async Task ReturnsFalse_WhenNameDoesNotExist()
        {
            // Act
            var result = await _sut.NameExistsAsync(new ClientName("Missing-Client"));
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public async Task DoesNotLoadEntity()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            
            _context.ChangeTracker.Clear();
            
            // Act
            await _sut.NameExistsAsync(client.Name);
            
            // Assert
            var trackedClient = _context.Clients.Local.FirstOrDefault();
            Assert.Null(trackedClient);
        }
    }
}