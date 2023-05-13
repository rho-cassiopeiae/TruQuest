using Microsoft.EntityFrameworkCore;

using Dapper;

using Domain.Aggregates;
using Application.Common.Interfaces;
using Application.User.Queries.GetWatchListUpdates;

namespace Infrastructure.Persistence.Queryables;

internal class WatchListQueryable : Queryable, IWatchListQueryable
{
    private readonly AppDbContext _dbContext;

    public WatchListQueryable(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<string>> GetWatchersFor(WatchedItemType itemType, Guid itemId)
    {
        // @@TODO: Use db distinct instead of client-side.
        var watchers = (
            await _dbContext.WatchList
                .Where(w => w.ItemType == itemType && w.ItemId == itemId)
                .Select(w => w.UserId)
                .ToListAsync()
        ).Distinct();

        return watchers;
    }

    public async Task<IEnumerable<WatchedItemUpdateQm>> GetLatestUpdatesFor(string userId)
    {
        var dbConn = await _getOpenConnection();
        var updates = await dbConn.QueryAsync<WatchedItemUpdateQm>(
            @"
                WITH ""UserWatchList"" AS (
                    SELECT ""ItemType"", ""ItemId"", ""ItemUpdateCategory"", ""LastSeenUpdateTimestamp""
                    FROM truquest.""WatchList""
                    WHERE ""UserId"" = @UserId
                )
                SELECT
                    u.""ItemType"", u.""ItemId"", u.""ItemUpdateCategory"",
                    s.""UpdateTimestamp"", s.""Title"", s.""Details""
                FROM
                    ""UserWatchList"" AS u
                        INNER JOIN
                    truquest.""SubjectUpdates"" AS s
                        ON (
                            u.""ItemType"" = 0 AND
                            u.""ItemId"" = s.""SubjectId"" AND
                            u.""ItemUpdateCategory"" = s.""Category"" AND
                            u.""LastSeenUpdateTimestamp"" < s.""UpdateTimestamp""
                        )
                
                UNION ALL

                SELECT
                    u.""ItemType"", u.""ItemId"", u.""ItemUpdateCategory"",
                    t.""UpdateTimestamp"", t.""Title"", t.""Details""
                FROM
                    ""UserWatchList"" AS u
                        INNER JOIN
                    truquest.""ThingUpdates"" AS t
                        ON (
                            u.""ItemType"" = 1 AND
                            u.""ItemId"" = t.""ThingId"" AND
                            u.""ItemUpdateCategory"" = t.""Category"" AND
                            u.""LastSeenUpdateTimestamp"" < t.""UpdateTimestamp""
                        )
                        
                UNION ALL

                SELECT
                    u.""ItemType"", u.""ItemId"", u.""ItemUpdateCategory"",
                    p.""UpdateTimestamp"", p.""Title"", p.""Details""
                FROM
                    ""UserWatchList"" AS u
                        INNER JOIN
                    truquest.""SettlementProposalUpdates"" AS p
                        ON (
                            u.""ItemType"" = 2 AND
                            u.""ItemId"" = p.""SettlementProposalId"" AND
                            u.""ItemUpdateCategory"" = p.""Category"" AND
                            u.""LastSeenUpdateTimestamp"" < p.""UpdateTimestamp""
                        )
                ORDER BY ""UpdateTimestamp"" DESC
                LIMIT 30 -- @@TODO: Config.
            ",
            param: new { UserId = userId }
        );

        return updates;
    }
}