using Infrastructure.Common;
using Infrastructure.Services;
using Infrastructure.Skills.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using universal_scheduler_be.Controllers;

namespace universal_scheduler_be.Tests.Controllers;

public class SkillControllerTests
{
    private static readonly SkillResponse SampleResponse = new()
    {
        Id = Guid.Parse("d4000001-0000-4000-8000-000000000001"),
        Name = "Brakes",
        Description = "Brake repair and maintenance"
    };

    [Fact]
    public async Task GetAll_ReturnsOkWhenServiceSucceeds()
    {
        var skills = new List<SkillResponse> { SampleResponse };
        var skillService = new Mock<ISkillService>();
        skillService
            .Setup(service => service.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<IReadOnlyList<SkillResponse>>.Ok(skills));

        var controller = new SkillController(skillService.Object);

        var actionResult = await controller.GetAll(CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        Assert.Same(skills, objectResult.Value);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtActionWhenServiceSucceeds()
    {
        var skillService = new Mock<ISkillService>();
        skillService
            .Setup(service => service.CreateAsync(It.IsAny<CreateSkillRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<SkillResponse>.Created(SampleResponse));

        var controller = new SkillController(skillService.Object);

        var actionResult = await controller.Create(new CreateSkillRequest(), CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal(nameof(SkillController.GetAll), createdResult.ActionName);
        Assert.Same(SampleResponse, createdResult.Value);
    }

    [Fact]
    public async Task Delete_ReturnsNoContentWhenServiceSucceeds()
    {
        var skillService = new Mock<ISkillService>();
        skillService
            .Setup(service => service.DeleteAsync(SampleResponse.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<object>.NoContent());

        var controller = new SkillController(skillService.Object);

        var actionResult = await controller.Delete(SampleResponse.Id, CancellationToken.None);

        Assert.IsType<NoContentResult>(actionResult);
    }

    [Fact]
    public async Task Delete_ReturnsConflictWhenServiceFails()
    {
        var skillService = new Mock<ISkillService>();
        skillService
            .Setup(service => service.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<object>.Conflict("Skill is in use and cannot be deleted."));

        var controller = new SkillController(skillService.Object);

        var actionResult = await controller.Delete(Guid.NewGuid(), CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status409Conflict, problemResult.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(problemResult.Value);
        Assert.Equal("Conflict", problem.Title);
    }
}
