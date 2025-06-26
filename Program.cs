using Microsoft.Azure.Cosmos;

namespace CosmosSchemaCloner;

internal abstract class Program
{
    private static async Task Main()
    {
        Console.WriteLine("This tool clones the structure of an Azure Cosmos DB database to a local emulator.\n");
        
        var azureDbConnectionString = GetConnectionString("azureDbConnectionString");
        var emulatorDbConnectionString = GetConnectionString("emulatorDbConnectionString");

        try
        {
            var azureClient = new CosmosClient(azureDbConnectionString);
            var emulatorClient = new CosmosClient(emulatorDbConnectionString);

            var dbResults = await azureClient.GetDatabaseQueryIterator<DatabaseProperties>().ReadNextAsync();

            foreach (var dbProps in dbResults)
            {
                Console.WriteLine($"Creating database: {dbProps.Id}");
                var targetDb = await emulatorClient.CreateDatabaseIfNotExistsAsync(dbProps.Id);
                var containerResults = await azureClient.GetDatabase(dbProps.Id).GetContainerQueryIterator<ContainerProperties>().ReadNextAsync();

                foreach (var containerProps in containerResults)
                {
                    var cleanPolicy = new IndexingPolicy
                    {
                        IndexingMode = containerProps.IndexingPolicy.IndexingMode,
                        Automatic = containerProps.IndexingPolicy.Automatic
                    };

                    foreach (var path in containerProps.IndexingPolicy.IncludedPaths)
                    {
                        cleanPolicy.IncludedPaths.Add(new IncludedPath { Path = path.Path });
                    }

                    foreach (var path in containerProps.IndexingPolicy.ExcludedPaths)
                    {
                        cleanPolicy.ExcludedPaths.Add(new ExcludedPath { Path = path.Path });
                    }

                    var newContainerProps = new ContainerProperties(containerProps.Id, containerProps.PartitionKeyPath)
                    {
                        IndexingPolicy = cleanPolicy,
                        DefaultTimeToLive = containerProps.DefaultTimeToLive
                    };

                    Console.WriteLine($"  Creating container: {containerProps.Id}");
                    await targetDb.Database.CreateContainerIfNotExistsAsync(newContainerProps);
                }
            }
        }
        catch (CosmosException ex)
        {
            HandleError($"Cosmos DB error: {ex.Message}");
            return;
        }
        catch (Exception ex)
        {
            HandleError($"General error: {ex.Message}");
            return;
        }

        Console.WriteLine("\n✅ Structure cloned to emulator successfully.");
        Console.ReadLine();
    }

    private static string GetConnectionString(string key)
    {
        const string configFilePath = "config.txt";

        // If config file doesn't exist, create it with default values
        if (!File.Exists(configFilePath))
        {
            Console.WriteLine("Config file not found. Creating config.txt with default values.");
            File.WriteAllLines(configFilePath, [
                "azureDbConnectionString=",
                "emulatorDbConnectionString="
            ]);
        }

        var lines = File.ReadLines(configFilePath);
        var line = lines.FirstOrDefault(l => l.StartsWith($"{key}="));
        if (!string.IsNullOrWhiteSpace(line))
        {
            return line[$"{key}=".Length..].Trim();
        }

        Console.WriteLine($"{key} not found in config file. \n Please update the config.txt file with Connection string value.");
        return string.Empty;
    }

    private static void HandleError(string message)
    {
        Console.WriteLine($"❌ {message}");
        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}