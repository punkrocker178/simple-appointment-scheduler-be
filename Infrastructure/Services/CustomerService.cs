using Infrastructure.Common;
using Infrastructure.Customers.Dtos;
using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _db;

    public CustomerService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult<IReadOnlyList<CustomerResponse>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var customers = await _db.Customers
            .AsNoTracking()
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .Select(c => ToResponse(c))
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyList<CustomerResponse>>.Ok(customers);
    }

    public async Task<ServiceResult<CustomerResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var customer = await _db.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (customer is null)
        {
            return ServiceResult<CustomerResponse>.NotFound("Customer not found.");
        }

        return ServiceResult<CustomerResponse>.Ok(ToResponse(customer));
    }

    public async Task<ServiceResult<CustomerResponse>> CreateAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateRequest(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone);

        if (validationError is not null)
        {
            return ServiceResult<CustomerResponse>.BadRequest(validationError);
        }

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(cancellationToken);

        return ServiceResult<CustomerResponse>.Created(ToResponse(customer));
    }

    public async Task<ServiceResult<CustomerResponse>> UpdateAsync(
        Guid id,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateRequest(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone);

        if (validationError is not null)
        {
            return ServiceResult<CustomerResponse>.BadRequest(validationError);
        }

        var customer = await _db.Customers.FindAsync([id], cancellationToken);
        if (customer is null)
        {
            return ServiceResult<CustomerResponse>.NotFound("Customer not found.");
        }

        customer.FirstName = request.FirstName.Trim();
        customer.LastName = request.LastName.Trim();
        customer.Email = request.Email.Trim();
        customer.Phone = request.Phone.Trim();

        await _db.SaveChangesAsync(cancellationToken);

        return ServiceResult<CustomerResponse>.Ok(ToResponse(customer));
    }

    private static string? ValidateRequest(
        string firstName,
        string lastName,
        string email,
        string phone)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return "First name is required.";
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return "Last name is required.";
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return "Email is required.";
        }

        if (string.IsNullOrWhiteSpace(phone))
        {
            return "Phone is required.";
        }

        return null;
    }

    private static CustomerResponse ToResponse(Customer customer) =>
        new()
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone,
            CreatedAt = customer.CreatedAt
        };
}
