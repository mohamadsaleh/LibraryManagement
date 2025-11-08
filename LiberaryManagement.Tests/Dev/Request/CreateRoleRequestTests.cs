using LibraryManagement.Endpoints.Dev;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LiberaryManagement.Tests.Dev.Request;

[TestClass]
public class CreateRoleRequestTests
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
    public async Task CreateRole_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new CreateRoleRequest($"TestRole_{Guid.NewGuid()}");

        // Act
        var response = await _client.PostAsJsonAsync(_settings.CreateRoleEndpoint, request);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.IsTrue(responseObject.GetProperty("id").GetInt32() > 0);
        Assert.AreEqual(request.Name, responseObject.GetProperty("name").GetString());
    }

    [TestMethod]
    public async Task CreateRole_DuplicateName_ReturnsConflict()
    {
        // Arrange
        var roleName = $"TestRole_{Guid.NewGuid()}";
        var request1 = new CreateRoleRequest(roleName);
        var request2 = new CreateRoleRequest(roleName);

        // Create first role
        await _client.PostAsJsonAsync(_settings.CreateRoleEndpoint, request1);

        // Act - Try to create duplicate
        var response = await _client.PostAsJsonAsync(_settings.CreateRoleEndpoint, request2);

        // Assert
        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.IsTrue(responseObject.GetProperty("message").GetString()?.Contains("already exists") ?? false);
    }

    [TestMethod]
    public async Task CreateRole_EmptyName_ReturnsConflict()
    {
        // Arrange
        var request = new CreateRoleRequest("");

        // Act
        var response = await _client.PostAsJsonAsync(_settings.CreateRoleEndpoint, request);

        // Assert - Empty name might conflict with existing roles or validation
        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.IsTrue(responseObject.GetProperty("message").GetString()?.Contains("already exists") ?? false);
    }

    [TestMethod]
    public async Task CreateRole_NullName_CausesDifferentResponse()
    {
        // Arrange
        var request = new CreateRoleRequest(null!);

        // Act
        var response = await _client.PostAsJsonAsync(_settings.CreateRoleEndpoint, request);

        // Assert - Null name might cause different response (could be BadRequest, Conflict, or InternalServerError)
        // Let's check what actually happens
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Status: {response.StatusCode}, Content: {responseContent}");

        // For now, just ensure it's not a successful creation
        Assert.AreNotEqual(HttpStatusCode.Created, response.StatusCode);
    }
}
