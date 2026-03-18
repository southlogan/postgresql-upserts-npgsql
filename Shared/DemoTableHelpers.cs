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
		#if false
		Console.WriteLine();
		Console.WriteLine("---- TABLE DUMP ----");

		var sql = "SELECT id, name, food FROM user_preferences ORDER BY id ASC;";
		await using var cmd = new NpgsqlCommand(sql, conn);
		await using var reader = await cmd.ExecuteReaderAsync();

		while (await reader.ReadAsync())
		{
			var id = reader.GetInt32(0);
			var name = reader.GetString(1);
			var food = reader.GetString(2);

			Console.WriteLine($"{id} | {name} | {food}");
		}

		Console.WriteLine("--------------------");
			#else
		var sql = "SELECT id, name, food FROM user_preferences ORDER BY id ASC;";
		await using var cmd = new NpgsqlCommand( sql, conn );
		await using var reader = await cmd.ExecuteReaderAsync();
		Console.WriteLine( "User Food Preferences:" );
		while ( await reader.ReadAsync() )
			Console.WriteLine( $"{reader.GetInt32( 0 )} | " +
								$"{reader.GetString( 1 )} | " +
								$"{reader.GetString( 2 )}" );
#endif
	}
}
