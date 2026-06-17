using FluentAssertions;
using LightMES.Application.Features.Materials;
using LightMES.Application.Features.Materials.Dtos;
using LightMES.Domain.Enums;
using LightMES.Tests.Common;
using System.Net.Http.Json;

namespace LightMES.Tests.Integration.Api;

public class MaterialEndpointsTests : IntegrationTestBase
{
    public MaterialEndpointsTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private string GetUniqueMaterialCode() => $"MAT-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    [Fact]
    public async Task CreateMaterial_WithValidData_ShouldReturnOkAndGuid()
    {
        // Arrange
        var command = new CreateMaterialCommand(
            GetUniqueMaterialCode(),
            "测试钢板",
            "T=2.0mm",
            "张",
            MaterialType.RawMaterial
            );
        // Act
        var response = await _client.PostAsJsonAsync("/api/materials", command);
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>();
        createdId.Should().NotBeEmpty();
    }
    [Fact]
    public async Task GetMaterialById_WhenItemExists_ShouldReturnMaterialDetails()
    {
        // Arrange
        var code = GetUniqueMaterialCode();
        var createCmd = new CreateMaterialCommand(code, "拉丝不锈钢", "1000*2000", "张", MaterialType.RawMaterial);
        var createResponse = await _client.PostAsJsonAsync("/api/materials", createCmd);
        var createdId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        // Act
        var getResponse = await _client.GetAsync($"/api/materials/{createdId}");
        // Assert
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var material = await getResponse.Content.ReadFromJsonAsync<MaterialDto>(TestJsonOptions.Default);
        material.Should().NotBeNull();
        material!.Id.Should().Be(createdId);
        material.MaterialCode.Should().Be(code);
    }
    [Fact]
    public async Task GetMaterialById_WhenItemDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        // Act
        var nonExistentId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/materials/{nonExistentId}");
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
    [Fact]
    public async Task UpdateMaterial_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        var createCmd = new CreateMaterialCommand(GetUniqueMaterialCode(), "原始物料名称", "10A", "KG", MaterialType.RawMaterial);
        var createResponse = await _client.PostAsJsonAsync("/api/materials", createCmd);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();
        var updateCmd = new UpdateMaterialCommand(
            id,
            "更新物料名称",
            "20B",
            "吨",
            MaterialType.FinishedGood
            );
        // Act
        var response = await _client.PutAsJsonAsync($"/api/materials/{id}", updateCmd);
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
        var getResponse = await _client.GetAsync($"/api/materials/{id}");
        var updatedMaterial = await getResponse.Content.ReadFromJsonAsync<MaterialDto>(TestJsonOptions.Default);
        updatedMaterial!.MaterialName.Should().Be("更新物料名称");
        updatedMaterial.Unit.Should().Be("吨");
    }
    [Fact]
    public async Task ActivateAndDeactivate_ShouldToggleIsActiveStatus()
    {
        // Arrange
        var createCmd = new CreateMaterialCommand(GetUniqueMaterialCode(), "开关测试料", "X", "PCS", MaterialType.RawMaterial);
        var createResponse = await _client.PostAsJsonAsync("/api/materials", createCmd);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();
        // Act
        var deactivateResponse = await _client.PutAsync($"/api/materials/{id}/deactivate", null);
        deactivateResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
        // Assert
        var getDeactivateResult = await _client.GetFromJsonAsync<MaterialDto>($"/api/materials/{id}", TestJsonOptions.Default);
        getDeactivateResult!.IsActive.Should().BeFalse();
        var activateResponse = await _client.PutAsync($"/api/materials/{id}/activate", null);
        activateResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
        var getActiveResult = await _client.GetFromJsonAsync<MaterialDto>($"/api/materials/{id}", TestJsonOptions.Default);
        getActiveResult!.IsActive.Should().BeTrue();
    }
}
