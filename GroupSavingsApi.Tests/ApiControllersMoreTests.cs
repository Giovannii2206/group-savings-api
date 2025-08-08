using Xunit;
using GroupSavingsApi.Controllers;
using GroupSavingsApi.Models;
using GroupSavingsApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GroupSavingsApi.Data;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace GroupSavingsApi.Tests
{
    public class ApiControllersMoreTests
    {
        private GroupSavingsDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<GroupSavingsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new GroupSavingsDbContext(options);
        }

        [Fact]
        public async Task Contribution_CRUD_Works()
        {
            var db = GetInMemoryDbContext();
            // Need a GroupSession and User for foreign keys
            var user = new User { Id = Guid.NewGuid(), Email = "a@b.com", PasswordHash = "x", Role = "Member" };
            db.Users.Add(user);
            var group = new Group { Id = Guid.NewGuid(), Name = "G", CreatedBy = user.Id, Status = "Active" };
            db.Groups.Add(group);
            var session = new GroupSession { Id = Guid.NewGuid(), GroupId = group.Id, TargetAmount = 100, TargetDate = DateTime.UtcNow.AddDays(30), StartDate = DateTime.UtcNow, Frequency = "weekly", Status = "Active" };
            db.GroupSessions.Add(session);
            db.SaveChanges();
            var controller = new ContributionController(db);
            var createDto = new CreateContributionDto { GroupSessionId = session.Id, UserId = user.Id, Amount = 50, Type = "manual" };
            var createResult = await controller.CreateContribution(createDto);
            var createdResult = createResult.Result as CreatedAtActionResult;
            var created = createdResult?.Value as ContributionResponseDto;
            Assert.NotNull(created);
            var getResult = await controller.GetContribution(created.Id);
            var fetchedResult = getResult.Result as OkObjectResult;
            var fetched = fetchedResult?.Value as ContributionResponseDto;
            Assert.Equal(50, fetched.Amount);
            var listResult = await controller.GetContributions();
            var listOk = listResult.Result as OkObjectResult;
            var list = listOk?.Value as IEnumerable<ContributionResponseDto>;
            Assert.NotNull(list);
            Assert.Contains(list, c => c.Id == created.Id);
            var updateDto = new UpdateContributionDto { Amount = 60 };
            var updateResult = await controller.UpdateContribution(created.Id, updateDto);
            Assert.IsType<NoContentResult>(updateResult);
            var deleteResult = await controller.DeleteContribution(created.Id);
            Assert.IsType<NoContentResult>(deleteResult);
        }

        [Fact]
        public async Task GroupSession_CRUD_Works()
        {
            var db = GetInMemoryDbContext();
            var group = new Group { Id = Guid.NewGuid(), Name = "G", CreatedBy = Guid.NewGuid(), Status = "Active" };
            db.Groups.Add(group);
            db.SaveChanges();
            var controller = new GroupSessionController(db);
            var session = new GroupSession { Id = Guid.NewGuid(), GroupId = group.Id, TargetAmount = 100, TargetDate = DateTime.UtcNow.AddDays(30), StartDate = DateTime.UtcNow, Frequency = "weekly", Status = "Active" };
            db.GroupSessions.Add(session);
            db.SaveChanges();
            var getResult = await controller.GetGroupSession(session.Id);
            var fetchedResult = getResult.Result as OkObjectResult;
            var fetched = fetchedResult?.Value as GroupSessionResponseDto;
            Assert.Equal(session.Id, fetched.Id);
            var listResult = await controller.GetGroupSessions();
            var listOk = listResult.Result as OkObjectResult;
            var list = listOk?.Value as IEnumerable<GroupSessionResponseDto>;
            Assert.NotNull(list);
            Assert.Contains(list, s => s.Id == session.Id);
            var updateDto = new UpdateGroupSessionDto { TargetAmount = 200 };
            var updateResult = await controller.UpdateGroupSession(session.Id, updateDto);
            Assert.IsType<NoContentResult>(updateResult);
            var deleteResult = await controller.DeleteGroupSession(session.Id);
            Assert.IsType<NoContentResult>(deleteResult);
        }

        [Fact]
        public async Task SavingsGoal_CRUD_Works()
        {
            var db = GetInMemoryDbContext();
            var group = new Group { Id = Guid.NewGuid(), Name = "G", CreatedBy = Guid.NewGuid(), Status = "Active" };
            db.Groups.Add(group);
            var session = new GroupSession { Id = Guid.NewGuid(), GroupId = group.Id, TargetAmount = 100, TargetDate = DateTime.UtcNow.AddDays(30), StartDate = DateTime.UtcNow, Frequency = "weekly", Status = "Active" };
            db.GroupSessions.Add(session);
            db.SaveChanges();
            var controller = new SavingsGoalController(db);
            var createDto = new CreateSavingsGoalDto { GroupSessionId = session.Id, Name = "Goal", TargetAmount = 100, TargetDate = DateTime.UtcNow.AddDays(30) };
            var createResult = await controller.CreateGoal(createDto);
            var createdResult = createResult.Result as CreatedAtActionResult;
            var created = createdResult?.Value as SavingsGoalResponseDto;
            Assert.NotNull(created);
            var getResult = await controller.GetGoal(created.Id);
            var fetchedResult = getResult.Result as OkObjectResult;
            var fetched = fetchedResult?.Value as SavingsGoalResponseDto;
            Assert.Equal("Goal", fetched.Name);
            var listResult = await controller.GetGoals();
            var listOk = listResult.Result as OkObjectResult;
            var list = listOk?.Value as IEnumerable<SavingsGoalResponseDto>;
            Assert.NotNull(list);
            Assert.Contains(list, g => g.Id == created.Id);
            var updateDto = new CreateSavingsGoalDto { GroupSessionId = session.Id, Name = "UpdatedGoal", TargetAmount = 200, TargetDate = DateTime.UtcNow.AddDays(60) };
            var updateResult = await controller.UpdateGoal(created.Id, updateDto);
            Assert.IsType<NoContentResult>(updateResult);
            var deleteResult = await controller.DeleteGoal(created.Id);
            Assert.IsType<NoContentResult>(deleteResult);
        }

        [Fact]
        public async Task AccountType_CRUD_Works()
        {
            var db = GetInMemoryDbContext();
            var controller = new AccountTypeController(db);
            var createDto = new CreateAccountTypeDto { Name = "TypeA", Description = "desc" };
            var createResult = await controller.CreateAccountType(createDto);
            var createdResult = createResult.Result as CreatedAtActionResult;
            var created = createdResult?.Value as AccountTypeResponseDto;
            Assert.NotNull(created);
            var getResult = await controller.GetAccountType(created.Id);
            var fetchedResult = getResult.Result as OkObjectResult;
            var fetched = fetchedResult?.Value as AccountTypeResponseDto;
            Assert.Equal("TypeA", fetched.Name);
            var listResult = await controller.GetAccountTypes();
            var listOk = listResult.Result as OkObjectResult;
            var list = listOk?.Value as IEnumerable<AccountTypeResponseDto>;
            Assert.NotNull(list);
            Assert.Contains(list, t => t.Id == created.Id);
            var updateDto = new UpdateAccountTypeDto { Name = "TypeB" };
            var updateResult = await controller.UpdateAccountType(created.Id, updateDto);
            Assert.IsType<NoContentResult>(updateResult);
            var deleteResult = await controller.DeleteAccountType(created.Id);
            Assert.IsType<NoContentResult>(deleteResult);
        }

        [Fact]
        public async Task GroupRole_CRUD_Works()
        {
            var db = GetInMemoryDbContext();
            var controller = new GroupRoleController(db);
            var role = new GroupRole { Name = "RoleA", Description = "desc" };
            db.GroupRoles.Add(role);
            db.SaveChanges();
            var getResult = await controller.GetGroupRole(role.Id);
            var fetchedResult = getResult.Result as OkObjectResult;
            var fetched = fetchedResult?.Value as GroupRoleResponseDto;
            Assert.Equal("RoleA", fetched.Name);
            var listResult = await controller.GetGroupRoles();
            var listOk = listResult.Result as OkObjectResult;
            var list = listOk?.Value as IEnumerable<GroupRoleResponseDto>;
            Assert.NotNull(list);
            Assert.Contains(list, r => r.Id == role.Id);
            var updateDto = new UpdateGroupRoleDto { Name = "RoleB" };
            var updateResult = await controller.UpdateGroupRole(role.Id, updateDto);
            Assert.IsType<NoContentResult>(updateResult);
            var deleteResult = await controller.DeleteGroupRole(role.Id);
            Assert.IsType<NoContentResult>(deleteResult);
        }

        [Fact]
        public async Task AuditLog_CRUD_Works()
        {
            var db = GetInMemoryDbContext();
            var controller = new AuditLogController(db);
            var createDto = new CreateAuditLogDto { Action = "Test", EntityType = "Type", Details = "Details" };
            var createResult = await controller.CreateAuditLog(createDto);
            var createdResult = createResult.Result as CreatedAtActionResult;
            var created = createdResult?.Value as AuditLogResponseDto;
            Assert.NotNull(created);
            var getResult = await controller.GetAuditLog(created.Id);
            var fetchedResult = getResult.Result as OkObjectResult;
            var fetched = fetchedResult?.Value as AuditLogResponseDto;
            Assert.Equal("Test", fetched.Action);
            var listResult = await controller.GetAuditLogs();
            var listOk = listResult.Result as OkObjectResult;
            var list = listOk?.Value as IEnumerable<AuditLogResponseDto>;
            Assert.NotNull(list);
            Assert.Contains(list, l => l.Id == created.Id);
        }
    }
}
