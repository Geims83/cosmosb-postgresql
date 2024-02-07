
// Replace <cluster> with your cluster name and <password> with your password:
using Bogus;
using DbSeeder;
using Npgsql;
using System.Text;

var connStr = new NpgsqlConnectionStringBuilder("");

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
    
    var userFaker = new Faker<User>()
            .RuleFor(u => u.UserId, f => f.IndexGlobal)
            .RuleFor(u => u.Name, f => f.Name.FullName())
            .RuleFor(u => u.Identifier, f => f.Internet.Email());
    
    for (int i = 0; i < numUsers; i++)
    {
        var currUser = userFaker.Generate();

        using (var command = new NpgsqlCommand("INSERT INTO  vu_users  (user_id, name, user_identifier) VALUES (@id, @name, @identifier)", conn))
        {
            command.Parameters.AddWithValue("id", currUser.UserId);
            command.Parameters.AddWithValue("name", currUser.Name);
            command.Parameters.AddWithValue("identifier", currUser.Identifier);
            await command.ExecuteNonQueryAsync();
            //Console.Out.WriteLine(String.Format("Number of rows inserted={0}", nRows));
        }

        await GenerateEvents(currUser.UserId);
    }
    await conn.CloseAsync();

    Console.Out.WriteLine($"{numUsers} users created");
}

async Task GenerateEvents(int userId)
{
    await using var conn = new NpgsqlConnection(connStr.ToString());

    for (int anno = 2010; anno < 2024; anno++)
    {
        var eventFaker = new Faker<Event>()
            .RuleFor(e => e.EventId, f => f.IndexGlobal)
            .RuleFor(e => e.Type, f => f.PickRandom(Strings.EventType))
            .RuleFor(e => e.Date, f => f.Date.Between(new DateTime(anno, 1, 1), new DateTime(anno, 12, 31)));

        var list = eventFaker.Generate(50);

        StringBuilder sql = new StringBuilder("INSERT INTO vu_events (event_id, user_id, event_type, event_date) VALUES ");
        for (int i = 0; i < 50; i++)
        {
            sql.AppendLine($"({list[i].EventId}, {userId}, '{list[i].Type}', '{list[i].Date.ToString("yyyy-MM-dd hh:mm:ss")}'),");
        }
        sql.Remove(sql.Length - 3, 3);

        await conn.OpenAsync();
        using (var command = new NpgsqlCommand(sql.ToString(), conn))
        {
            await command.ExecuteNonQueryAsync();
        }
        await conn.CloseAsync();
    }
}