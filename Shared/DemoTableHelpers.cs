using Npgsql;

namespace Upserts;

public static class DemoTableHelpers
{
	public static async Task ResetDemoTableAsync( NpgsqlConnection conn )
	{
		//
		// for demo purposes, drop and recreate the table.
		//
		var sql = """
			DROP TABLE IF EXISTS user_preferences;
			CREATE TABLE user_preferences (
				id INT PRIMARY KEY,
				name TEXT NOT NULL,
				food TEXT NOT NULL
			);
			""";
		await using var cmd = new NpgsqlCommand( sql, conn );
		await cmd.ExecuteNonQueryAsync();
	}

	public static async Task WritePreferencesToConsoleAsync( NpgsqlConnection conn )
	{
		var sql = "SELECT id, name, food FROM user_preferences;";
		await using var cmd = new NpgsqlCommand( sql, conn );
		await using var reader = await cmd.ExecuteReaderAsync();
		Console.WriteLine( "User Food Preferences:" );
		while ( await reader.ReadAsync() )
			Console.WriteLine( $"{reader.GetInt32( 0 )} | " +
								$"{reader.GetString( 1 )} | " +
								$"{reader.GetString( 2 )}" );
	}
}
