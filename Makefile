.PHONY: all
all:
	make proto_server
	make proto_unity

.PHONY: proto_server
proto_server:
	protoc --go_out=./grpc_server ./proto/hello.proto

.PHONY: proto_unity
proto_unity:
	protoc --csharp_out=./unity/Assets/Proto ./proto/hello.proto
