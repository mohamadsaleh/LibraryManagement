using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LiberaryManagement.Tests.Dev.Request;

[TestClass]
public class GetRightsFromDBRequestTests
{
    private HttpClient _client = null!;
    private TestSettings _settings = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        // Load test settings
        var settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestSettings.json");
        var settingsJson = File.ReadAllText(settingsPath);
        _settings = JsonSerializer.Deserialize<TestSettings>(settingsJson)!;

        // Create HTTP client with proper configuration
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true // For development
        };

        _client = new HttpClient(handler)
        {
            BaseAddress = new Uri(_settings.BaseUrl)
        };
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _client.Dispose();
    }

    [TestMethod]
    public async Task GetRightsFromDB_ValidRequest_ReturnsAllRights()
    {
        // Act
        var response = await _client.GetAsync(_settings.GetRightsFromDBEndpoint);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var rights = JsonSerializer.Deserialize<List<JsonElement>>(responseContent);

        Assert.IsNotNull(rights);
        // Should return at least some rights (may include system rights)
    }

    [TestMethod]
    public async Task GetRightsFromDB_ReturnsExpectedStructure()
    {
        // Act
        var response = await _client.GetAsync(_settings.GetRightsFromDBEndpoint);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var rights = JsonSerializer.Deserialize<List<JsonElement>>(responseContent);

        Assert.IsNotNull(rights);

        if (rights.Count > 0)
        {
            // Check structure of first right
            var firstRight = rights[0];
            Assert.IsTrue(firstRight.TryGetProperty("id", out _));
            Assert.IsTrue(firstRight.TryGetProperty("name", out _));
            Assert.IsTrue(firstRight.TryGetProperty("description", out _));
            Assert.IsTrue(firstRight.TryGetProperty("type", out _));
        }
    }

    [TestMethod]
    public async Task GetRightsFromDB_AfterAddingRights_IncludesNewRights()
    {
        // First, create a test right
        var rightName = $"TestRight_{Guid.NewGuid()}";
        var createRightRequest = new
        {
            Name = rightName,
            Description = "Test right for GetRightsFromDB",
            Type = "Endpoint"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Dev/CreateRight", createRightRequest);
        Assert.AreEqual(HttpStatusCode.Created, createResponse.StatusCode);

        // Act - Get all rights
        var response = await _client.GetAsync(_settings.GetRightsFromDBEndpoint);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var rights = JsonSerializer.Deserialize<List<JsonElement>>(responseContent);

        Assert.IsNotNull(rights);

        // Check if our newly created right is in the list
        var ourRight = rights.FirstOrDefault(r => r.GetProperty("name").GetString() == rightName);
        Assert.IsNotNull(ourRight, "Newly created right should be in the list");

        Assert.AreEqual(createRightRequest.Description, ourRight.GetProperty("description").GetString());
        Assert.AreEqual(createRightRequest.Type, ourRight.GetProperty("type").GetString());
    }
}
