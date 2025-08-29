using OpenAuth.Application.Common;

namespace OpenAuth.Application.Tests.Stubs;

public class FakeUnitOfWork : IUnitOfWork
{
    public bool Saved { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        Saved = true;
        return Task.FromResult(0);
    }
}