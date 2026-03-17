using Npgsql;

namespace Upserts;

internal class UserFoodPreference
{
	#region Properties
	public required int Id { get; set; }
	public required string Name { get; set; }
	public required string Food { get; set; }
	#endregion
}

internal class Program
{
	private static readonly UserFoodPreference[] preferences =
	{
		new() { Id = 1, Name = "Alice", Food = "apples" },
		new() { Id = 2, Name = "Bob", Food = "cherries" },
		new() { Id = 3, Name = "Charles", Food = "steak" }
	};


	private static async Task UpsertPreferenceAsync(
		UserFoodPreference preference,
		NpgsqlConnection conn )
	{
		//
		// the command below demonstrates the use of PostgreSQL
		// ON CONFLICT.
		//
		// If a row with the same primary key already exists,
		// ON CONFLICT (id) DO UPDATE updates that row instead
		// of raising a primary key violation.
		//
		var sql = @"INSERT INTO user_preferences (id, name, food) 
						VALUES (@id, @name, @food)
						ON CONFLICT( id )
						DO UPDATE SET 
						    name = EXCLUDED.name, 
						    food = EXCLUDED.food;";

		Console.WriteLine( $"Upserting {preference.Id} {preference.Name} {preference.Food}" );
		await using var cmd = new NpgsqlCommand( sql, conn );
		cmd.Parameters.AddWithValue( "id", preference.Id );
		cmd.Parameters.AddWithValue( "name", preference.Name );
		cmd.Parameters.AddWithValue( "food", preference.Food );
		await cmd.ExecuteNonQueryAsync();
	}
	private static async Task Main()
	{
		var connectionString =
			Environment.GetEnvironmentVariable( "PG_CONNECTION_STRING" )
			?? throw new InvalidOperationException(
				"Environment variable PG_CONNECTION_STRING was not set." );

		await using var conn = new NpgsqlConnection( connectionString );
		await conn.OpenAsync();
		await ResetDemoTableAsync( conn );

		//
		// insert the initial values - no conflicts
		//
		await UpsertPreferencesAsync( preferences, conn );

		//
		// change the favorite foods of Bob and Alice and update the database.
		// The database uses the Id to find the food preference to update.
		//
		preferences[0].Food = "eggs";
		preferences[1].Food = "bananas";
		await UpsertPreferencesAsync( preferences, conn );

		//
		// in this demo, name changes are allowed. Later we will add
		// a UNIQUE constraint that includes "name". That will disallow
		// name changes.
		//
		preferences[2].Name = "Harold";
		await UpsertPreferencesAsync( preferences, conn );

		//
		// The values written to the console should be:
		// User Food Preferences:
		// 1 | Alice | eggs
		// 2 | Bob | bananas
		// 3 | Harold | steak
		//
		await WritePreferencesToConsoleAsync( conn );

		await TearDownAsync( conn );
	}
	private static async Task UpsertPreferencesAsync(
		UserFoodPreference[] userPreferences,
		NpgsqlConnection conn )
	{
		//
		// for demo clarity, upsert one row at a time. Later
		// we'll add a bulk upsert.
		//
		foreach ( var preference in userPreferences )
			await UpsertPreferenceAsync( preference, conn );
	}
	private static async Task WritePreferencesToConsoleAsync( NpgsqlConnection conn )
	{
		var sql = @"SELECT * FROM user_preferences;";
		await using var cmd = new NpgsqlCommand( sql, conn );
		await using var reader = await cmd.ExecuteReaderAsync();
		Console.WriteLine( "User Food Preferences:" );
		while ( await reader.ReadAsync() )
			Console.WriteLine( $"{reader.GetInt32( 0 )} | " +
								$"{reader.GetString( 1 )} | " +
								$"{reader.GetString( 2 )}" );
	}
	private static async Task ResetDemoTableAsync( NpgsqlConnection conn )
	{
		//
		// for demo purposes, drop and recreate the table.
		//
		var sql = @"
			DROP TABLE IF EXISTS user_preferences;
            CREATE TABLE user_preferences (
                id INT PRIMARY KEY,
                name TEXT NOT NULL,
                food TEXT NOT NULL
             );";
		await using var cmd = new NpgsqlCommand( sql, conn );
		await cmd.ExecuteNonQueryAsync();
	}
	private static async Task TearDownAsync( NpgsqlConnection conn )
	{
		var sql = @"DROP TABLE IF EXISTS user_preferences;";
		await using var cmd = new NpgsqlCommand( sql, conn );
		await cmd.ExecuteNonQueryAsync();
	}

}
