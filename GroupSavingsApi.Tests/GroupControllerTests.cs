using Xunit;
using GroupSavingsApi.Controllers;
using GroupSavingsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GroupSavingsApi.Data;
using System;
using System.Threading.Tasks;

namespace GroupSavingsApi.Tests
{
    public class GroupControllerTests
    {
        private GroupController GetControllerWithInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<GroupSavingsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var dbContext = new GroupSavingsDbContext(options);
            return new GroupController(dbContext);
        }

        [Fact]
        public async Task Can_Create_And_Get_Group()
        {
            // Arrange
            var controller = GetControllerWithInMemoryDb();
            var group = new Group { Name = "Test Group", Description = "Unit test group" };

            // Act
            var createResult = await controller.CreateGroup(group);
            var created = (createResult as CreatedAtActionResult)?.Value as Group;
            var getResult = await controller.GetGroup(created.Id);
            var fetched = (getResult as OkObjectResult)?.Value as Group;

            // Assert
            Assert.NotNull(created);
            Assert.NotNull(fetched);
            Assert.Equal(group.Name, fetched.Name);
        }
    }
}
