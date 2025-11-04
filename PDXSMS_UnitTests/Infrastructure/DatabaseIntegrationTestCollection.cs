namespace PDXSMS_UnitTests.Infrastructure;

/// <summary>
/// Test collection definition for database integration tests
/// Ensures tests run sequentially to avoid database conflicts
/// </summary>
[CollectionDefinition("Database Integration Tests", DisableParallelization = true)]
public class DatabaseIntegrationTestCollection
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}