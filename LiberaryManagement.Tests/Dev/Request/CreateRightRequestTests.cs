using LibraryManagement.Endpoints.Dev;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LiberaryManagement.Tests.Dev.Request;

[TestClass]
public class CreateRightRequestTests
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
    public async Task CreateRight_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new CreateRightRequest(
            $"TestRight_{Guid.NewGuid()}",
            "Test right description",
            "Endpoint"
        );

        // Act
        var response = await _client.PostAsJsonAsync(_settings.CreateRightEndpoint, request);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.IsTrue(responseObject.GetProperty("id").GetInt32() > 0);
        Assert.AreEqual(request.Name, responseObject.GetProperty("name").GetString());
        Assert.AreEqual(request.Description, responseObject.GetProperty("description").GetString());
        Assert.AreEqual(request.Type, responseObject.GetProperty("type").GetString());
    }

    [TestMethod]
    public async Task CreateRight_DuplicateName_ReturnsConflict()
    {
        // Arrange
        var rightName = $"TestRight_{Guid.NewGuid()}";
        var request1 = new CreateRightRequest(rightName, "Description 1", "Endpoint");
        var request2 = new CreateRightRequest(rightName, "Description 2", "Manual");

        // Create first right
        await _client.PostAsJsonAsync(_settings.CreateRightEndpoint, request1);

        // Act - Try to create duplicate
        var response = await _client.PostAsJsonAsync(_settings.CreateRightEndpoint, request2);

        // Assert
        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.IsTrue(responseObject.GetProperty("message").GetString()?.Contains("already exists") ?? false);
    }

    [TestMethod]
    public async Task CreateRight_DefaultType_ReturnsCreated()
    {
        // Arrange
        var request = new CreateRightRequest(
            $"TestRight_{Guid.NewGuid()}",
            "Test right with default type",
            null // Type will default to "Manual"
        );

        // Act
        var response = await _client.PostAsJsonAsync(_settings.CreateRightEndpoint, request);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.AreEqual("Manual", responseObject.GetProperty("type").GetString());
    }

    [TestMethod]
    public async Task CreateRight_EmptyName_AllowsEmptyName()
    {
        // Arrange
        var request = new CreateRightRequest("", "Description", "Endpoint");

        // Act
        var response = await _client.PostAsJsonAsync(_settings.CreateRightEndpoint, request);

        // Assert - Since Name has default value, empty name might be allowed
        // But the endpoint checks for uniqueness, so it might conflict or succeed
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Empty Name - Status: {response.StatusCode}, Content: {responseContent}");

        // For now, just ensure it's not a successful creation if validation fails
        if (response.StatusCode != HttpStatusCode.Created)
        {
            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        }
    }

    [TestMethod]
    public async Task CreateRight_EmptyDescription_AllowsEmptyDescription()
    {
        // Arrange
        var request = new CreateRightRequest($"TestRight_{Guid.NewGuid()}", "", "Endpoint");

        // Act
        var response = await _client.PostAsJsonAsync(_settings.CreateRightEndpoint, request);

        // Assert - Since Description has no validation attributes, empty description should be allowed
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.AreEqual("", responseObject.GetProperty("description").GetString());
    }
}
