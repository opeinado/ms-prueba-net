using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using CreditPro.Application.DTOs;
using CreditPro.Application.Exceptions;
using CreditPro.Application.Interfaces;
using CreditPro.Application.Services;
using CreditPro.Domain.Entities;
using CreditPro.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace CreditPro.Tests.Services;

public class CreditApplicationServiceTests
{
    private readonly Mock<ICreditApplicationRepository> _creditApplicationRepositoryMock = new();
    private readonly Mock<IAuditEventRepository> _auditEventRepositoryMock = new();
    private readonly CreditApplicationService _sut;

    public CreditApplicationServiceTests()
    {
        _sut = new CreditApplicationService(_creditApplicationRepositoryMock.Object, _auditEventRepositoryMock.Object);
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(150000)]
    [InlineData(999)]
    [InlineData(200000)]
    public async Task CreateAsync_WithInvalidAmount_ThrowsValidationException(decimal invalidAmount)
    {
        var request = new CreateCreditApplicationRequest
        {
            CustomerId = "123",
            CreditAmount = invalidAmount,
            ApplicationDate = DateTime.UtcNow
        };

        await FluentActions.Awaiting(() => _sut.CreateAsync(request)).Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("*monto solicitado*");
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_PersistsDataAndReturnsDto()
    {
        CreditApplication? persistedApplication = null;
        AuditEvent? persistedEvent = null;

        _creditApplicationRepositoryMock
            .Setup(repo => repo.CreateAsync(It.IsAny<CreditApplication>(), It.IsAny<CancellationToken>()))
            .Callback<CreditApplication, CancellationToken>((app, _) => persistedApplication = app)
            .Returns(Task.CompletedTask);

        _auditEventRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEvent, CancellationToken>((audit, _) => persistedEvent = audit)
            .Returns(Task.CompletedTask);

        var request = new CreateCreditApplicationRequest
        {
            CustomerId = "cust-1",
            CreditAmount = 5000,
            ApplicationDate = new DateTime(2024, 10, 10, 8, 30, 0, DateTimeKind.Utc),
            CollateralDescription = "Auto"
        };

        var result = await _sut.CreateAsync(request);

        result.CustomerId.Should().Be(request.CustomerId);
        result.Status.Should().Be(CreditApplicationStatus.Received);
        result.CollateralDescription.Should().Be(request.CollateralDescription);

        persistedApplication.Should().NotBeNull();
        persistedApplication!.CreditAmount.Should().Be(request.CreditAmount);
        persistedApplication.Status.Should().Be(CreditApplicationStatus.Received);

        persistedEvent.Should().NotBeNull();
        persistedEvent!.EventType.Should().Be("Creación");
        persistedEvent.NewState.Should().Be(CreditApplicationStatus.Received);
        persistedEvent.Details["creditAmount"]!.GetValue<decimal>().Should().Be(request.CreditAmount);
    }

    [Fact]
    public async Task UpdateStatusAsync_WithInvalidStatus_ThrowsValidationException()
    {
        var request = new UpdateCreditApplicationStatusRequest
        {
            NewStatus = "Foo"
        };

        await FluentActions.Awaiting(() => _sut.UpdateStatusAsync(Guid.NewGuid(), request))
            .Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("*no es válido*");
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenApplicationNotFound_ThrowsNotFoundException()
    {
        _creditApplicationRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreditApplication?)null);

        var request = new UpdateCreditApplicationStatusRequest
        {
            NewStatus = CreditApplicationStatus.Approved
        };

        await FluentActions.Awaiting(() => _sut.UpdateStatusAsync(Guid.NewGuid(), request))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsApplicationWithHistory()
    {
        var applicationId = Guid.NewGuid();
        var application = new CreditApplication(
            applicationId,
            "cust-2",
            25000,
            DateTime.UtcNow,
            CreditApplicationStatus.Received);

        var events = new List<AuditEvent>
        {
            new(applicationId, DateTime.UtcNow.AddMinutes(-10), "Creación", CreditApplicationStatus.Received, new JsonObject()),
            new(applicationId, DateTime.UtcNow, "Actualización de Estado", CreditApplicationStatus.Approved, new JsonObject())
        };

        _creditApplicationRepositoryMock
            .Setup(repo => repo.GetByIdAsync(applicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        _auditEventRepositoryMock
            .Setup(repo => repo.GetByApplicationIdAsync(applicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var result = await _sut.GetByIdAsync(applicationId);

        result.Application.ApplicationId.Should().Be(applicationId);
        result.History.Should().HaveCount(2);
    }
}
