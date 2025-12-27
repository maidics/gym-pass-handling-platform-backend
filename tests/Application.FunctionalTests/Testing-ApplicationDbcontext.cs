using System.Linq.Expressions;
using FitPass.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests;

public partial class Testing
{
    public static async Task<TEntity?> FindAsync<TEntity>(params object[] keyValues)
    where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.FindAsync<TEntity>(keyValues);
    }

    public static async Task<TEntity?> GetFirstAsync<TEntity>() where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().FirstAsync();
    }

    public static async Task<TEntity?> FindByUserIdAsync<TEntity>(string userId)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().FirstOrDefaultAsync(e => EF.Property<string>(e, "UserId") == userId);
    }

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();
    }

    public static async Task<int> CountAsync<TEntity>() where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().CountAsync();
    }

    public static async Task<TEntity?> FindAsync<TEntity>(object[] keyValues, params Expression<Func<TEntity, object?>>[] includeProperties) where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (includeProperties.Length == 0)
        {
            return await context.FindAsync<TEntity>(keyValues);
        }

        IQueryable<TEntity> query = context.Set<TEntity>();

        foreach (var property in includeProperties)
        {
            query = query.Include(property);
        }

        return await FindByKeyAsync(context, query, keyValues);
    }

    private static async Task<TEntity?> FindByKeyAsync<TEntity>(ApplicationDbContext context, IQueryable<TEntity> query, object[] keyvalues) where TEntity : class
    {
        var entityType = context.Model.FindEntityType(typeof(TEntity));

        if (entityType == null)
        {
            throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} not found in db context model.");
        }

        var keys = entityType.FindPrimaryKey()?.Properties;

        if (keys == null || keys.Count != keyvalues.Length)
        {
            throw new InvalidOperationException($"Number of key values ({keyvalues.Length}) does not match the number of key properties ({keys?.Count ?? 0}).");
        }

        var parameter = Expression.Parameter(typeof(TEntity), "e");

        Expression? predicate = null;

        for (int i = 0; i < keyvalues.Length; i++)
        {
            var property = keys[i];
            var propertyAccess = Expression.Property(parameter, property.Name);
            var keyValue = Expression.Constant(keyvalues[i]);
            var equals = Expression.Equal(propertyAccess, keyValue);

            predicate = predicate == null ? equals : Expression.AndAlso(predicate, equals);
        }

        if (predicate == null)
        {
            return null;
        }

        var lambda = Expression.Lambda<Func<TEntity, bool>>(predicate, parameter);

        return await query.FirstOrDefaultAsync(lambda);
    }

    public static async Task<int> SaveChangesAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        using var service = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await service.SaveChangesAsync();
    }
}
