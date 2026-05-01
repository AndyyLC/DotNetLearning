namespace CustomerBatchImporter;

public class CsvImporter
{
    private readonly ICustomerRepository _customerRepo;

    public CsvImporter(ICustomerRepository customerRepo) => _customerRepo = customerRepo;
    //gets repository into constructor

    public async Task ReadAsync(Stream stream)
    {
        var reader = new StreamReader(stream); //streamreader to read lines
        string? line;
        while ((line = await reader.ReadLineAsync()) != null) //reads while lines are not null
        {
            var customer = ReadCsvLine(line); //null if line is invalid
            if (customer == null) //if line is invalid then skip rest of while loop
            {
                continue;
            }
            var existing = await _customerRepo.GetByEmailAsync(customer.Email); //check if email exists
            if (existing == null) //if email was not found, create new customer
            {
                await _customerRepo.CreateAsync(customer);
            }
            else
            {
                await _customerRepo.UpdateAysnc(new UpdateCustomerDto(existing.Id, customer.Name, customer.License));
            }
        }
    }

    private NewCustomerDto? ReadCsvLine(string line)
    {
        var el = line.Split(',');
        return el.Length != 3 ? null : new NewCustomerDto(el[0], el[1], el[2]);
    }
 
}