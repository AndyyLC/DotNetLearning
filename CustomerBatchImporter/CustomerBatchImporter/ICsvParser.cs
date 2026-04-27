namespace CustomerBatchImporter;

public interface ICsvParser<NewCustomerDto>
{
    NewCustomerDto? ParseLine(string line);
}