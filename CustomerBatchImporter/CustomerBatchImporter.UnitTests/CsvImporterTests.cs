using System.Text;
using CustomerBatchImporter;
using FakeItEasy;
using Microsoft.VisualBasic;
using Xunit;

namespace CustomerBatchImporter.UnitTests;

public class CsvImporterTests
{
    private readonly ICustomerRepository _fakeCustomerRepo; //each test can access a fake
    private readonly CsvImporter _csvImporter; //each test can access CsvImporter

    public CsvImporterTests() //each test is an instanct of this class
    {
        //because every test will perform some operation on CsvImporter, the object can be created in the constructor
        //the fake object for ICustomerReposityory is also created here as a field
        _fakeCustomerRepo = A.Fake<ICustomerRepository>();
        _csvImporter = new(_fakeCustomerRepo);
    }

    private Stream GetStreamFromString(String content) => //helper method
        new MemoryStream(Encoding.UTF8.GetBytes(content)); //converts string to bytes and then wraps in memorystream

    [Fact]
    public async Task OneCustomer()
    {
        string email = "some@email.com";
        string name = "A Customer";
        string license = "Basic";
        string csv = string.Join(',', email, name, license);

        var stream = GetStreamFromString(csv); //use method that turns String into Stream
        await _csvImporter.ReadAsync(stream);
    }

    [Fact]
    public async Task ValidCustomerOneLine()
    {
        //Arrange Step
        string email = "some@email.com";
        string name = "A Customer";
        string license = "Basic";
        string csv = string.Join(',', email, name, license);
        A.CallTo(() => _fakeCustomerRepo.GetByEmailAsync(email)).Returns(default(Customer)); //Sets up a fake for GetEmailAsync and returns null
        //The value of default for a reference type is null while the default of a value type cannot be null and is 0 instead
        //Cannot use .ReturnsN(null) as the compiler can't tell what is the type due to overloading this method
        //Since this method can be overloaded as either Customer or Task<Customer>, the method cannot tell if the return is null or Task is null
        //Returns null so that CsvImporter creates a new customer, because null value tells
        //the importer that no customer with that email exists
        //By returning default, it helps the compiler deteremine the type is a Task
        //also can be done with .Returns(Task.FromResult<Customer?>(null))

        //Act Step
        var stream = GetStreamFromString(csv);
        await _csvImporter.ReadAsync(stream);

        //Assert Step
        A.CallTo(() => _fakeCustomerRepo.GetByEmailAsync(email)).MustHaveHappened();

        A.CallTo(() =>
                _fakeCustomerRepo.CreateAsync(
                    A<NewCustomerDto>.That.Matches(n =>
                        n.Email == email && n.Name == name && n.License == license
                    )
                )
            )
            .MustHaveHappened();
        //checks if new customer is correct and if csvimporter has called this method
    }

    [Fact]
    public async Task InvalidLine()
    {
        var stream = GetStreamFromString("Line that should fail due to no commas");
        await _csvImporter.ReadAsync(stream);

        var calls = Fake.GetCalls(_fakeCustomerRepo); //get all calls to fake
        Assert.Empty(calls); //Assert that there were no calls
    }

    [Fact]
    public async Task ThreeLineOneInvalid()
    {
        //Arrange
        A.CallTo(() => _fakeCustomerRepo.GetByEmailAsync(A<string>.Ignored)).Returns(default(Customer));
        //A<string>.Ignored makes it not care about email

        //Act
        var stream = GetStreamFromString("a@b.com,customer1,None\ninvalidline\nc@d.com,customer2,None");
        await _csvImporter.ReadAsync(stream);

        //Assert
        A.CallTo(() => _fakeCustomerRepo.CreateAsync(A<NewCustomerDto>.Ignored))
            .MustHaveHappenedTwiceExactly();
    }   //A<NewCustomerDto>.Ignored makes it so it doesn't care about input
}
