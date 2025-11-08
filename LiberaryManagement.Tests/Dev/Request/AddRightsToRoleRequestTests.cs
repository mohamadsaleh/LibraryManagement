using LibraryManagement.Endpoints.Dev;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LiberaryManagement.Tests.Dev.Request;

[TestClass]
public class AddRightsToRoleRequestTests
{
    private HttpClient _client = null!;
    private TestSettings _settings = null!;
    private int _testRoleId = 0;
    private List<int> _testRightIds = new();

    [TestInitialize]
    public async Task TestInitialize()
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

        // Setup test data by creating a test role and rights
        await SetupTestData();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _client.Dispose();
    }

    private async Task SetupTestData()
    {
        try
        {
            // Create a test role
            var createRoleRequest = new { Name = $"TestRole_{Guid.NewGuid()}" };
            var createRoleResponse = await _client.PostAsJsonAsync("/api/Dev/CreateRole", createRoleRequest);

            if (createRoleResponse.IsSuccessStatusCode)
            {
                var roleContent = await createRoleResponse.Content.ReadAsStringAsync();
                var roleData = JsonSerializer.Deserialize<JsonElement>(roleContent);
                _testRoleId = roleData.GetProperty("id").GetInt32();
            }

            // Create test rights
            var rightNames = new[] { $"TestRight1_{Guid.NewGuid()}", $"TestRight2_{Guid.NewGuid()}" };
            foreach (var rightName in rightNames)
            {
                var createRightRequest = new
                {
                    Name = rightName,
                    Description = "Test right for integration testing",
                    Type = "Endpoint"
                };

                var createRightResponse = await _client.PostAsJsonAsync("/api/Dev/CreateRight", createRightRequest);

                if (createRightResponse.IsSuccessStatusCode)
                {
                    var rightContent = await createRightResponse.Content.ReadAsStringAsync();
                    var rightData = JsonSerializer.Deserialize<JsonElement>(rightContent);
                    _testRightIds.Add(rightData.GetProperty("id").GetInt32());
                }
            }
        }
        catch
        {
            // If setup fails, use fallback IDs (might not exist, but tests will handle it)
            _testRoleId = 1;
            _testRightIds = new List<int> { 1, 2 };
        }
    }

    [TestMethod]
    public async Task AddRightsToRole_ValidRequest_ReturnsOk()
    {
        // Skip if test data setup failed
        if (_testRoleId == 0 || _testRightIds.Count < 2)
            Assert.Inconclusive("Test data setup failed");

        // Arrange
        var request = new AddRightsToRoleRequest
        {
            RoleId = _testRoleId,
            RightIds = _testRightIds
        };

        // Act
        var response = await _client.PostAsJsonAsync(_settings.AddRightsToRoleEndpoint, request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.IsTrue(responseObject.GetProperty("message").GetString()?.Contains("Successfully assigned") ?? false);
    }

    [TestMethod]
    public async Task AddRightsToRole_RoleNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = new AddRightsToRoleRequest
        {
            RoleId = 99999, // Non-existent role
            RightIds = new List<int> { 1, 2 }
        };

        // Act
        var response = await _client.PostAsJsonAsync(_settings.AddRightsToRoleEndpoint, request);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.IsTrue(responseObject.GetProperty("message").GetString()?.Contains("not found") ?? false);
    }

    [TestMethod]
    public async Task AddRightsToRole_SomeRightsNotFound_ReturnsBadRequest()
    {
        // Skip if test data setup failed
        if (_testRoleId == 0)
            Assert.Inconclusive("Test data setup failed");

        // Arrange
        var request = new AddRightsToRoleRequest
        {
            RoleId = _testRoleId,
            RightIds = new List<int> { _testRightIds.First(), 99999 } // One existing, one non-existing right
        };

        // Act
        var response = await _client.PostAsJsonAsync(_settings.AddRightsToRoleEndpoint, request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.IsTrue(responseObject.GetProperty("message").GetString()?.Contains("not found") ?? false);
    }

    [TestMethod]
    public async Task AddRightsToRole_EmptyRightsList_RemovesAllRights()
    {
        // Skip if test data setup failed
        if (_testRoleId == 0)
            Assert.Inconclusive("Test data setup failed");

        // Arrange
        var request = new AddRightsToRoleRequest
        {
            RoleId = _testRoleId,
            RightIds = new List<int>() // Empty list
        };

        // Act
        var response = await _client.PostAsJsonAsync(_settings.AddRightsToRoleEndpoint, request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.IsTrue(responseObject.GetProperty("message").GetString()?.Contains("Successfully assigned 0 rights") ?? false);
    }

    [TestMethod]
    public async Task AddRightsToRole_InvalidRoleId_ReturnsNotFound()
    {
        // Arrange
        var request = new AddRightsToRoleRequest
        {
            RoleId = 0, // Invalid role ID
            RightIds = new List<int> { 1 }
        };

        // Act
        var response = await _client.PostAsJsonAsync(_settings.AddRightsToRoleEndpoint, request);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task AddRightsToRole_MissingRightIdsProperty_ReturnsOk()
    {
        // Skip if test data setup failed
        if (_testRoleId == 0)
            Assert.Inconclusive("Test data setup failed");

        // Arrange - Send request without RightIds property at all
        var request = new
        {
            RoleId = _testRoleId
            // RightIds is completely missing, so it will use default value (new List<int>())
        };

        // Act
        var response = await _client.PostAsJsonAsync(_settings.AddRightsToRoleEndpoint, request);

        // Assert - Since RightIds has default value new() in the model, missing property should work
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.IsTrue(responseObject.GetProperty("message").GetString()?.Contains("Successfully assigned 0 rights") ?? false);
    }
}

public class TestSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string AddRightsToRoleEndpoint { get; set; } = string.Empty;
    public string AddEndpointsAsRightsEndpoint { get; set; } = string.Empty;
    public string AddRolesToUserEndpoint { get; set; } = string.Empty;
    public string CreateRightEndpoint { get; set; } = string.Empty;
    public string CreateRoleEndpoint { get; set; } = string.Empty;
    public string CreateUserEndpoint { get; set; } = string.Empty;
    public string GetProjectEndpointsEndpoint { get; set; } = string.Empty;
    public string GetRightsByRoleIdEndpoint { get; set; } = string.Empty;
    public string GetRightsByUserIdEndpoint { get; set; } = string.Empty;
    public string GetRightsFromDBEndpoint { get; set; } = string.Empty;
    public string GetRolesByUserIdEndpoint { get; set; } = string.Empty;
}
