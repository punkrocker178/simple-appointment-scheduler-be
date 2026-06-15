using Infrastructure.Common;
using Infrastructure.Skills.Dtos;

namespace Infrastructure.Services;

public interface ISkillService
{
    Task<ServiceResult<IReadOnlyList<SkillResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<SkillResponse>> CreateAsync(CreateSkillRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<object>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
