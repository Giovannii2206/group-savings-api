using GroupSavingsApi.Controllers;
using GroupSavingsApi.Data;
using GroupSavingsApi.DTOs;
using GroupSavingsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;

namespace GroupSavingsApi.IntegrationTests.Controllers
{
    public class GroupControllerTests : IDisposable
    {
        private readonly GroupSavingsDbContext _context;
        private readonly GroupController _controller;

        public GroupControllerTests()
        {
            var options = new DbContextOptionsBuilder<GroupSavingsDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new GroupSavingsDbContext(options);
            _controller = new GroupController(_context);
            
            // Seed test data
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // Add test user
            var testUserId = Guid.NewGuid();
            var testUser = new User
            {
                Id = testUserId,
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                Role = "Admin"
            };
            _context.Users.Add(testUser);

            // Add test group
            var testGroupId = Guid.NewGuid();
            var testGroup = new Group
            {
                Id = testGroupId,
                Name = "Test Group",
                CreatedBy = testUser.Id,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };
            _context.Groups.Add(testGroup);

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetGroups_ReturnsAllGroups()
        {
            // Act
            var result = await _controller.GetGroups();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<GroupResponseDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnValue = Assert.IsType<List<GroupResponseDto>>(okResult.Value);
            Assert.Single(returnValue);
            Assert.Equal("Test Group", returnValue[0].Name);
        }

        [Fact]
        public async Task GetGroup_ExistingId_ReturnsGroup()
        {
            // Arrange
            var testGroup = await _context.Groups.FirstAsync();

            // Act
            var result = await _controller.GetGroup(testGroup.Id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<GroupResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var group = Assert.IsType<GroupResponseDto>(okResult.Value);
            Assert.Equal(testGroup.Id, group.Id);
            Assert.Equal("Test Group", group.Name);
        }

        [Fact]
        public async Task GetGroup_NonExistingId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetGroup(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
