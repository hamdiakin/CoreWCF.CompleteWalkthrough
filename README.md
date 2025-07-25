# Echo Walkthrough

This solution demonstrates a simple request-response echo service using ZeroMQ in .NET 8.0. It is a modern, clean, and CoreWCF-free implementation, focusing on clarity and maintainability.

## Solution Structure

- **Common/**: Shared contracts and logic (interfaces, messages, service implementation)
- **NetCoreServer/**: ZeroMQ server that processes echo requests
- **NetCoreClient/**: ZeroMQ client that sends echo requests and receives responses

## Key Technologies
- [.NET 8.0](https://dotnet.microsoft.com/)
- [ZeroMQ (NetMQ)](https://netmq.readthedocs.io/en/latest/)

## How It Works
- The client sends requests (simple echo, complex echo, fail, permission test) to the server using ZeroMQ sockets.
- The server processes requests using the `EchoService` and returns responses.
- All communication is via JSON-serialized messages.

## Build & Run

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)

### Build
```
dotnet build ZeroMQ.EchoWalkthrough.sln
```

### Run the Server
```
dotnet run --project NetCoreServer
```

### Run the Client (in a separate terminal)
```
dotnet run --project NetCoreClient
```

## Removing CoreWCF
- All CoreWCF and WCF-related dependencies and code have been removed.
- The solution is now focused solely on ZeroMQ-based messaging.

## Extending
- Add new request types by extending `EchoService` and updating the client accordingly.
- Use the shared `Common` project for new message types and contracts.

## License
MIT 