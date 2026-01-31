namespace Excos.AspNetCore.Lite.UITests;

/// <summary>
/// Collection definition for tests that share the test server.
/// This ensures the server is only started once for all tests in the collection.
/// </summary>
[CollectionDefinition("TestServer")]
public class TestServerCollection : ICollectionFixture<TestServerFixture>
{
    // This class has no code, and is never created.
    // Its purpose is simply to be the place to apply [CollectionDefinition] and the ICollectionFixture<> interface.
}
