# Prerequisites

## Common (Optional)

To build protocol buffer, `protoc` is required.

- protoc

To install protoc, refer this site.

https://developers.google.com/protocol-buffers/docs/gotutorial

## Server

To build server, go is required.

- go >= 1.16

Note that prebuilt binaries are not included in this repository.

## Unity

Build Settings modifications below;

- .NET 4.x
- IL2CPP
- Android target API level >= 25 (7.1 Nougat)

gRPC package contains large files so those are not commited.

Please download gRPC package from https://packages.grpc.io/ and import `Plugins` directory manually.

# Build

## Protocol Buffer IDL

```
% make proto_server
% make proto_client
```

## Server

Then compile client and server

```
% cd grpc_server
% make server
```

## Test Client (Optional)

```
% cd grpc_server
% make client
```

## Unity

Before building, please modify local server IP address defined at `Greet` method in `unity/Assets/GrpcTest.cs`

```
Channel channel = new Channel("192.168.11.9:50051", ChannelCredentials.Insecure);
```

to

```
Channel channel = new Channel("<your IP address>:50051", ChannelCredentials.Insecure);
```

Then build iOS or Android with exporting project.

Additional modification is needed after projects are exported.

### iOS

- disable bit code for targets
- add `libz.tbd` to build phases

### Android

- add INTERNET permission to manifest


# Run

## Server

Launch gRPC server to communicate with clients include Unity app.

```
% ./bin/${GOOS}_${GOARC}/server
```

## Test Client (Optional)

gRPC communication can be tested by executing test client more instantly than Unity.

```
% ./bin/${GOOS}_${GOARC}/client
```

Host name can be specified.

```
% TEST_GRPC_HOST=xxxxxx ./bin/${GOOS}_${GOARC}/client
```
