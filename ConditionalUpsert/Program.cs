using Npgsql;
using Upserts;

//
// This version of the program performs bulk upserts,
// but also makes the update conditional on the new
// data having the same Name as the existing record.
//
// If an incoming record has the same primary key but
// a different Name, the conflicting row isn't updated.
//

UserFoodPreference[] preferences =
[
	new() { Id = 1, Name = "Alice", Food = "apples" },
	new() { Id = 2, Name = "Bob", Food = "cherries" },
	new() { Id = 3, Name = "Charles", Food = "steak" }
];

var connectionString =
	Environment.GetEnvironmentVariable( "PG_CONNECTION_STRING" )
	?? throw new InvalidOperationException(
		"Environment variable PG_CONNECTION_STRING was not set." );

await using var conn = new NpgsqlConnection( connectionString );
await conn.OpenAsync();
await DemoTableHelpers.ResetDemoTableAsync( conn );

//
// insert the initial values
//
await BulkConditionalUpsertPreferencesAsync( preferences, conn );

await DemoTableHelpers.WriteAllPreferencesToConsoleAsync(
	"Original Food Preferences:", conn );

//
// change the favorite foods of Bob and Alice and update the database.
// The database uses the Id to find the food preference to update.
//
preferences[0].Food = "eggs";
preferences[1].Food = "bananas";
await BulkConditionalUpsertPreferencesAsync( preferences, conn );

await DemoTableHelpers.WriteAllPreferencesToConsoleAsync(
	"New Foods for Alice and Bob:", conn );

//
// Name changes are disallowed in this version. If the
// incoming name is different, the record simply isn't updated.
//
preferences[2].Name = "Harold";
preferences[2].Food = "ravioli";
await BulkConditionalUpsertPreferencesAsync( preferences, conn );

await DemoTableHelpers.WriteAllPreferencesToConsoleAsync(
	"Tried to change the food and the name for Charles (update skipped):", conn );

//
// The values written to the console should be:
// User Food Preferences:
// 1 | Alice | eggs
// 2 | Bob | bananas
// 3 | Charles | steak
//
await DemoTableHelpers.WriteAllPreferencesToConsoleAsync( "Final Values:", conn );

static async Task BulkConditionalUpsertPreferencesAsync(
	UserFoodPreference[] preferences,
	NpgsqlConnection conn )
{
	if ( preferences.Length == 0 ) return;

	//
	// This command performs a bulk upsert using PostgreSQL
	// ON CONFLICT executed through Npgsql.
	//
	// The difference between this and the "BulkUpsert" version
	// is that this command disallows name changes. If there is
	// a primary key conflict, the row update happens if the
	// incoming name is the same as the existing name. Otherwise,
	// the update is skipped.
	//
	// The SQL parameters should be arrays of values.
	//
	// Each array is projected from the same source array
	// in the same order so the values remain aligned by index.
	//
	var ids = preferences.Select( p => p.Id ).ToArray();
	var names = preferences.Select( p => p.Name ).ToArray();
	var foods = preferences.Select( p => p.Food ).ToArray();

	var sql = """
			INSERT INTO user_preferences (id, name, food)
			SELECT *
			FROM unnest(@ids, @names, @foods)
			ON CONFLICT (id)
			DO UPDATE SET
			    food = EXCLUDED.food
			WHERE user_preferences.name = EXCLUDED.name;
			""";

	await using var cmd = new NpgsqlCommand( sql, conn );

	cmd.Parameters.AddWithValue( "ids", ids );
	cmd.Parameters.AddWithValue( "names", names );
	cmd.Parameters.AddWithValue( "foods", foods );

	await cmd.ExecuteNonQueryAsync();
}
