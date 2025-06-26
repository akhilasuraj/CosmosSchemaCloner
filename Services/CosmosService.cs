using Microsoft.Azure.Cosmos;

namespace CosmosSchemaCloner.Services;

public class CosmosService(string azureDbConnectionString, string emulatorDbConnectionString)
{
    public async Task CloneDbStructureToEmulator()
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
}
