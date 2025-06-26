using Microsoft.Azure.Cosmos;

namespace CosmosSchemaCloner.Services;

/// <summary>
/// Service responsible for interacting with Azure Cosmos DB and local emulator.
/// Handles the cloning of database structure (databases and containers) from an Azure Cosmos DB
/// to a local Cosmos DB emulator without copying the actual data.
/// </summary>
public class CosmosService(string azureDbConnectionString, string emulatorDbConnectionString)
{
    /// <summary>
    /// Clones the database structure from Azure Cosmos DB to the local emulator.
    /// Creates databases and containers with matching properties, including indexing policies.
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task CloneDbStructureToEmulator()
    {
        // Initialize clients for both Azure and Emulator Cosmos DB instances
        var azureClient = new CosmosClient(azureDbConnectionString);
        var emulatorClient = new CosmosClient(emulatorDbConnectionString);

        // Get all databases from Azure Cosmos DB
        var dbResults = await azureClient.GetDatabaseQueryIterator<DatabaseProperties>().ReadNextAsync();

        // Process each database
        foreach (var dbProps in dbResults)
        {
            Console.WriteLine($"Creating database: {dbProps.Id}");
            
            // Create the database in the emulator if it doesn't exist
            var targetDb = await emulatorClient.CreateDatabaseIfNotExistsAsync(dbProps.Id);
            
            // Get all containers from the current database
            var containerResults = await azureClient.GetDatabase(dbProps.Id).GetContainerQueryIterator<ContainerProperties>().ReadNextAsync();

            // Process each container within the database
            foreach (var containerProps in containerResults)
            {
                // Create a clean indexing policy to avoid serialization issues
                var cleanPolicy = new IndexingPolicy
                {
                    IndexingMode = containerProps.IndexingPolicy.IndexingMode,
                    Automatic = containerProps.IndexingPolicy.Automatic
                };

                // Copy included paths from source container
                foreach (var path in containerProps.IndexingPolicy.IncludedPaths)
                {
                    cleanPolicy.IncludedPaths.Add(new IncludedPath { Path = path.Path });
                }

                // Copy excluded paths from source container
                foreach (var path in containerProps.IndexingPolicy.ExcludedPaths)
                {
                    cleanPolicy.ExcludedPaths.Add(new ExcludedPath { Path = path.Path });
                }

                // Create new container properties with the same configuration
                var newContainerProps = new ContainerProperties(containerProps.Id, containerProps.PartitionKeyPath)
                {
                    IndexingPolicy = cleanPolicy,
                    DefaultTimeToLive = containerProps.DefaultTimeToLive
                };

                Console.WriteLine($"  Creating container: {containerProps.Id}");
                
                // Create the container in the emulator if it doesn't exist
                await targetDb.Database.CreateContainerIfNotExistsAsync(newContainerProps);
            }
        }
    }
}
