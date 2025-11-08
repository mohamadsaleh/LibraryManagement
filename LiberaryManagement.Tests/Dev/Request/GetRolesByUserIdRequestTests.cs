using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LiberaryManagement.Tests.Dev.Request;

[TestClass]
public class GetRolesByUserIdRequestTests
{
    private HttpClient _client = null!;
    private TestSettings _settings = null!;
    private int _testUserId = 0;
    private int _testRoleId = 0;

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

        // Setup test data by creating a test user and role
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
        }
    }

    [TestMethod]
    public async Task GetRolesByUserId_ValidUserId_ReturnsRoles()
    {
        // Skip if test data setup failed
        if (_testUserId == 0)
            Assert.Inconclusive("Test data setup failed");

        // Act
        var response = await _client.GetAsync($"{_settings.GetRolesByUserIdEndpoint}?userId={_testUserId}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var roles = JsonSerializer.Deserialize<List<JsonElement>>(responseContent);

        Assert.IsNotNull(roles);
        Assert.AreEqual(1, roles.Count);
    }

    [TestMethod]
    public async Task GetRolesByUserId_UserNotFound_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"{_settings.GetRolesByUserIdEndpoint}?userId=99999");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.IsTrue(responseObject.GetProperty("message").GetString()?.Contains("not found") ?? false);
    }

    [TestMethod]
    public async Task GetRolesByUserId_UserWithNoRoles_ReturnsEmptyList()
    {
        // Create a user with no roles
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
        var response = await _client.GetAsync($"{_settings.GetRolesByUserIdEndpoint}?userId={emptyUserId}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var roles = JsonSerializer.Deserialize<List<JsonElement>>(responseContent);

        Assert.IsNotNull(roles);
        Assert.AreEqual(0, roles.Count);
    }

    [TestMethod]
    public async Task GetRolesByUserId_InvalidUserId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"{_settings.GetRolesByUserIdEndpoint}?userId=0");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task GetRolesByUserId_ReturnsOrderedByName()
    {
        // Skip if test data setup failed
        if (_testUserId == 0)
            Assert.Inconclusive("Test data setup failed");

        // Create another role and assign it to the user
        var createRoleRequest = new { Name = $"ATestRole_{Guid.NewGuid()}" }; // Name starts with 'A' to test ordering
        var createRoleResponse = await _client.PostAsJsonAsync("/api/Dev/CreateRole", createRoleRequest);

        Assert.AreEqual(HttpStatusCode.Created, createRoleResponse.StatusCode);

        var roleContent = await createRoleResponse.Content.ReadAsStringAsync();
        var roleData = JsonSerializer.Deserialize<JsonElement>(roleContent);
        var secondRoleId = roleData.GetProperty("id").GetInt32();

        // Assign second role to user
        var assignRoleRequest = new
        {
            UserId = _testUserId,
            RoleIds = new List<int> { _testRoleId, secondRoleId }
        };
        await _client.PostAsJsonAsync("/api/Dev/AddRolesToUser", assignRoleRequest);

        // Act
        var response = await _client.GetAsync($"{_settings.GetRolesByUserIdEndpoint}?userId={_testUserId}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var roles = JsonSerializer.Deserialize<List<JsonElement>>(responseContent);

        Assert.IsNotNull(roles);
        Assert.AreEqual(2, roles.Count);

        // Check if ordered by name (second role should come first due to 'A' prefix)
        var firstRoleName = roles[0].GetProperty("name").GetString();
        var secondRoleName = roles[1].GetProperty("name").GetString();

        Assert.IsTrue(string.Compare(firstRoleName, secondRoleName) <= 0,
            $"Roles not ordered correctly: {firstRoleName} should come before {secondRoleName}");
    }
}
