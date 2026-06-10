using FluentAssertions;
using LightMES.Application.Common.Models;
using LightMES.Application.Features.Equipments;
using LightMES.Application.Features.Equipments.Dtos;
using LightMES.Domain.Entities;
using LightMES.Domain.Enums;
using LightMES.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LightMES.Tests.Api;

public class EquipmentEndpointTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = {new JsonStringEnumConverter()}
    };
    public EquipmentEndpointTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }
    #region 1. 创建测试 (Create)
    [Fact]
    public async Task Create_ShouldReturnCreated_WithValidData()
    {
        //Arrange
        var command = new
        {
            EquipmentCode = "EQ-INTEG-01",
            EquipmentName = "集成测试切削机",
            Location = "B车间"
        };
        //Act
        var response = await _client.PostAsJsonAsync("/api/equipments", command);
        //Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var equipment = db.Equipments.FirstOrDefault(e => e.EquipmentCode == "EQ-INTEG-01");
        equipment.Should().NotBeNull();
        equipment!.EquipmentName.Should().Be("集成测试切削机");
    }
    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenCodeAlreadyExists()
    {
        //Arrange
        var existingCode = "EQ-DUP";
        await SeedEquipmentAsync(new Equipment(Guid.NewGuid(), existingCode, "旧设备", "", "", "System"));
        var command = new CreateEquipmentCommand
        (
            existingCode,
            "新设备",
            "",
            "");
        //ACT
        var response = await _client.PostAsJsonAsync("/api/equipments", command);
        //Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("设备编码已存在.");
    }
    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenRequiredFieldsAreMissing()
    {
        //Arrange
        var invalidCommand = new
        {
            EquipmentCode = "",
            EquipmentName = ""
        };
        //ACT
        var response = await _client.PostAsJsonAsync("/api/equipments", invalidCommand);
        //Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task Create_WithValidCommand_ShouldReturnCreatedAndId()
    {
        //Arrange
        var command = new CreateEquipmentCommand("CNC-03", "New CNC Machine", null, null);
        //Act
        var response = await _client.PostAsJsonAsync("/api/equipments", command);
        //Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        var id = await response.Content.ReadFromJsonAsync<Guid>();
        id.Should().NotBeEmpty();
    }
    #endregion
    #region 2. 查询测试 (Read)
    [Fact]
    public async Task GetById_ShouldReturnEquipment_WhenEquipmentExists()
    {
        //Arrange
        var equipment = new Equipment(Guid.NewGuid(), "EQ-002", "注塑机 #2", "", "", "System");
        await SeedEquipmentAsync(equipment);
        //Act
        var response = await _client.GetAsync($"/api/equipments/{equipment.Id}");
        //Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<Equipment>();
        dto.Should().NotBeNull();
        dto!.EquipmentCode.Should().Be("EQ-002");
    }
    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenEquipmentDoseNotExist()
    {
        //Act
        var response = await _client.GetAsync($"/api/equipments/{Guid.NewGuid()}");
        //Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
    [Fact]
    public async Task GetWithPagination_ShouldReturnPaginatedList_AndSupportFiltering()
    {
        //Arrange
        await ClearEquipmentsTableAsync();
        await SeedEquipmentAsync(new Equipment(Guid.NewGuid(), "CNC-01", "CNC A", "", "", "System"));
        await SeedEquipmentAsync(new Equipment(Guid.NewGuid(), "CNC-02", "CNC B", "", "", "System"));
        await SeedEquipmentAsync(new Equipment(Guid.NewGuid(), "PLC-01", "PLC A", "", "", "System"));
        //Act
        var response = await _client.GetAsync("/api/equipments?PageNumber=1&PageSize=10&SearchText=CNC");
        //Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedList<EquipmentDto>>(JsonSerializerOptions);

        result.Should().NotBeNull();
        result!.TotalCount.Should().Be(2);
        result.Items.Should().OnlyContain(e => e.EquipmentCode.StartsWith("CNC"));
    }
    [Fact]
    public async Task GetWithPagination_ShouldReturnPaginateList()
    {
        //Act
        await ClearEquipmentsTableAsync();
        var response = await _client.GetAsync("/api/equipments?PageNumber=1&PageSize=10");
        //Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var result = await response.Content!.ReadFromJsonAsync<PaginatedList<EquipmentDto>>(JsonSerializerOptions);
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
    }
    #endregion
    #region 3. 更新测试 (Update)
    [Fact]
    public async Task Update_ShouldUpdateDatabase_WhenDataIsValid()
    {
        //Arrange
        var equipment = new Equipment(Guid.NewGuid(), "EQ-003", "原始名称", "", "", "System");
        equipment.ChangeStatus(EquipmentStatus.Maintenance, "System");
        await SeedEquipmentAsync(equipment);
        var command = new UpdateEquipmentCommand(equipment.Id, "修改后的名称", "", "");
        //Act
        var response = await _client.PutAsJsonAsync($"/api/equipments/{equipment.Id}", command);
        //Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var updated = await db.Equipments.FindAsync(equipment.Id);
        updated!.EquipmentName.Should().Be("修改后的名称");
        updated.Status.Should().Be(EquipmentStatus.Maintenance);
    }
    #endregion

    #region 辅助方法 (Helper)
    private async Task SeedEquipmentAsync(Equipment equipment)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Equipments.AddAsync(equipment);
        await db.SaveChangesAsync();
    }
    private async Task ClearEquipmentsTableAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Equipments.RemoveRange(db.Equipments);
        await db.SaveChangesAsync();
    }
    #endregion
}
