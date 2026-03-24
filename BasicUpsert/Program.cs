using Npgsql;
using Upserts;

//
// this version upserts each row in the table, one-by-one
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
await UpsertPreferencesAsync( preferences, conn );

await DemoTableHelpers.WriteAllPreferencesToConsoleAsync(
	"Original Food Preferences:", conn );

//
// change the favorite foods of Bob and Alice and update the database.
// The database uses the Id to find the food preference to update.
//
preferences[0].Food = "eggs";
preferences[1].Food = "bananas";
await UpsertPreferencesAsync( preferences, conn );

await DemoTableHelpers.WriteAllPreferencesToConsoleAsync(
	"New Foods for Alice and Bob:", conn );

//
// in this basic demo, name changes are allowed. Later
// we will disallow name changes.
//
preferences[2].Name = "Harold";
preferences[2].Food = "ravioli";
await UpsertPreferencesAsync( preferences, conn );

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

static async Task UpsertPreferenceAsync(
	UserFoodPreference preference,
	NpgsqlConnection conn )
{
	//
	// This command demonstrates PostgreSQL ON CONFLICT
	// executed through Npgsql. For clarity,
	// this method upserts a single row of the table.
	//
	// If a row with the same primary key already exists,
	// ON CONFLICT (id) DO UPDATE updates that row instead
	// of raising a primary key violation.
	//
	var sql = """
			INSERT INTO user_preferences (id, name, food) 
			VALUES (@id, @name, @food)
			ON CONFLICT( id )
			DO UPDATE SET 
				name = EXCLUDED.name, 
				food = EXCLUDED.food;
			""";

	await using var cmd = new NpgsqlCommand( sql, conn );

	cmd.Parameters.AddWithValue( "id", preference.Id );
	cmd.Parameters.AddWithValue( "name", preference.Name );
	cmd.Parameters.AddWithValue( "food", preference.Food );

	await cmd.ExecuteNonQueryAsync();
}
static async Task UpsertPreferencesAsync(
	UserFoodPreference[] userPreferences,
	NpgsqlConnection conn )
{
	//
	// for demo clarity, upsert one row at a time. Later
	// we'll add a bulk upsert.
	//
	foreach ( var preference in userPreferences ) await UpsertPreferenceAsync( preference, conn );
}
