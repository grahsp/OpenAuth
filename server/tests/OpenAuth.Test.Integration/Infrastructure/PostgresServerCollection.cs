using OpenAuth.Test.Common.Infrastructure;

namespace OpenAuth.Test.Integration.Infrastructure;

[CollectionDefinition("postgres")]
public class PostgresServerCollection : ICollectionFixture<PostgresServer>;