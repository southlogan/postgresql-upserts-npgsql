using Npgsql;
using Upserts;

//
// Instead of upserting each record one-by-one, this version
// performs a bulk upsert, reducing the number of round-trips
// made to the database.
//

UserFoodPreference[] preferences =
[
	new() { Id = 1, Name = "Alice", Food = "apples" },
	new() { Id = 2, Name = "Bob", Food = "cherries" },
	new() { Id = 3, Name = "Charles", Food = "steak" }
];

var connectionString =
	Environment.GetEnvironmentVariable( "UPSERTS_CONN_STRING" )
	?? throw new InvalidOperationException(
		"Environment variable UPSERTS_CONN_STRING was not set." );

await using var conn = new NpgsqlConnection( connectionString );
await conn.OpenAsync();
await DemoTableHelpers.ResetDemoTableAsync( conn );

//
// insert the initial values
//
await BulkUpsertPreferencesAsync( preferences, conn );

await DemoTableHelpers.WriteAllPreferencesToConsoleAsync(
	"Original Food Preferences:", conn );

//
// change the favorite foods of Bob and Alice and update the database.
// The database uses the Id to find the food preference to update.
//
preferences[0].Food = "eggs";
preferences[1].Food = "bananas";
await BulkUpsertPreferencesAsync( preferences, conn );

await DemoTableHelpers.WriteAllPreferencesToConsoleAsync(
	"New Foods for Alice and Bob:", conn );

//
// in this basic demo, name changes are allowed. Later
// we will disallow name changes.
//
preferences[2].Name = "Harold";
preferences[2].Food = "ravioli";
await BulkUpsertPreferencesAsync( preferences, conn );

await DemoTableHelpers.WriteAllPreferencesToConsoleAsync(
	"Changed the food and the name for Charles:", conn );

//
// The values written to the console should be:
// User Food Preferences:
// 1 | Alice | eggs
// 2 | Bob | bananas
// 3 | Harold | ravioli
//
await DemoTableHelpers.WriteAllPreferencesToConsoleAsync( "Final Values:", conn );

static async Task BulkUpsertPreferencesAsync(
	UserFoodPreference[] preferences,
	NpgsqlConnection conn )
{
	if ( preferences.Length == 0 ) return;

	//
	// This command performs a bulk upsert using PostgreSQL
	// ON CONFLICT executed through Npgsql.
	//
	// The SQL parameters should be arrays of values, like:
	// ids[] = { 1, 2, 3}
	// names[] = { "Alice", "Bob", "Charles" }
	// foods[] = { "apples", "cherries", "steak" }
	//
	// Each array is projected from the same source array
	// in the same order so the values remain aligned by index.
	//
	// If a row with the same primary key already exists,
	// the ON CONFLICT (id) handles it and updates the
	// row instead of raising a primary key violation.
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
			    name = EXCLUDED.name,
			    food = EXCLUDED.food;
			""";

	await using var cmd = new NpgsqlCommand( sql, conn );

	cmd.Parameters.AddWithValue( "ids", ids );
	cmd.Parameters.AddWithValue( "names", names );
	cmd.Parameters.AddWithValue( "foods", foods );

	await cmd.ExecuteNonQueryAsync();
}
