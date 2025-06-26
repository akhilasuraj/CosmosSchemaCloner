using Microsoft.Azure.Cosmos;
using CosmosSchemaCloner.Services;

namespace CosmosSchemaCloner;

internal abstract class Program
{
    private static async Task Main()
    {
        Console.WriteLine("This tool clones the structure of an Azure Cosmos DB database to a local emulator.\n");
        
        var azureDbConnectionString = ConfigurationService.GetConnectionString("azureDbConnectionString");
        var emulatorDbConnectionString = ConfigurationService.GetConnectionString("emulatorDbConnectionString");

        try
        {
            var cosmosService = new CosmosService(azureDbConnectionString, emulatorDbConnectionString);
            await cosmosService.CloneDbStructureToEmulator();
            
            Console.WriteLine("\n✅ Structure cloned to emulator successfully.");
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

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }


    private static void HandleError(string message)
    {
        Console.WriteLine($"❌ {message}");
        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}