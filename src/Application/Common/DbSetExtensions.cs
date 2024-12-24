using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;

namespace Crpg.Application.Common;

public static class DbSetExtensions
{
    public static async Task<int> RemoveRangeAsync<T>(this DbSet<T> entitySet, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) where T : class
    {
        var list = await entitySet.Where(predicate).ToArrayAsync();
        entitySet.RemoveRange(list);

        return list.Count();
    }
}
