using FluentAssertions;
using LightMES.Application.Features.Routes;
using LightMES.Application.Features.Routes.Dtos;
using LightMES.Domain.Constants;
using LightMES.Domain.Entities;
using LightMES.Infrastructure.Persistence;
using LightMES.Tests.Common;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace LightMES.Tests.Integration.Api;

public class RouteEndpointsTests : IntegrationTestBase
{
    public RouteEndpointsTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }
    private string GetUniqueRouteCode() => $"ROUTE-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    #region 查询测试
    [Fact]
    public async Task GetRoutes_ShouldReturnOkkAndList()
    {
        await ClearRoutesTableAsync();
        // Arrange
        await SeedRouteAsync(new Route(
              Guid.NewGuid(),
              GetUniqueRouteCode(),
              "测试工艺A",
              "v1.0",
              SystemConst.User.DefaultUser
            ));
        // Act
        var response = await _client.GetAsync("/api/routes");
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var routes = await response.Content.ReadFromJsonAsync<List<RouteListDto>>(TestJsonOptions.Default);
        routes.Should().NotBeNull();
        routes.Should().NotBeEmpty();
    }
    [Fact]
    public async Task GetRouteDetails_WhenExists_ShouldReturnOkAndDetails()
    {
        // Arrange
        var routeCode = GetUniqueRouteCode();
        var routeName = "高精密组装工艺";
        var routeId = Guid.NewGuid();
        await SeedRouteAsync(new Route(
            routeId,
            routeCode,
            routeName,
           "v1.0",
           SystemConst.User.DefaultUser
           ));
        // Act
        var response = await _client.GetAsync($"/api/routes/{routeId}");
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var routeDetail = await response.Content.ReadFromJsonAsync<RouteDetailDto>(TestJsonOptions.Default);
        routeDetail.Should().NotBeNull();
        routeDetail!.Id.Should().Be(routeId);
        routeDetail.RouteCode.Should().Be(routeCode);
        routeDetail.RouteName.Should().Be(routeName);
    }
    [Fact]
    public async Task GetRouteDetails_WhenDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistenId = Guid.NewGuid();
        // Act
        var response = await _client.GetAsync($"/api/routes/{nonExistenId}");
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    #endregion
    #region 创建测试
    public async Task CreateRoute_WithValidData_ShouldReturnCreatedAndRouteId()
    {
        // Arrange
        var routeCode = GetUniqueRouteCode();
        var command = new CreateRouteCommand(routeCode, "主板贴片工艺", "v2.0", null!, SystemConst.User.DefaultUser);
        // Act
        var response = await _client.PostAsJsonAsync("/api/routes", command, TestJsonOptions.Default);
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<CreateRouteResponse>(TestJsonOptions.Default);
        result.Should().NotBeNull();
        result!.RouteId.Should().NotBeEmpty();
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/api/routes");
    }
    #endregion
    #region 更新测试
    [Fact]
    public async Task UpdateRoute_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        var routeId = Guid.NewGuid();
        await SeedRouteAsync(
           new Route(
               routeId,
               GetUniqueRouteCode(),
               "旧工艺名称",
               "v1.0",
               SystemConst.User.DefaultUser
               )
           );
        var updateCode = GetUniqueRouteCode();
        var updateCommand = new UpdateRouteCommand(routeId, updateCode, "新工艺名称", "V2.0", true);
        // Act
        var response = await _client.PutAsJsonAsync($"/api/routes/{routeId}", updateCommand, TestJsonOptions.Default);
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/routes/{routeId}");
        var updateRoute = await getResponse.Content.ReadFromJsonAsync<RouteDetailDto>(TestJsonOptions.Default);
        updateRoute!.RouteName.Should().Be("新工艺名称");
        updateRoute.RouteCode.Should().Be(updateCode);
    }
    [Fact]
    public async Task UpdateRoute_IdMismatch_ShouldReturnBadRequest()
    {
        // Arrange
        var routeId = Guid.NewGuid();
        var differenId = Guid.NewGuid();
        var updateCommand = new UpdateRouteCommand(differenId, "R-Error", "不匹配的ID", "V1.0", true);
        // Act
        var response = await _client.PutAsJsonAsync($"/api/routes/{routeId}", updateCommand, TestJsonOptions.Default);
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("请求的 ID 与路由主键不一致");
    }
    [Fact]
    public async Task UpdateRoute_WhenDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateCommand = new UpdateRouteCommand(nonExistentId, "R-NOTFOUND", "不存在", "V2.0", false);
        // Act
        var response = await _client.PutAsJsonAsync($"/api/routes/{nonExistentId}", updateCommand, TestJsonOptions.Default);
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    #endregion
    #region 删除测试
    [Fact]
    public async Task DeleteRoute_WhenExists_ShouldReturnNoContentAndBeDeleted()
    {
        // Arrange
        await ClearRoutesTableAsync();
        var routeId = Guid.NewGuid();
        await SeedRouteAsync(new Route(
            routeId,
            "R-ToBeDeleted",
            "待删除工艺",
            "V1.0",
            SystemConst.User.DefaultUser
            ));
        // Act
        var response = await _client.DeleteAsync($"/api/routes/{routeId}");
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var getResponse = await _client.GetAsync($"/api/routes/{routeId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    [Fact]
    public async Task DeleteRoute_WhenDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        // Act
        var response = await _client.DeleteAsync($"/api/routes/{nonExistentId}");
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    #endregion

    #region 辅助方法 (Helper)
    private async Task SeedRouteAsync(Route route)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Routes.AddAsync(route);
        await db.SaveChangesAsync();
    }

    private async Task ClearRoutesTableAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Routes.RemoveRange(db.Routes);
        await db.SaveChangesAsync();
    }
    #endregion

}

public class CreateRouteResponse
{
    public Guid RouteId { get; set; }
};