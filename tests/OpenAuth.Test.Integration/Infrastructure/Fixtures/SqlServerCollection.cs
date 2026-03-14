using OpenAuth.Test.Common.Infrastructure;

namespace OpenAuth.Test.Integration.Infrastructure.Fixtures;

[CollectionDefinition("sqlserver")]
public class SqlServerCollection : ICollectionFixture<SqlServer>;