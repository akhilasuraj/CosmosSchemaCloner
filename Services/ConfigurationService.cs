namespace CosmosSchemaCloner.Services;

/// <summary>
/// Static service that manages application configuration.
/// Handles reading connection strings from a configuration file and creates
/// the file with default values if it doesn't exist.
/// </summary>
public static class ConfigurationService
{
    /// <summary>
    /// Path to the configuration file relative to the application's working directory.
    /// </summary>
    private const string ConfigFilePath = "config.txt";

    /// <summary>
    /// Retrieves a connection string from the configuration file by key.
    /// If the configuration file doesn't exist, it creates one with default placeholder values.
    /// </summary>
    /// <param name="key">The key of the connection string to retrieve (e.g., "azureDbConnectionString")</param>
    /// <returns>The connection string value if found; otherwise, an empty string</returns>
    public static string GetConnectionString(string key)
    {
        // If config file doesn't exist, create it with default values
        if (!File.Exists(ConfigFilePath))
        {
            Console.WriteLine("Config file not found. Creating config.txt with default values.");
            File.WriteAllLines(ConfigFilePath, [
                "azureDbConnectionString=YOUR_AZURE_COSMOS_DB_CONNECTION_STRING",
                "emulatorDbConnectionString=YOUR_EMULATOR_CONNECTION_STRING"
            ]);
        }

        // Read all lines from the config file
        var lines = File.ReadLines(ConfigFilePath);
        
        // Find the line that starts with the specified key
        var line = lines.FirstOrDefault(l => l.StartsWith($"{key}="));
        
        if (!string.IsNullOrWhiteSpace(line))
        {
            // Extract the value after the key= part
            return line[$"{key}=".Length..].Trim();
        }

        // If key not found, notify the user and return empty string
        Console.WriteLine($"{key} not found in config file. \nPlease update the config.txt file with Connection string value.");
        return string.Empty;
    }
}
