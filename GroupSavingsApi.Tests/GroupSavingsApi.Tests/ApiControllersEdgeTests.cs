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
    public class ApiControllersEdgeTests
    {
        private GroupSavingsDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<GroupSavingsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new GroupSavingsDbContext(options);
        }

        [Fact]
        public async Task UserController_DuplicateEmail_ReturnsBadRequest()
        {
            var db = GetInMemoryDbContext();
            var controller = new UserController(db);
            var dto = new CreateUserDto { Email = "dup@x.com", Password = "pw", Role = "User" };
            var result1 = await controller.CreateUser(dto);
            var result2 = await controller.CreateUser(dto);
            Assert.IsType<CreatedAtActionResult>(result1.Result);
            Assert.IsType<BadRequestObjectResult>(result2.Result);
        }

        [Fact]
        public async Task GroupController_PaginationAndFilter_Works()
        {
            var db = GetInMemoryDbContext();
            var controller = new GroupController(db);
            for (int i = 0; i < 30; i++)
                db.Groups.Add(new Group { Id = Guid.NewGuid(), Name = i % 2 == 0 ? "Alpha" : "Beta", CreatedBy = Guid.NewGuid(), Status = "Active", CreatedAt = DateTime.UtcNow });
            db.SaveChanges();
            var result = await controller.GetGroups(page: 2, pageSize: 10, name: "Alpha");
            var ok = result.Result as OkObjectResult;
            var list = ok?.Value as IEnumerable<GroupResponseDto>;
            Assert.NotNull(list);
            Assert.All(list, g => Assert.Contains("Alpha", g.Name));
            Assert.True(list.Count() <= 10);
        }

        [Fact]
        public async Task MemberController_SoftDelete_HidesFromList()
        {
            var db = GetInMemoryDbContext();
            var controller = new MemberController(db);
            var user = new User { Id = Guid.NewGuid(), Email = "softdelete@test.com", PasswordHash = "pw", Role = "Member" };
            db.Users.Add(user);
            db.SaveChanges();
            var memberDto = new CreateMemberDto { UserId = user.Id, FirstName = "X", LastName = "Y" };
            var createResult = await controller.CreateMember(memberDto);
            var created = (createResult.Result as CreatedAtActionResult)?.Value as MemberResponseDto;
            Assert.NotNull(created);
            await controller.DeleteMember(created.Id);
            var listResult = await controller.GetMembers();
            var listOk = listResult.Result as OkObjectResult;
            var list = listOk?.Value as IEnumerable<MemberResponseDto>;
            Assert.NotNull(list);
            Assert.DoesNotContain(list, m => m.Id == created.Id);
        }

        [Fact]
        public async Task SavingsGoalController_SoftDelete_HidesFromList()
        {
            var db = GetInMemoryDbContext();
            var group = new Group { Id = Guid.NewGuid(), Name = "G", CreatedBy = Guid.NewGuid(), Status = "Active" };
            db.Groups.Add(group);
            var session = new GroupSession { Id = Guid.NewGuid(), GroupId = group.Id, TargetAmount = 100, TargetDate = DateTime.UtcNow.AddDays(30), StartDate = DateTime.UtcNow, Frequency = "weekly", Status = "Active" };
            db.GroupSessions.Add(session);
            var goal = new SavingsGoal { Id = Guid.NewGuid(), GroupSessionId = session.Id, Name = "Goal", TargetAmount = 100, TargetDate = DateTime.UtcNow.AddDays(30), IsDeleted = false };
            db.SavingsGoals.Add(goal);
            db.SaveChanges();
            var controller = new SavingsGoalController(db);
            await controller.DeleteGoal(goal.Id);
            var listResult = await controller.GetGoals();
            var listOk = listResult.Result as OkObjectResult;
            var list = listOk?.Value as IEnumerable<SavingsGoalResponseDto>;
            Assert.NotNull(list);
            Assert.DoesNotContain(list, g => g.Id == goal.Id);
        }

        [Fact]
        public async Task ContributionReportsController_ByUser_BySession_ByGroup()
        {
            var db = GetInMemoryDbContext();
            var user = new User { Id = Guid.NewGuid(), Email = "a@b.com", PasswordHash = "x", Role = "Member" };
            db.Users.Add(user);
            var group = new Group { Id = Guid.NewGuid(), Name = "G", CreatedBy = user.Id, Status = "Active" };
            db.Groups.Add(group);
            var session = new GroupSession { Id = Guid.NewGuid(), GroupId = group.Id, TargetAmount = 100, TargetDate = DateTime.UtcNow.AddDays(30), StartDate = DateTime.UtcNow, Frequency = "weekly", Status = "Active" };
            db.GroupSessions.Add(session);
            db.SaveChanges();
            db.Contributions.Add(new Contribution { Id = Guid.NewGuid(), GroupSessionId = session.Id, UserId = user.Id, Amount = 10, Type = "manual", IsDeleted = false });
            db.Contributions.Add(new Contribution { Id = Guid.NewGuid(), GroupSessionId = session.Id, UserId = user.Id, Amount = 20, Type = "manual", IsDeleted = false });
            db.SaveChanges();
            var controller = new ContributionReportsController(db);
            var byUser = await controller.GetContributionsByUser();
            var bySession = await controller.GetContributionsBySession();
            var byGroup = await controller.GetContributionsByGroup();
            Assert.IsType<OkObjectResult>(byUser);
            Assert.IsType<OkObjectResult>(bySession);
            Assert.IsType<OkObjectResult>(byGroup);
        }
    }
}
