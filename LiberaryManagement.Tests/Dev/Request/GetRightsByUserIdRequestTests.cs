using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LiberaryManagement.Tests.Dev.Request;

[TestClass]
public class GetRightsByUserIdRequestTests
{
    private HttpClient _client = null!;
    private TestSettings _settings = null!;
    private int _testUserId = 0;
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

        // Setup test data by creating a test user, role, and rights
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

            // Assign rights to role
            if (_testRoleId > 0 && _testRightIds.Count > 0)
            {
                var assignRightsRequest = new
                {
                    RoleId = _testRoleId,
                    RightIds = _testRightIds
                };
                await _client.PostAsJsonAsync("/api/Dev/AddRightsToRole", assignRightsRequest);
            }

            // Assign role to user
            if (_testUserId > 0 && _testRoleId > 0)
            {
                var assignRoleRequest = new
                {
                    UserId = _testUserId,
                    RoleIds = new List<int> { _testRoleId }
                };
                await _client.PostAsJsonAsync("/api/Dev/AddRolesToUser", assignRoleRequest);
            }
        }
        catch
        {
            // If setup fails, use fallback IDs (might not exist, but tests will handle it)
            _testUserId = 1;
            _testRoleId = 1;
            _testRightIds = new List<int> { 1, 2 };
        }
    }

    [TestMethod]
    public async Task GetRightsByUserId_ValidUserId_ReturnsRights()
    {
        // Skip if test data setup failed
        if (_testUserId == 0)
            Assert.Inconclusive("Test data setup failed");

        // Act
        var response = await _client.GetAsync($"{_settings.GetRightsByUserIdEndpoint}?userId={_testUserId}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var rights = JsonSerializer.Deserialize<List<JsonElement>>(responseContent);

        Assert.IsNotNull(rights);
        Assert.AreEqual(_testRightIds.Count, rights.Count);
    }

    [TestMethod]
    public async Task GetRightsByUserId_UserNotFound_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"{_settings.GetRightsByUserIdEndpoint}?userId=99999");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.IsTrue(responseObject.GetProperty("message").GetString()?.Contains("not found") ?? false);
    }

    [TestMethod]
    public async Task GetRightsByUserId_UserWithNoRights_ReturnsMessage()
    {
        // Create a user with no roles/rights
        var createUserRequest = new
        {
            DisplayName = $"Empty User {Guid.NewGuid()}",
            Username = $"emptyuser_{Guid.NewGuid()}",
            Password = "TestPassword123!"
        };
        var createUserResponse = await _client.PostAsJsonAsync("/api/Dev/CreateUser", createUserRequest);

        Assert.AreEqual(HttpStatusCode.Created, createUserResponse.StatusCode);

        var userContent = await createUserResponse.Content.ReadAsStringAsync();
        var userData = JsonSerializer.Deserialize<JsonElement>(userContent);
        var emptyUserId = userData.GetProperty("id").GetInt32();

        // Act
        var response = await _client.GetAsync($"{_settings.GetRightsByUserIdEndpoint}?userId={emptyUserId}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.IsTrue(responseObject.GetProperty("message").GetString()?.Contains("has no rights") ?? false);
        var rights = responseObject.GetProperty("rights");
        Assert.AreEqual(0, rights.GetArrayLength());
    }

    [TestMethod]
    public async Task GetRightsByUserId_InvalidUserId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"{_settings.GetRightsByUserIdEndpoint}?userId=0");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task GetRightsByUserId_ReturnsDistinctRights()
    {
        // Skip if test data setup failed
        if (_testUserId == 0 || _testRoleId == 0)
            Assert.Inconclusive("Test data setup failed");

        // Create another role with some overlapping rights
        var createRoleRequest = new { Name = $"TestRole2_{Guid.NewGuid()}" };
        var createRoleResponse = await _client.PostAsJsonAsync("/api/Dev/CreateRole", createRoleRequest);

        Assert.AreEqual(HttpStatusCode.Created, createRoleResponse.StatusCode);

        var roleContent = await createRoleResponse.Content.ReadAsStringAsync();
        var roleData = JsonSerializer.Deserialize<JsonElement>(roleContent);
        var secondRoleId = roleData.GetProperty("id").GetInt32();

        // Assign same rights to second role
        var assignRightsRequest = new
        {
            RoleId = secondRoleId,
            RightIds = _testRightIds
        };
        await _client.PostAsJsonAsync("/api/Dev/AddRightsToRole", assignRightsRequest);

        // Assign both roles to user
        var assignRolesRequest = new
        {
            UserId = _testUserId,
            RoleIds = new List<int> { _testRoleId, secondRoleId }
        };
        await _client.PostAsJsonAsync("/api/Dev/AddRolesToUser", assignRolesRequest);

        // Act
        var response = await _client.GetAsync($"{_settings.GetRightsByUserIdEndpoint}?userId={_testUserId}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var rights = JsonSerializer.Deserialize<List<JsonElement>>(responseContent);

        Assert.IsNotNull(rights);
        // Should still return distinct rights (no duplicates)
        Assert.AreEqual(_testRightIds.Count, rights.Count);
    }
}
