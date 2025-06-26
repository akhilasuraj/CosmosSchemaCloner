namespace CosmosSchemaCloner.Services;

public static class ConfigurationService
{
    private const string ConfigFilePath = "config.txt";

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

        var lines = File.ReadLines(ConfigFilePath);
        var line = lines.FirstOrDefault(l => l.StartsWith($"{key}="));
        if (!string.IsNullOrWhiteSpace(line))
        {
            return line[$"{key}=".Length..].Trim();
        }

        Console.WriteLine($"{key} not found in config file. \nPlease update the config.txt file with Connection string value.");
        return string.Empty;
    }
}
