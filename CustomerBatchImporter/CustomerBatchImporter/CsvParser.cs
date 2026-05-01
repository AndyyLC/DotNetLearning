// using GenericParsing;
//Abandoned because Generic Parser does not take in Stream despite the Github documentation saying it can parse from Stream 
//with the github example being
//using (GenericParser parser = new GenericParser(stream)) { }

// namespace CustomerBatchImporter;

// public class CsvParser : ICsvParser<NewCustomerDto>
// {
//     public NewCustomerDto? ParseLine(Stream line)
//     {
//         using (GenericParser parser = new GenericParser(line)) //
//         {
//             parser.ColumnDelimiter = ',';

//             string email = parser[0];
//             string name = parser[1];
//             string license = parser[2];
//             if (parser.ColumnCount != 3)
//             {
//                 return null;
//             }
//             return new NewCustomerDto(parser[0], parser[1], parser[2]);
//         }
//     }
// }
