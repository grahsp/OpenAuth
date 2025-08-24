using OpenAuth.Application.Common;

namespace OpenAuth.Application.Tests.Stubs;

public class FakeUnitOfWork : IUnitOfWork
{
    public bool Saved { get; private set; }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        Saved = true;
        return Task.CompletedTask;
    }
}