using FluentAssertions;
using LightMES.Application.Common.Interfaces;
using LightMES.Application.Features.Equipments;
using LightMES.Domain.Entities;
using NSubstitute;

namespace LightMES.Tests.Application;

public class CreateEquipmentCommandHandlerTests
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly CreateEquipmentCommandHandler _handler;
    private readonly CreateEquipmentCommandValidator _validator;

    public CreateEquipmentCommandHandlerTests()
    {
        _context = Substitute.For<IAppDbContext>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _currentUserService.UserId.Returns("test-user-admin");
        _handler = new CreateEquipmentCommandHandler(_context, _currentUserService);
        _validator = new CreateEquipmentCommandValidator(_context);
    }

    [Fact]
    public async Task Handle_Should_CreateEquipment_When_RequestIsValid()
    {
        //Arrange
        var command = new CreateEquipmentCommand(
            "EQ-TEST-001",
            "测试设备",
            "测试车间",
            "单元测试描述"
        );
        //Act
        var result = await _handler.Handle(command, CancellationToken.None);
        //Assert
        result.Should().NotBeEmpty();
        await _context
            .Received(1)
            .Equipments.AddAsync(
                Arg.Is<Equipment>(e =>
                    e.EquipmentCode == command.EquipmentCode
                    && e.EquipmentName == command.EquipmentName
                ),
                Arg.Any<CancellationToken>()
            );
        await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validator_WhenCodeIsEmpty_ShouldHaveValidationError(string? invalidCode)
    {
        //Arrange
        var command = new CreateEquipmentCommand(invalidCode!, "Test", "", "");
        //Act
        var result = await _validator.ValidateAsync(command);
        //Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EquipmentCode");
    }
}
