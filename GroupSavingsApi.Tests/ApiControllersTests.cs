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
    public class ApiControllersTests
    {
        private GroupSavingsDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<GroupSavingsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new GroupSavingsDbContext(options);
        }

        [Fact]
        public async Task UserController_CRUD_Works()
        {
            var db = GetInMemoryDbContext();
            var controller = new UserController(db);
            var createDto = new CreateUserDto { Email = "unit@test.com", Password = "pass", Role = "user" };
            var createResult = await controller.CreateUser(createDto);
            var createdResult = createResult.Result as CreatedAtActionResult;
            var created = createdResult?.Value as UserResponseDto;
            Assert.NotNull(created);
            var getResult = await controller.GetUser(created.Id);
            var fetchedResult = getResult.Result as OkObjectResult;
            var fetched = fetchedResult?.Value as UserResponseDto;
            Assert.Equal("unit@test.com", fetched.Email);
            var listResult = await controller.GetUsers();
            var listOk = listResult.Result as OkObjectResult;
            var list = listOk?.Value as IEnumerable<UserResponseDto>;
            Assert.NotNull(list);
            Assert.Contains(list, u => u.Id == created.Id);
            var updateDto = new UpdateUserDto { Role = "admin", IsActive = false };
            var updateResult = await controller.UpdateUser(created.Id, updateDto);
            Assert.IsType<NoContentResult>(updateResult);
            var deleteResult = await controller.DeleteUser(created.Id);
            Assert.IsType<NoContentResult>(deleteResult);
        }

        [Fact]
        public async Task GroupController_CRUD_Works()
        {
            var db = GetInMemoryDbContext();
            var controller = new GroupController(db);
            var createDto = new CreateGroupDto { Name = "Test Group", CreatedBy = Guid.NewGuid(), Status = "Active" };
            var createResult = await controller.CreateGroup(createDto);
            var created = (createResult.Result as CreatedAtActionResult)?.Value as GroupResponseDto;
            Assert.NotNull(created);
            var getResult = await controller.GetGroup(created.Id);
            var fetched = (getResult.Result as OkObjectResult)?.Value as GroupResponseDto;
            Assert.Equal("Test Group", fetched.Name);
            var listResult = await controller.GetGroups();
            var listOk = listResult.Result as OkObjectResult;
            var list = listOk?.Value as IEnumerable<GroupResponseDto>;
            Assert.NotNull(list);
            Assert.Contains(list, g => g.Id == created.Id);
            var updateDto = new UpdateGroupDto { Name = "Updated Group" };
            var updateResult = await controller.UpdateGroup(created.Id, updateDto);
            Assert.IsType<NoContentResult>(updateResult);
            var deleteResult = await controller.DeleteGroup(created.Id);
            Assert.IsType<NoContentResult>(deleteResult);
        }

        [Fact]
        public async Task MemberController_CRUD_Works()
        {
            var db = GetInMemoryDbContext();
            var controller = new MemberController(db);
            var user = new User { Id = Guid.NewGuid(), Email = "member@test.com", PasswordHash = "pw", Role = "Member" };
            db.Users.Add(user);
            db.SaveChanges();
            var memberDto = new CreateMemberDto { UserId = user.Id, FirstName = "A", LastName = "B" };
            var createResult = await controller.CreateMember(memberDto);
            var created = (createResult.Result as CreatedAtActionResult)?.Value as MemberResponseDto;
            Assert.NotNull(created);
            var getResult = await controller.GetMember(created.Id);
            var fetched = (getResult.Result as OkObjectResult)?.Value as MemberResponseDto;
            Assert.Equal("A", fetched.FirstName);
            var listResult = await controller.GetMembers();
            var listOk = listResult.Result as OkObjectResult;
            var list = listOk?.Value as IEnumerable<MemberResponseDto>;
            Assert.NotNull(list);
            Assert.Contains(list, m => m.Id == created.Id);
            var updateDto = new UpdateMemberDto { FirstName = "C" };
            var updateResult = await controller.UpdateMember(created.Id, updateDto);
            Assert.IsType<NoContentResult>(updateResult);
            var deleteResult = await controller.DeleteMember(created.Id);
            Assert.IsType<NoContentResult>(deleteResult);
        }

        [Fact]
        public async Task PaymentMethodController_CRUD_Works()
        {
            var db = GetInMemoryDbContext();
            var controller = new PaymentMethodController(db);
            var method = new CreatePaymentMethodDto { CustomerId = Guid.NewGuid(), AccountTypeId = 1, ProviderName = "Bank", AccountName = "Name", AccountNumber = "123", Currency = "USD", CountryCode = "US" };
            var createResult = await controller.CreatePaymentMethod(method);
            var createdResult = createResult.Result as CreatedAtActionResult;
            var created = createdResult?.Value as PaymentMethodResponseDto;
            Assert.NotNull(created);
            var getResult = await controller.GetPaymentMethod(created.Id);
            var fetchedResult = getResult.Result as OkObjectResult;
            var fetched = fetchedResult?.Value as PaymentMethodResponseDto;
            Assert.Equal("Bank", fetched.ProviderName);
            var listResult = await controller.GetPaymentMethods();
            var listOk = listResult.Result as OkObjectResult;
            var list = listOk?.Value as IEnumerable<PaymentMethodResponseDto>;
            Assert.NotNull(list);
            Assert.Contains(list, p => p.Id == created.Id);
            var updateDto = new UpdatePaymentMethodDto { ProviderName = "NewBank" };
            var updateResult = await controller.UpdatePaymentMethod(created.Id, updateDto);
            Assert.IsType<NoContentResult>(updateResult);
            var deleteResult = await controller.DeletePaymentMethod(created.Id);
            Assert.IsType<NoContentResult>(deleteResult);
        }
    }
}
