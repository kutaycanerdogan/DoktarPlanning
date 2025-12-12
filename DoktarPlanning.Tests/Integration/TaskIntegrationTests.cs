using DoktarPlanning.Application.Services;
using DoktarPlanning.Data.Repositories;
using DoktarPlanning.Domain.Common;
using DoktarPlanning.Infrastructure.DTOs;
using DoktarPlanning.Infrastructure.Interfaces;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DoktarPlanning.Tests.Integration
{
    public class TaskIntegrationTests : IClassFixture<TestApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestApplicationFactory _factory;
        private readonly Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public TaskIntegrationTests(TestApplicationFactory factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            _factory = factory;
        }

        [Fact]
        public async Task Task_Flow_Create_List_Complete()
        {
            var createDto = new TaskDto
            {
                Title = "Integration Test Task" + DateTime.Now,
                Description = "Test Desc",
                DueAt = DateTime.Now.AddDays(1),
                Priority = Priority.Medium,
                UserId = userId
            };

            var createResponse = await _client.PostAsJsonAsync("/api/tasks", createDto);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var created = await createResponse.Content.ReadFromJsonAsync<TaskDto>();
            created.Should().NotBeNull();
            created!.Id.Should().NotBe(Guid.Empty);

            var listResponse = await _client.GetAsync("/api/tasks?from=2025-01-01&to=2030-01-01");
            listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var list = await listResponse.Content.ReadFromJsonAsync<IEnumerable<TaskDto>>();
            list.Should().Contain(t => t.Title.Contains("Integration Test Task"));

            var completeResponse = await _client.PutAsync($"/api/tasks/{created.Id}/complete?isComplete=true", null);
            completeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await _client.GetAsync($"/api/tasks/{created.Id}");
            var task = await getResponse.Content.ReadFromJsonAsync<TaskDto>();

            task!.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task Recurrence_ShouldGenerateInstances()
        {
            var ruleDto = new RecurrenceDto
            {
                Frequency = Frequency.Daily,
                Interval = 1,
                UserId = userId,
            };

            var ruleResponse = await _client.PostAsJsonAsync("/api/recurrence", ruleDto);
            var rule = await ruleResponse.Content.ReadFromJsonAsync<RecurrenceDto>();

            using var scope = _factory.Services.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<IRecurrenceJobRunner>();

            await runner.RunAsync(userId, rule!.Id);

            var listResponse = await _client.GetAsync("/api/tasks");
            var tasks = await listResponse.Content.ReadFromJsonAsync<IEnumerable<TaskDto>>();

            tasks.Should().NotBeEmpty();
        }
    }
}