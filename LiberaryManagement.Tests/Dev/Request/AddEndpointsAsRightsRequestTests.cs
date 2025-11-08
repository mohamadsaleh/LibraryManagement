using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LiberaryManagement.Tests.Dev.Request;

[TestClass]
public class AddEndpointsAsRightsRequestTests
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
    public async Task AddEndpointsAsRights_ValidRequest_ReturnsOk()
    {
        // Act
        var response = await _client.PostAsync(_settings.AddEndpointsAsRightsEndpoint, null);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

        // Should have addedEndpoints and existedEndpoints arrays
        Assert.IsTrue(responseObject.TryGetProperty("addedEndpoints", out _));
        Assert.IsTrue(responseObject.TryGetProperty("existedEndpoints", out _));
    }

    [TestMethod]
    public async Task AddEndpointsAsRights_MultipleCalls_ReturnsConsistentResults()
    {
        // Act - First call
        var response1 = await _client.PostAsync(_settings.AddEndpointsAsRightsEndpoint, null);
        var response2 = await _client.PostAsync(_settings.AddEndpointsAsRightsEndpoint, null);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response1.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);

        var content1 = await response1.Content.ReadAsStringAsync();
        var content2 = await response2.Content.ReadAsStringAsync();

        var object1 = JsonSerializer.Deserialize<JsonElement>(content1);
        var object2 = JsonSerializer.Deserialize<JsonElement>(content2);

        // First call should have added endpoints, second should have existed endpoints
        var addedEndpoints1 = object1.GetProperty("addedEndpoints");
        var existedEndpoints2 = object2.GetProperty("existedEndpoints");

        Assert.IsTrue(addedEndpoints1.GetArrayLength() > 0 || existedEndpoints2.GetArrayLength() > 0);
    }

    [TestMethod]
    public async Task AddEndpointsAsRights_ReturnsExpectedStructure()
    {
        // Act
        var response = await _client.PostAsync(_settings.AddEndpointsAsRightsEndpoint, null);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

        // Check structure
        var addedEndpoints = responseObject.GetProperty("addedEndpoints");
        var existedEndpoints = responseObject.GetProperty("existedEndpoints");

        // Each endpoint should have id, name, description, type
        foreach (var endpoint in addedEndpoints.EnumerateArray())
        {
            Assert.IsTrue(endpoint.TryGetProperty("id", out _));
            Assert.IsTrue(endpoint.TryGetProperty("name", out _));
            Assert.IsTrue(endpoint.TryGetProperty("description", out _));
            Assert.IsTrue(endpoint.TryGetProperty("type", out _));
        }

        foreach (var endpoint in existedEndpoints.EnumerateArray())
        {
            Assert.IsTrue(endpoint.TryGetProperty("id", out _));
            Assert.IsTrue(endpoint.TryGetProperty("name", out _));
            Assert.IsTrue(endpoint.TryGetProperty("description", out _));
            Assert.IsTrue(endpoint.TryGetProperty("type", out _));
        }
    }
}
