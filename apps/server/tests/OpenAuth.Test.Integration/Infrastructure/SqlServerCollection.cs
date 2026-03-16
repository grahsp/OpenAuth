using OpenAuth.Test.Common.Infrastructure;

namespace OpenAuth.Test.Integration.Infrastructure;

[CollectionDefinition("sqlserver")]
public class SqlServerCollection : ICollectionFixture<SqlServer>;