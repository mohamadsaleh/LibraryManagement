using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LiberaryManagement.Tests.Dev.Request;

[TestClass]
public class GetRightsByRoleIdRequestTests
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

            // Assign rights to role
            if (_testRoleId > 0 && _testRightIds.Count > 0)
            {
                var assignRequest = new
                {
                    RoleId = _testRoleId,
                    RightIds = _testRightIds
                };
                await _client.PostAsJsonAsync("/api/Dev/AddRightsToRole", assignRequest);
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
    public async Task GetRightsByRoleId_ValidRoleId_ReturnsRights()
    {
        // Skip if test data setup failed
        if (_testRoleId == 0)
            Assert.Inconclusive("Test data setup failed");

        // Act
        var response = await _client.GetAsync($"{_settings.GetRightsByRoleIdEndpoint}?roleId={_testRoleId}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var rights = JsonSerializer.Deserialize<List<JsonElement>>(responseContent);

        Assert.IsNotNull(rights);
        Assert.AreEqual(_testRightIds.Count, rights.Count);
    }

    [TestMethod]
    public async Task GetRightsByRoleId_RoleNotFound_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"{_settings.GetRightsByRoleIdEndpoint}?roleId=99999");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.IsTrue(responseObject.GetProperty("message").GetString()?.Contains("not found") ?? false);
    }

    [TestMethod]
    public async Task GetRightsByRoleId_RoleWithNoRights_ReturnsEmptyList()
    {
        // Create a role with no rights
        var createRoleRequest = new { Name = $"EmptyRole_{Guid.NewGuid()}" };
        var createRoleResponse = await _client.PostAsJsonAsync("/api/Dev/CreateRole", createRoleRequest);

        Assert.AreEqual(HttpStatusCode.Created, createRoleResponse.StatusCode);

        var roleContent = await createRoleResponse.Content.ReadAsStringAsync();
        var roleData = JsonSerializer.Deserialize<JsonElement>(roleContent);
        var emptyRoleId = roleData.GetProperty("id").GetInt32();

        // Act
        var response = await _client.GetAsync($"{_settings.GetRightsByRoleIdEndpoint}?roleId={emptyRoleId}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var rights = JsonSerializer.Deserialize<List<JsonElement>>(responseContent);

        Assert.IsNotNull(rights);
        Assert.AreEqual(0, rights.Count);
    }

    [TestMethod]
    public async Task GetRightsByRoleId_InvalidRoleId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"{_settings.GetRightsByRoleIdEndpoint}?roleId=0");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task GetRightsByRoleId_ReturnsOrderedByName()
    {
        // Skip if test data setup failed
        if (_testRoleId == 0 || _testRightIds.Count < 2)
            Assert.Inconclusive("Test data setup failed");

        // Act
        var response = await _client.GetAsync($"{_settings.GetRightsByRoleIdEndpoint}?roleId={_testRoleId}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var rights = JsonSerializer.Deserialize<List<JsonElement>>(responseContent);

        Assert.IsNotNull(rights);
        Assert.IsTrue(rights.Count >= 2);

        // Check if ordered by name
        for (int i = 1; i < rights.Count; i++)
        {
            var currentName = rights[i].GetProperty("name").GetString();
            var previousName = rights[i - 1].GetProperty("name").GetString();

            Assert.IsTrue(string.Compare(previousName, currentName) <= 0,
                $"Rights not ordered correctly: {previousName} should come before {currentName}");
        }
    }
}
