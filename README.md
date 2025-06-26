# CosmosSchemaCloner

A .NET tool to clone the structure (databases and containers, including indexing policies) of an Azure Cosmos DB account to a local Cosmos DB Emulator.

## Features
- Reads all databases and containers from a source Azure Cosmos DB account
- Recreates the same structure (databases, containers, partition keys, indexing policies, TTL) in a local Cosmos DB Emulator
- Simple configuration via `config.txt`

## Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download)
- [Azure Cosmos DB Emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/local-emulator)
- Access to an Azure Cosmos DB account (for source)

## Configuration
Before running, create a `config.txt` file in the application directory with the following content:

```
azureDbConnectionString=YOUR_AZURE_COSMOS_DB_CONNECTION_STRING
emulatorDbConnectionString=YOUR_EMULATOR_CONNECTION_STRING
```

The tool will auto-generate this file with empty values if it does not exist. Fill in the correct connection strings.

## Usage
1. Build and publish the project as a single executable:
   ```sh
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   ```
2. Run the generated executable from the `bin/Release/net9.0/win-x64/publish/` directory:
   ```sh
   CosmosSchemaCloner.exe
   ```
3. Follow the console instructions. The tool will display progress and notify you when cloning is complete.

## Notes
- Only the structure (not the data) is cloned.
- Indexing policies and TTL settings are preserved.
- Update `config.txt` if you need to change connection strings.

## License
MIT License.
