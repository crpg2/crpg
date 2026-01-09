using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Common;

public static class DbSetExtensions
{
    public static async Task<int> RemoveRangeAsync<T>(this DbSet<T> entitySet, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) where T : class
    {
        var list = await entitySet.Where(predicate).ToArrayAsync(cancellationToken: cancellationToken);
        entitySet.RemoveRange(list);

        return list.Length;
    }
}
