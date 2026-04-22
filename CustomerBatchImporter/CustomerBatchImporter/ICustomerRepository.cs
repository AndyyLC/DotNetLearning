namespace CustomerBatchImporter;

public interface ICustomerRepository
{
    Task CreateAsync(NewCustomerDto customerDto);
    //Defaults to async and Immutable Data Transfer Object(DTO)

    Task UpdateAysnc(UpdateCustomerDto customerDto);

    Task<Customer?> GetByEmailAsync(string email);
    
}

public record NewCustomerDto(
    string Email,
    string? Name,
    string? License
) {}

public record UpdateCustomerDto(
    int Id,
    string? NewName,
    string? NewLicense
) {}