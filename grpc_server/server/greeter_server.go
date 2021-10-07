package server

import (
	"context"

	codes "google.golang.org/grpc/codes"
	status "google.golang.org/grpc/status"

	"stub/hello-grpc/hello/hello"
)

type GreeterServer interface {
	SayHello(context.Context, *hello.HelloRequest) (*hello.HelloReply, error)
	SayHelloEnum(context.Context, *hello.HelloEnumRequest) (*hello.HelloEnumReply, error)
	SayHelloOneOf(context.Context, *hello.HelloOneOfRequest) (*hello.HelloOneOfReply, error)
	mustEmbedUnimplementedGreeterServer()
}

type UnimplementedGreeterServer struct {
}

func (UnimplementedGreeterServer) SayHello(context.Context, *hello.HelloRequest) (*hello.HelloReply, error) {
	return nil, status.Errorf(codes.Unimplemented, "method SayHello not implemented")
}
func (UnimplementedGreeterServer) SayHelloEnum(context.Context, *hello.HelloEnumRequest) (*hello.HelloEnumReply, error) {
	return nil, status.Errorf(codes.Unimplemented, "method SayHelloEnum not implemented")
}
func (UnimplementedGreeterServer) SayHelloOneOf(context.Context, *hello.HelloOneOfRequest) (*hello.HelloOneOfReply, error) {
	return nil, status.Errorf(codes.Unimplemented, "method SayHelloOneOf not implemented")
}
func (UnimplementedGreeterServer) mustEmbedUnimplementedGreeterServer() {}
