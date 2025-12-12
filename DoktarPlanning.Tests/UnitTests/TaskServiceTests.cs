using AutoMapper;

using DoktarPlanning.Application.Services;
using DoktarPlanning.Domain.Common;
using DoktarPlanning.Domain.Entities;
using DoktarPlanning.Domain.Interfaces;
using DoktarPlanning.Infrastructure.DTOs;
using DoktarPlanning.Infrastructure.Interfaces;
using DoktarPlanning.Infrastructure.Repositories.Interfaces;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace DoktarPlanning.Tests.UnitTests
{
    public class TaskServiceTests
    {
        private static IMapper CreateMapper()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
            });

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TaskItem, TaskDto>().ReverseMap();
                cfg.CreateMap<RecurrenceRule, RecurrenceDto>().ReverseMap();
            }, loggerFactory);

            return config.CreateMapper();
        }


        private static ILogger<T> CreateLogger<T>()
        {
            return Mock.Of<ILogger<T>>();
        }

        private static TaskService CreateTaskService(
            ITaskRepository? taskRepo = null,
            IRepository<TaskItem>? genericRepo = null,
            IUserRepository? userRepo = null,
            IRecurrenceRuleRepository? recurrenceRepo = null,
            IBackgroundJobService? jobs = null,
            IEmailSender? email = null,
            IReminderService? reminder = null,
            ISubTaskRepository? subTaskRepo = null
            )
        {
            return new TaskService(
                genericRepo ?? Mock.Of<IRepository<TaskItem>>(),
                taskRepo ?? Mock.Of<ITaskRepository>(),
                userRepo ?? Mock.Of<IUserRepository>(),
                recurrenceRepo ?? Mock.Of<IRecurrenceRuleRepository>(),
                CreateMapper(),
                CreateLogger<TaskService>(),
                jobs ?? Mock.Of<IBackgroundJobService>(),
                email ?? Mock.Of<IEmailSender>(),
                reminder ?? Mock.Of<IReminderService>(),
                recurrenceJobRunner: Mock.Of<IRecurrenceJobRunner>(),
                subTaskRepo ?? Mock.Of<ISubTaskRepository>()
            );
        }

        // ✅ TEST 1 — Duplicate title
        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenTitleExists()
        {
            var repo = new Mock<ITaskRepository>();
            repo.Setup(r => r.FindAsync(
                    It.IsAny<Expression<Func<TaskItem, bool>>>(),
                    null, null, null, default))
                .ReturnsAsync(new[] { new TaskItem { Title = "Test" } });

            var svc = CreateTaskService(taskRepo: repo.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                svc.CreateAsync(Guid.NewGuid(), new TaskDto { Title = "Test" }));
        }

        // ✅ TEST 2 — Recurrence daily interval
        [Fact]
        public async Task GenerateInstancesAsync_DailyInterval2_ShouldGenerateCorrectDates()
        {
            var rule = new RecurrenceRule
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Frequency = Frequency.Daily,
                Interval = 2,
                CreatedAt = new DateTime(2025, 1, 1)
            };

            var repo = new Mock<IRecurrenceRuleRepository>();
            repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<RecurrenceRule, bool>>>(), default))
                .ReturnsAsync(new[] { rule });

            var svc = new RecurrenceService(
                Mock.Of<IRepository<RecurrenceRule>>(),
                repo.Object,
                Mock.Of<IBackgroundJobService>(),
                CreateMapper(),
                CreateLogger<RecurrenceService>()
            );

            var result = await svc.GenerateInstancesAsync(
                rule.UserId,
                rule.Id,
                new DateTime(2025, 1, 1),
                new DateTime(2025, 1, 10));

            var dueDates = result.Select(r => r.DueAt!.Value.Date).ToList();

            dueDates.Should().BeEquivalentTo(new[]
            {
                new DateTime(2025,1,1),
                new DateTime(2025,1,3),
                new DateTime(2025,1,5),
                new DateTime(2025,1,7),
                new DateTime(2025,1,9)
            });
        }

        // ✅ TEST 3 — Reminder scheduling
        [Fact]
        public async Task ScheduleReminderAsync_ShouldScheduleEmailJob()
        {
            var task = new TaskItem { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Title = "T" };

            var repo = new Mock<IRepository<TaskItem>>();
            repo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<TaskItem, bool>>>(), default))
                .ReturnsAsync(task);

            var jobs = new Mock<IBackgroundJobService>();
            jobs.Setup(j => j.ScheduleAsync(
                    It.IsAny<Expression<Func<Task>>>(),
                    It.IsAny<TimeSpan>(),
                    default))
                .ReturnsAsync("job-1");

            var svc = new ReminderService(
                jobs.Object,
                Mock.Of<IEmailSender>(),
                repo.Object,
                CreateLogger<ReminderService>(),
                Mock.Of<IWebhookSender>()
            );

            await svc.ScheduleReminderAsync(
                task.UserId,
                task.Id,
                DateTime.Now.AddHours(1),
                ReminderChannel.Email,
                "a@b.com");

            jobs.Verify(j =>
                j.ScheduleAsync(
                    It.IsAny<Expression<Func<Task>>>(),
                    It.IsAny<TimeSpan>(),
                    default),
                Times.Once);
        }
    }
}