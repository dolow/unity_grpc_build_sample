package server

import (
	"context"

	codes "google.golang.org/grpc/codes"
	status "google.golang.org/grpc/status"

	"stub/hello-grpc/hello/hello"
)

type GreeterServer interface {
	SayHello(context.Context, *hello.HelloRequest) (*hello.HelloReply, error)
	mustEmbedUnimplementedGreeterServer()
}

type UnimplementedGreeterServer struct {
}

func (UnimplementedGreeterServer) SayHello(context.Context, *hello.HelloRequest) (*hello.HelloReply, error) {
	return nil, status.Errorf(codes.Unimplemented, "method SayHello not implemented")
}
func (UnimplementedGreeterServer) mustEmbedUnimplementedGreeterServer() {}
