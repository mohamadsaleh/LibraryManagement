using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LiberaryManagement.Tests.Dev.Request;

[TestClass]
public class GetProjectEndpointsRequestTests
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
    public async Task GetProjectEndpoints_DefaultParameters_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync(_settings.GetProjectEndpointsEndpoint);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var endpoints = JsonSerializer.Deserialize<List<JsonElement>>(responseContent);

        Assert.IsNotNull(endpoints);
        Assert.IsTrue(endpoints.Count > 0);
    }

    [TestMethod]
    public async Task GetProjectEndpoints_WithHasEndpointNameRequireTrue_ReturnsFilteredEndpoints()
    {
        // Act
        var response = await _client.GetAsync($"{_settings.GetProjectEndpointsEndpoint}?hasEndpointNameRequire=true");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var endpoints = JsonSerializer.Deserialize<List<JsonElement>>(responseContent);

        Assert.IsNotNull(endpoints);
        // Should return endpoints that have authorization with "EndpointName" policy
    }

    [TestMethod]
    public async Task GetProjectEndpoints_WithHasEndpointNameRequireFalse_ReturnsAllEndpoints()
    {
        // Act
        var response = await _client.GetAsync($"{_settings.GetProjectEndpointsEndpoint}?hasEndpointNameRequire=false");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var endpoints = JsonSerializer.Deserialize<List<JsonElement>>(responseContent);

        Assert.IsNotNull(endpoints);
        // Should return all endpoints regardless of authorization
    }

    [TestMethod]
    public async Task GetProjectEndpoints_ReturnsExpectedStructure()
    {
        // Act
        var response = await _client.GetAsync(_settings.GetProjectEndpointsEndpoint);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var endpoints = JsonSerializer.Deserialize<List<JsonElement>>(responseContent);

        Assert.IsNotNull(endpoints);
        Assert.IsTrue(endpoints.Count > 0);

        // Check structure of first endpoint
        var firstEndpoint = endpoints[0];
        Assert.IsTrue(firstEndpoint.TryGetProperty("endpointName", out _));
        Assert.IsTrue(firstEndpoint.TryGetProperty("endpointRoute", out _));
        Assert.IsTrue(firstEndpoint.TryGetProperty("method", out _));
        Assert.IsTrue(firstEndpoint.TryGetProperty("authorizeMeta", out _));
    }

    [TestMethod]
    public async Task GetProjectEndpoints_ReturnsOrderedByEndpointName()
    {
        // Act
        var response = await _client.GetAsync(_settings.GetProjectEndpointsEndpoint);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var endpoints = JsonSerializer.Deserialize<List<JsonElement>>(responseContent);

        Assert.IsNotNull(endpoints);
        Assert.IsTrue(endpoints.Count > 1);

        // Check if ordered by endpointName
        for (int i = 1; i < endpoints.Count; i++)
        {
            var currentName = endpoints[i].GetProperty("endpointName").GetString();
            var previousName = endpoints[i - 1].GetProperty("endpointName").GetString();

            Assert.IsTrue(string.Compare(previousName, currentName) <= 0,
                $"Endpoints not ordered correctly: {previousName} should come before {currentName}");
        }
    }
}
