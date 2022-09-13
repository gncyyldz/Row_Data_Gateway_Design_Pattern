using System.Data.SqlClient;

PersonGateway eklenecekPerson = new();
eklenecekPerson.Name = "Nevin";
eklenecekPerson.Surname = "Yılmaz";
await eklenecekPerson.AddPerson();
await eklenecekPerson.AddPerson("Gençay", "Yıldız");

PersonGateway guncellenecekPerson = new();
guncellenecekPerson.Id = 101;
guncellenecekPerson.Name = "Nevin";
guncellenecekPerson.Surname = "Yıldız";
await guncellenecekPerson.UpdateCustomer();
await guncellenecekPerson.UpdateCustomer(102, "Kürşad", "Yıldız");

PersonGateway silinecekPerson = new();
silinecekPerson.Id = 3;
await silinecekPerson.RemoveCustomer();
await silinecekPerson.RemoveCustomer(4);

PersonGateway arananPerson = new() { Id = 10 };
PersonFinder finder = new(arananPerson);
var person1 = await finder.GetPersonById();
var person2 = await finder.GetPersonById(10);

Console.WriteLine();

public class PersonGateway
{
    public PersonGateway() { }
    public PersonGateway(int id, string name, string surname) { Id = id; Name = name; Surname = surname; }

    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }

    public async Task AddPerson()
        => await AddPerson(Name, Surname);

    public async Task AddPerson(string name, string surname)
        => await Database.ExecuteNonQueryAsync("INSERT Persons(Name, Surname) VALUES(@name, @surname)"
            , new SqlParameter("name", name)
            , new SqlParameter("surname", surname));

    public async Task RemoveCustomer()
        => await RemoveCustomer(Id);

    public async Task RemoveCustomer(int id)
        => await Database.ExecuteNonQueryAsync("DELETE FROM Persons WHERE Id = @id"
            , new SqlParameter("id", id));

    public async Task UpdateCustomer()
        => await UpdateCustomer(Id, Name, Surname);

    public async Task UpdateCustomer(int id, string name, string surname)
        => await Database.ExecuteNonQueryAsync("UPDATE Persons SET Name = @name, Surname = @surname WHERE Id = @id"
            , new SqlParameter("id", id)
            , new SqlParameter("name", name)
            , new SqlParameter("surname", surname));
}
public class PersonFinder
{
    public PersonFinder() { }
    readonly PersonGateway _personGateway;
    public PersonFinder(PersonGateway personGateway)
        => _personGateway = personGateway;

    public async Task<PersonGateway> GetPersonById()
        => await GetPersonById(_personGateway.Id);

    public async Task<PersonGateway> GetPersonById(int id)
    {
        SqlDataReader dataReader = await Database.ExecuteReaderAsync("SELECT * FROM Persons WHERE Id = @id"
                    , new SqlParameter("id", id));
        await dataReader.ReadAsync();
        PersonGateway person = new(int.Parse(dataReader["id"].ToString()), dataReader["name"].ToString(), dataReader["surname"].ToString());

        await dataReader.CloseAsync();
        await dataReader.DisposeAsync();
        return person;
    }

    public async Task<List<PersonGateway>> GetPersonByName()
        => await GetPersonByName(_personGateway.Name);

    public async Task<List<PersonGateway>> GetPersonByName(string name)
    {
        SqlDataReader dataReader = await Database.ExecuteReaderAsync("SELECT * FROM Persons WHERE Name = @name"
                  , new SqlParameter("name", name));
        List<PersonGateway> persons = new();
        while (await dataReader.ReadAsync())
            persons.Add(new(int.Parse(dataReader["id"].ToString()), dataReader["name"].ToString(), dataReader["surname"].ToString()));

        await dataReader.CloseAsync();
        await dataReader.DisposeAsync();
        return persons;
    }

    public async Task<List<PersonGateway>> GetPersonBySurname()
        => await GetPersonBySurname(_personGateway.Surname);

    public async Task<List<PersonGateway>> GetPersonBySurname(string surname)
    {
        SqlDataReader dataReader = await Database.ExecuteReaderAsync("SELECT * FROM Persons WHERE Surname = @surname"
                  , new SqlParameter("surname", surname));
        List<PersonGateway> persons = new();
        while (await dataReader.ReadAsync())
            persons.Add(new(int.Parse(dataReader["id"].ToString()), dataReader["name"].ToString(), dataReader["surname"].ToString()));

        await dataReader.CloseAsync();
        await dataReader.DisposeAsync();
        return persons;
    }

    public async Task<List<PersonGateway>> GetAllPersons()
    {
        SqlDataReader dataReader = await Database.ExecuteReaderAsync("SELECT * FROM Persons");
        List<PersonGateway> persons = new();
        while (await dataReader.ReadAsync())
            persons.Add(new(int.Parse(dataReader["id"].ToString()), dataReader["name"].ToString(), dataReader["surname"].ToString()));

        await dataReader.CloseAsync();
        await dataReader.DisposeAsync();
        return persons;
    }
}
public static class Database
{
    static SqlConnection _connection;
    static Database()
    {
        object _lock = new();
        lock (_lock)
            _connection = new("Server=localhost, 1433;Database=RowDataGatewayDB;User Id=sa;Password=1q2w3e4r+!");
        _connection.Open();
    }

    public static async Task<int> ExecuteNonQueryAsync(string query, params SqlParameter[] parameters)
    {
        SqlCommand command = new(query, _connection);
        command.Parameters.AddRange(parameters);
        return await command.ExecuteNonQueryAsync();
    }

    public static async Task<SqlDataReader> ExecuteReaderAsync(string query, params SqlParameter[] parameters)
    {
        SqlCommand command = new(query, _connection);
        command.Parameters.AddRange(parameters);
        return await command.ExecuteReaderAsync();
    }
}