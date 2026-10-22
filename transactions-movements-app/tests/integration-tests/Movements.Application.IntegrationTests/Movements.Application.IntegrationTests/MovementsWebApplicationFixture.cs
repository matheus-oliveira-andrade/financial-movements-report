using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Movements.Domain.Interfaces.Data;
using Movements.Infrastructure.Data;
using Xunit;

namespace Movements.Api.Tests;

public class MovementsWebApplicationFixture : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _applicationFactory;
    
    public IMediator Mediator { get; private set; }
    public IMovementRepository MovementRepository { get; private set; }

    public MovementsWebApplicationFixture()
    {
        _applicationFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IDbContextOptionsConfiguration<MovementsDbContext>>();
                services.RemoveAll(typeof(DbContextOptions<MovementsDbContext>));
                services.AddDbContext<MovementsDbContext>(opt =>
                {
                    opt.UseInMemoryDatabase($"TransactionsDb-{Guid.NewGuid()}");
                });
            });
        });
    }
    
    public async Task InitializeAsync() 
    {
        var scope = _applicationFactory.Services.CreateScope();

        var serviceProvider = scope.ServiceProvider;
        
        Mediator = serviceProvider.GetRequiredService<IMediator>();
        MovementRepository = serviceProvider.GetRequiredService<IMovementRepository>();
        
        var dbContext = serviceProvider.GetService<MovementsDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        using var scope = _applicationFactory.Services.CreateScope();

        var serviceProvider = scope.ServiceProvider;
        var dbContext = serviceProvider.GetService<MovementsDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
    }
}