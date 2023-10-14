using System.Linq.Expressions;
using BLL.DTO.Entities;
using Utils;

namespace DAL.EF.Extensions;

public static class PaginationExtensions
{
    public static IOrderedQueryable<TEntity> OrderBy<TEntity>(
        this IQueryable<TEntity> query,
        SortOptions sortOptions,
        SortBehaviour<TEntity> sortBehaviour)
    {
        if (sortOptions.Descending != null)
        {
            sortBehaviour = sortBehaviour with { Descending = sortOptions.Descending.Value };
        }

        return sortBehaviour.Descending
            ? query.OrderByDescending(sortBehaviour.OrderExpression)
            : query.OrderBy(sortBehaviour.OrderExpression);
    }

    public static IOrderedEnumerable<TEntity> OrderBy<TEntity>(
        this IEnumerable<TEntity> query,
        SortOptions sortOptions,
        SortBehaviour<TEntity> sortBehaviour)
    {
        if (sortOptions.Descending != null)
        {
            sortBehaviour = sortBehaviour with { Descending = sortOptions.Descending.Value };
        }

        return sortBehaviour.Descending
            ? query.OrderByDescending(sortBehaviour.OrderExpression.Compile())
            : query.OrderBy(sortBehaviour.OrderExpression.Compile());
    }

    public static IOrderedQueryable<TEntity> ThenBy<TEntity>(
        this IOrderedQueryable<TEntity> query,
        SortOptions sortOptions,
        SortBehaviour<TEntity> sortBehaviour)
    {
        if (sortOptions.Descending != null)
        {
            sortBehaviour = sortBehaviour with { Descending = sortOptions.Descending.Value };
        }

        return sortBehaviour.Descending
            ? query.ThenByDescending(sortBehaviour.OrderExpression)
            : query.ThenBy(sortBehaviour.OrderExpression);
    }

    public static IQueryable<TEntity> Paginate<TEntity>(
        this IQueryable<TEntity> query,
        IPaginationQuery paginationParams)
    {
        paginationParams.ConformValues();
        var skipAmount = paginationParams.GetSkipAmount();

        return query.Skip(skipAmount).Take(paginationParams.Limit);
    }

    public static IEnumerable<TEntity> Paginate<TEntity>(
        this IEnumerable<TEntity> query,
        IPaginationQuery paginationParams)
    {
        paginationParams.ConformValues();
        var skipAmount = paginationParams.GetSkipAmount();

        return query.Skip(skipAmount).Take(paginationParams.Limit);
    }
}

public record SortBehaviour<TEntity>(Expression<Func<TEntity, dynamic>> OrderExpression, bool Descending)
{
    public static implicit operator SortBehaviour<TEntity>((Expression<Func<TEntity, dynamic>> orderExpression,
        bool descending) tuple) => new(tuple.orderExpression, tuple.descending);
}