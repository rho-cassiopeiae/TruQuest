using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

using Domain.Aggregates;
using Domain.Errors;
using Domain.Results;
using UserDm = Domain.Aggregates.User;
using Application.Common.Interfaces;

using Infrastructure.User;

namespace Infrastructure.Persistence.Repositories;

internal class UserRepository : Repository<UserDm>, IUserRepository
{
    private readonly UserManager<UserDm> _userManager;

    public UserRepository(
        IConfiguration configuration,
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope,
        UserManager<UserDm> userManager
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _userManager = userManager;
    }

    public override ValueTask SaveChanges() => ValueTask.CompletedTask;

    public async Task<UserError?> Create(UserDm user)
    {
        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            // @@NOTE: Since auto-saving is enabled by default, a possible
            // error (like duplicate email/username) is detected right away.
            return new UserError(result.ToErrorDictionary());
        }

        return null;
    }

    public async Task<UserError?> AddClaimsTo(UserDm user, List<Claim> claims)
    {
        var result = await _userManager.AddClaimsAsync(user, claims);
        if (!result.Succeeded)
        {
            return new UserError(result.ToErrorDictionary());
        }

        return null;
    }

    // @@TODO: Should be implemented in queryable, not repo.
    public async Task<Either<UserError, List<Claim>>> GetClaimsFor(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return new UserError($"User with id = {id} does not exist");
        }

        // @@??: Why can't return IList<Claim> ?
        var claims = await _userManager.GetClaimsAsync(user);
        return claims.ToList();
    }
}