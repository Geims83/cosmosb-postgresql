
// Replace <cluster> with your cluster name and <password> with your password:
using Bogus;
using DbSeeder;
using Npgsql;
using System.Text;

string server = Environment.GetEnvironmentVariable("CITUS_SERVER");
string password = Environment.GetEnvironmentVariable("CITUS_PASSWORD");

var connStr = new NpgsqlConnectionStringBuilder($"Server = {server}.postgres.cosmos.azure.com; Database = citus; Port = 5432; User Id = citus; Password = {password}; Ssl Mode = Require; Pooling = true; Minimum Pool Size=0; Maximum Pool Size =50");

connStr.TrustServerCertificate = true;

using (var conn = new NpgsqlConnection(connStr.ToString()))
{
    Console.Out.WriteLine("Opening connection");
    await conn.OpenAsync();

    using (var command = new NpgsqlCommand(Strings.DbCreation, conn))
    {
        Console.Out.WriteLine("Creating tables");
        await command.ExecuteNonQueryAsync();
    }
    await conn.CloseAsync();

    Console.WriteLine(DateTime.Now);
    await Task.WhenAll(
        CreateUsers(25000),
        CreateUsers(25000),
        CreateUsers(25000),
        CreateUsers(25000),
        CreateUsers(25000),
        CreateUsers(25000)
        );
    Console.WriteLine(DateTime.Now);
}
Console.WriteLine("Press RETURN to exit");
Console.ReadLine();


async Task CreateUsers(int numUsers)
{
    await using var conn = new NpgsqlConnection(connStr.ToString());
    await conn.OpenAsync();
    
    var userFaker = new Faker<User>("it")
            .RuleFor(u => u.UserId, f => f.IndexGlobal)
            .RuleFor(u => u.Name, f => f.Person.FullName)
            .RuleFor(u => u.Identifier, f => f.Person.UserName);
    
    for (int i = 0; i < numUsers; i++)
    {
        var currUser = userFaker.Generate();

        using (var command = new NpgsqlCommand("INSERT INTO  vu_users  (user_id, name, user_identifier) VALUES (@id, @name, @identifier)", conn))
        {
            command.Parameters.AddWithValue("id", currUser.UserId);
            command.Parameters.AddWithValue("name", currUser.Name);
            command.Parameters.AddWithValue("identifier", currUser.Identifier);
            await command.ExecuteNonQueryAsync();
        }

        await Task.WhenAll (GenerateEvents(currUser.UserId), GenerateSpeeches(currUser.UserId));

        Console.WriteLine($"User {currUser.UserId} created");
    }
    await conn.CloseAsync();

    Console.WriteLine($"{numUsers} users created");
}

async Task GenerateEvents(int userId)
{
    await using var conn = new NpgsqlConnection(connStr.ToString());

    for (int anno = 2010; anno < 2024; anno++)
    {
        var eventFaker = new Faker<Event>()
            .RuleFor(e => e.EventId, f => f.IndexGlobal)
            .RuleFor(e => e.Type, f => f.Random.Int(1, 5))
            .RuleFor(e => e.Date, f => f.Date.Between(new DateTime(anno, 1, 1), new DateTime(anno, 12, 31)));

        var list = eventFaker.Generate(50);

        StringBuilder sql = new StringBuilder("INSERT INTO vu_events (event_id, user_id, event_type_id, event_date) VALUES ");
        foreach (var e in list)
        {
            sql.AppendLine($"({e.EventId}, {userId}, {e.Type}, '{e.Date.ToString("yyyy-MM-dd")}'),");
        }
        sql.Remove(sql.ToString().LastIndexOf(','), 1);

        await conn.OpenAsync();
        using (var command = new NpgsqlCommand(sql.ToString(), conn))
        {
            await command.ExecuteNonQueryAsync();
        }
        await conn.CloseAsync();
    }
}

async Task GenerateSpeeches(int userId)
{
    await using var conn = new NpgsqlConnection(connStr.ToString());

    for (int anno = 2010; anno < 2024; anno++)
    {
        var speechFaker = new Faker<Speech>("it")
            .RuleFor(s => s.SpeechId, f => f.IndexGlobal)
            .RuleFor(s => s.Date, f => f.Date.Between(new DateTime(anno, 1, 1), new DateTime(anno, 12, 31)))
            .RuleFor(s => s.Text, f => f.Lorem.Paragraphs());

        var list = speechFaker.Generate(2);

        StringBuilder sql = new StringBuilder("INSERT INTO vu_speeches (speech_id, user_id, speech_date, speech_text) VALUES ");
        foreach (var speech in list)
        {
            sql.AppendLine($"({speech.SpeechId}, {userId}, '{speech.Date.ToString("yyyy-MM-dd")}', '{speech.Text}'),");
        }
        sql.Remove(sql.ToString().LastIndexOf(','), 1);

        await conn.OpenAsync();
        using (var command = new NpgsqlCommand(sql.ToString(), conn))
        {
            await command.ExecuteNonQueryAsync();
        }
        await conn.CloseAsync();
    }   
}