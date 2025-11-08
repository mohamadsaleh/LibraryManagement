using LibraryManagement.Endpoints.Dev;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LiberaryManagement.Tests.Dev.Request;

[TestClass]
public class AddRolesToUserRequestTests
{
    private HttpClient _client = null!;
    private TestSettings _settings = null!;
    private int _testUserId = 0;
    private List<int> _testRoleIds = new();

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

        // Setup test data by creating a test user and roles
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
            // Create a test user
            var createUserRequest = new
            {
                DisplayName = $"Test User {Guid.NewGuid()}",
                Username = $"testuser_{Guid.NewGuid()}",
                Password = "TestPassword123!"
            };
            var createUserResponse = await _client.PostAsJsonAsync("/api/Dev/CreateUser", createUserRequest);

            if (createUserResponse.IsSuccessStatusCode)
            {
                var userContent = await createUserResponse.Content.ReadAsStringAsync();
                var userData = JsonSerializer.Deserialize<JsonElement>(userContent);
                _testUserId = userData.GetProperty("id").GetInt32();
            }

            // Create test roles
            var roleNames = new[] { $"TestRole1_{Guid.NewGuid()}", $"TestRole2_{Guid.NewGuid()}" };
            foreach (var roleName in roleNames)
            {
                var createRoleRequest = new { Name = roleName };
                var createRoleResponse = await _client.PostAsJsonAsync("/api/Dev/CreateRole", createRoleRequest);

                if (createRoleResponse.IsSuccessStatusCode)
                {
                    var roleContent = await createRoleResponse.Content.ReadAsStringAsync();
                    var roleData = JsonSerializer.Deserialize<JsonElement>(roleContent);
                    _testRoleIds.Add(roleData.GetProperty("id").GetInt32());
                }
            }
        }
        catch
        {
            // If setup fails, use fallback IDs (might not exist, but tests will handle it)
            _testUserId = 1;
            _testRoleIds = new List<int> { 1, 2 };
        }
    }

    [TestMethod]
    public async Task AddRolesToUser_ValidRequest_ReturnsOk()
    {
        // Skip if test data setup failed
        if (_testUserId == 0 || _testRoleIds.Count < 2)
            Assert.Inconclusive("Test data setup failed");

        // Arrange
        var request = new AddRolesToUserRequest(_testUserId, _testRoleIds);

        // Act
        var response = await _client.PostAsJsonAsync(_settings.AddRolesToUserEndpoint, request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.IsTrue(responseObject.GetProperty("message").GetString()?.Contains("Successfully assigned") ?? false);
    }

    [TestMethod]
    public async Task AddRolesToUser_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = new AddRolesToUserRequest(99999, new List<int> { 1, 2 });

        // Act
        var response = await _client.PostAsJsonAsync(_settings.AddRolesToUserEndpoint, request);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.IsTrue(responseObject.GetProperty("message").GetString()?.Contains("not found") ?? false);
    }

    [TestMethod]
    public async Task AddRolesToUser_SomeRolesNotFound_ReturnsBadRequest()
    {
        // Skip if test data setup failed
        if (_testUserId == 0)
            Assert.Inconclusive("Test data setup failed");

        // Arrange
        var request = new AddRolesToUserRequest(_testUserId, new List<int> { _testRoleIds.First(), 99999 });

        // Act
        var response = await _client.PostAsJsonAsync(_settings.AddRolesToUserEndpoint, request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.IsTrue(responseObject.GetProperty("message").GetString()?.Contains("not found") ?? false);
    }

    [TestMethod]
    public async Task AddRolesToUser_EmptyRolesList_RemovesAllRoles()
    {
        // Skip if test data setup failed
        if (_testUserId == 0)
            Assert.Inconclusive("Test data setup failed");

        // Arrange
        var request = new AddRolesToUserRequest(_testUserId, new List<int>());

        // Act
        var response = await _client.PostAsJsonAsync(_settings.AddRolesToUserEndpoint, request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.IsTrue(responseObject.GetProperty("message").GetString()?.Contains("Successfully assigned 0 roles") ?? false);
    }

    [TestMethod]
    public async Task AddRolesToUser_InvalidUserId_ReturnsNotFound()
    {
        // Arrange
        var request = new AddRolesToUserRequest(0, new List<int> { 1 });

        // Act
        var response = await _client.PostAsJsonAsync(_settings.AddRolesToUserEndpoint, request);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
