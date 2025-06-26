using Microsoft.Azure.Cosmos;
using CosmosSchemaCloner.Services;

namespace CosmosSchemaCloner;

/// <summary>
/// Main program class for the Cosmos DB Schema Cloner application.
/// This tool clones the structure (databases and containers) from an Azure Cosmos DB 
/// to a local Cosmos DB Emulator without copying the data.
/// </summary>
internal abstract class Program
{
    /// <summary>
    /// Entry point of the application.
    /// Retrieves connection strings, initializes the CosmosService, and handles any exceptions during cloning.
    /// </summary>
    private static async Task Main()
    {
        Console.WriteLine("This tool clones the structure of an Azure Cosmos DB database to a local emulator.\n");
        
        // Get connection strings from the configuration file
        var azureDbConnectionString = ConfigurationService.GetConnectionString("azureDbConnectionString");
        var emulatorDbConnectionString = ConfigurationService.GetConnectionString("emulatorDbConnectionString");

        try
        {
            // Initialize the CosmosService with the connection strings
            var cosmosService = new CosmosService(azureDbConnectionString, emulatorDbConnectionString);
            
            // Execute the cloning process
            await cosmosService.CloneDbStructureToEmulator();
            
            Console.WriteLine("\n✅ Structure cloned to emulator successfully.");
        }
        catch (CosmosException ex)
        {
            // Handle specific Cosmos DB errors (authentication, connectivity, etc.)
            HandleError($"Cosmos DB error: {ex.Message}");
            return;
        }
        catch (Exception ex)
        {
            // Handle any other unexpected errors
            HandleError($"General error: {ex.Message}");
            return;
        }

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }

    /// <summary>
    /// Displays error messages to the user in a consistent format.
    /// </summary>
    /// <param name="message">The error message to display</param>
    private static void HandleError(string message)
    {
        Console.WriteLine($"❌ {message}");
        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}