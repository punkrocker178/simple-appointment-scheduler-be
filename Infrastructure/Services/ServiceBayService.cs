using Infrastructure.Common;
using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.ServiceBays.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ServiceBayService : IServiceBayService
{
    private readonly ApplicationDbContext _db;

    public ServiceBayService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult<IReadOnlyList<ServiceBayResponse>>> GetByDealershipAsync(
        Guid dealershipId,
        CancellationToken cancellationToken = default)
    {
        if (!await _db.Dealerships.AnyAsync(d => d.Id == dealershipId, cancellationToken))
        {
            return ServiceResult<IReadOnlyList<ServiceBayResponse>>.NotFound("Dealership not found.");
        }

        var serviceBays = await _db.ServiceBays
            .AsNoTracking()
            .Where(sb => sb.DealershipId == dealershipId)
            .OrderBy(sb => sb.Name)
            .Select(sb => ToResponse(sb))
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyList<ServiceBayResponse>>.Ok(serviceBays);
    }

    public async Task<ServiceResult<ServiceBayResponse>> CreateAsync(
        Guid dealershipId,
        CreateServiceBayRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await _db.Dealerships.AnyAsync(d => d.Id == dealershipId, cancellationToken))
        {
            return ServiceResult<ServiceBayResponse>.NotFound("Dealership not found.");
        }

        var validationError = ValidateName(request.Name);
        if (validationError is not null)
        {
            return ServiceResult<ServiceBayResponse>.BadRequest(validationError);
        }

        var serviceBay = new ServiceBay
        {
            Id = Guid.NewGuid(),
            DealershipId = dealershipId,
            Name = request.Name.Trim(),
            IsActive = true
        };

        _db.ServiceBays.Add(serviceBay);
        await _db.SaveChangesAsync(cancellationToken);

        return ServiceResult<ServiceBayResponse>.Created(ToResponse(serviceBay));
    }

    public async Task<ServiceResult<ServiceBayResponse>> UpdateAsync(
        Guid dealershipId,
        Guid id,
        UpdateServiceBayRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateName(request.Name);
        if (validationError is not null)
        {
            return ServiceResult<ServiceBayResponse>.BadRequest(validationError);
        }

        var serviceBay = await _db.ServiceBays
            .FirstOrDefaultAsync(sb => sb.Id == id && sb.DealershipId == dealershipId, cancellationToken);

        if (serviceBay is null)
        {
            return ServiceResult<ServiceBayResponse>.NotFound("Service bay not found.");
        }

        serviceBay.Name = request.Name.Trim();
        await _db.SaveChangesAsync(cancellationToken);

        return ServiceResult<ServiceBayResponse>.Ok(ToResponse(serviceBay));
    }

    public async Task<ServiceResult<object>> SoftDeleteAsync(
        Guid dealershipId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var serviceBay = await _db.ServiceBays
            .FirstOrDefaultAsync(sb => sb.Id == id && sb.DealershipId == dealershipId, cancellationToken);

        if (serviceBay is null)
        {
            return ServiceResult<object>.NotFound("Service bay not found.");
        }

        if (!serviceBay.IsActive)
        {
            return ServiceResult<object>.BadRequest("Service bay is already inactive.");
        }

        serviceBay.IsActive = false;
        await _db.SaveChangesAsync(cancellationToken);

        return ServiceResult<object>.NoContent();
    }

    private static string? ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Name is required.";
        }

        return null;
    }

    private static ServiceBayResponse ToResponse(ServiceBay serviceBay) =>
        new()
        {
            Id = serviceBay.Id,
            DealershipId = serviceBay.DealershipId,
            Name = serviceBay.Name,
            IsActive = serviceBay.IsActive
        };
}
