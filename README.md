# Basis Theory .NET Sample App

The [Basis Theory](https://basistheory.com/) .NET Sample App

## Documentation

The sample app performs the full lifecycle of a token. Example code can be found in the [token API docs](https://docs.basistheory.com/api-reference/?csharp#tokens)

## Dependencies
- [.NET 5](https://dotnet.microsoft.com/download/dotnet/5.0)

Verify .NET is installed by running: `dotnet --list-sdks` and verify an entry with version `5.*` is displayed

## Setup

1. Login to [portal.basistheory.com](https://portal.basistheory.com)
2. Create a new `server_to_server` application with the following token permissions:
    1. `token:create`
    2. `token:read`
    3. `token:decrypt`
3. Copy and save the `API Key` for the newly created application

## Run the Sample App

```
make token
```

The application will prompt you for your `API Key` and if you want to tokenize from console input or a file.
