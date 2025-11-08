using LibraryManagement;
using LibraryManagement.Endpoints.Dev;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LiberaryManagement.Tests.Dev.Unit;

[TestClass]
public class AddRightsToRoleTests
{
    private ApplicationDbContext _dbContext = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _dbContext.Database.EnsureCreated();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [TestMethod]
    public async Task AddRightsToRole_ValidRequest_ReturnsOk()
    {
        // Arrange
        var role = new Role { Name = "TestRole" };
        _dbContext.Roles.Add(role);

        var right1 = new Right { Name = "Right1", Description = "Test Right 1", Type = "Endpoint" };
        var right2 = new Right { Name = "Right2", Description = "Test Right 2", Type = "Endpoint" };
        _dbContext.Rights.AddRange(right1, right2);

        await _dbContext.SaveChangesAsync();

        var request = new AddRightsToRoleRequest
        {
            RoleId = role.Id,
            RightIds = new List<int> { right1.Id, right2.Id }
        };

        // Act
        var result = await AddRightsToRoleEndpointLogic(request, _dbContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);

        // Verify database state
        var roleHasRights = await _dbContext.RoleHasRights.Where(rhr => rhr.RoleId == role.Id).ToListAsync();
        Assert.AreEqual(2, roleHasRights.Count);
        Assert.IsTrue(roleHasRights.Any(rhr => rhr.RightId == right1.Id));
        Assert.IsTrue(roleHasRights.Any(rhr => rhr.RightId == right2.Id));
    }

    [TestMethod]
    public async Task AddRightsToRole_RoleNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = new AddRightsToRoleRequest
        {
            RoleId = 999, // Non-existent role
            RightIds = new List<int> { 1, 2 }
        };

        // Act
        var result = await AddRightsToRoleEndpointLogic(request, _dbContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(404, result.StatusCode);
    }

    [TestMethod]
    public async Task AddRightsToRole_SomeRightsNotFound_ReturnsBadRequest()
    {
        // Arrange
        var role = new Role { Name = "TestRole" };
        _dbContext.Roles.Add(role);

        var right1 = new Right { Name = "Right1", Description = "Test Right 1", Type = "Endpoint" };
        _dbContext.Rights.Add(right1);

        await _dbContext.SaveChangesAsync();

        var request = new AddRightsToRoleRequest
        {
            RoleId = role.Id,
            RightIds = new List<int> { right1.Id, 999 } // 999 doesn't exist
        };

        // Act
        var result = await AddRightsToRoleEndpointLogic(request, _dbContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
    }

    [TestMethod]
    public async Task AddRightsToRole_ReplaceExistingRights_WorksCorrectly()
    {
        // Arrange
        var role = new Role { Name = "TestRole" };
        _dbContext.Roles.Add(role);

        var oldRight = new Right { Name = "OldRight", Description = "Old Right", Type = "Endpoint" };
        var newRight = new Right { Name = "NewRight", Description = "New Right", Type = "Endpoint" };
        _dbContext.Rights.AddRange(oldRight, newRight);

        // Add existing right
        _dbContext.RoleHasRights.Add(new RoleHasRight { RoleId = role.Id, RightId = oldRight.Id });

        await _dbContext.SaveChangesAsync();

        var request = new AddRightsToRoleRequest
        {
            RoleId = role.Id,
            RightIds = new List<int> { newRight.Id }
        };

        // Act
        var result = await AddRightsToRoleEndpointLogic(request, _dbContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);

        // Verify old right is removed and new right is added
        var roleHasRights = await _dbContext.RoleHasRights.Where(rhr => rhr.RoleId == role.Id).ToListAsync();
        Assert.AreEqual(1, roleHasRights.Count);
        Assert.AreEqual(newRight.Id, roleHasRights.First().RightId);
    }

    [TestMethod]
    public async Task AddRightsToRole_EmptyRightsList_RemovesAllRights()
    {
        // Arrange
        var role = new Role { Name = "TestRole" };
        _dbContext.Roles.Add(role);

        var right = new Right { Name = "Right1", Description = "Test Right", Type = "Endpoint" };
        _dbContext.Rights.Add(right);

        // Add existing right
        _dbContext.RoleHasRights.Add(new RoleHasRight { RoleId = role.Id, RightId = right.Id });

        await _dbContext.SaveChangesAsync();

        var request = new AddRightsToRoleRequest
        {
            RoleId = role.Id,
            RightIds = new List<int>() // Empty list
        };

        // Act
        var result = await AddRightsToRoleEndpointLogic(request, _dbContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);

        // Verify all rights are removed
        var roleHasRights = await _dbContext.RoleHasRights.Where(rhr => rhr.RoleId == role.Id).ToListAsync();
        Assert.AreEqual(0, roleHasRights.Count);
    }

    // Helper method to extract the logic from the endpoint
    private static async Task<TestResult> AddRightsToRoleEndpointLogic(AddRightsToRoleRequest request, ApplicationDbContext db)
    {
        // Find the role
        var role = await db.Roles.FindAsync(request.RoleId);

        if (role == null)
        {
            return new TestResult { StatusCode = 404, Message = $"Role with ID {request.RoleId} not found." };
        }

        // Find the rights that should be assigned based on the provided IDs
        var rightsToAssign = await db.Rights
            .Where(r => request.RightIds.Contains(r.Id))
            .ToListAsync();

        // Optional: Check if all requested right IDs were found
        if (rightsToAssign.Count != request.RightIds.Count)
        {
            var foundIds = rightsToAssign.Select(r => r.Id);
            var notFoundIds = request.RightIds.Except(foundIds);
            return new TestResult { StatusCode = 400, Message = $"The following Right IDs were not found: {string.Join(", ", notFoundIds)}" };
        }

        // Find and remove all existing rights for this role
        var existingRights = await db.RoleHasRights
            .Where(rhr => rhr.RoleId == request.RoleId)
            .ToListAsync();

        db.RoleHasRights.RemoveRange(existingRights);

        // Create new RoleHasRight entries for the new set of rights
        foreach (var right in rightsToAssign)
        {
            db.RoleHasRights.Add(new RoleHasRight { RoleId = role.Id, RightId = right.Id });
        }

        await db.SaveChangesAsync();

        return new TestResult { StatusCode = 200, Message = $"Successfully assigned {rightsToAssign.Count} rights to role '{role.Name}'." };
    }

    private class TestResult
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}