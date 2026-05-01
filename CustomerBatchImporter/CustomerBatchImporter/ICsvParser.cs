namespace CustomerBatchImporter;

public interface ICsvParser<NewCustomerDto>
{
    NewCustomerDto? ParseLine(Stream line);
}