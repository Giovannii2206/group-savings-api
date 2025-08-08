using Xunit;
using GroupSavingsApi.Controllers;
using GroupSavingsApi.Models;
using GroupSavingsApi.DTOs;
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
            var createDto = new CreateGroupDto { Name = "Test Group", CreatedBy = Guid.NewGuid(), Status = "Active" };

            // Act
            var createResult = await controller.CreateGroup(createDto);
            var created = (createResult.Result as CreatedAtActionResult)?.Value as GroupResponseDto;
            var getResult = await controller.GetGroup(created.Id);
            var fetched = (getResult.Result as OkObjectResult)?.Value as GroupResponseDto;

            // Assert
            Assert.NotNull(created);
            Assert.NotNull(fetched);
            Assert.Equal(createDto.Name, fetched.Name);
        }
    }
}
