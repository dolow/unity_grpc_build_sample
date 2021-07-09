// example from https://github.com/grpc/grpc-go
package main

import (
	"context"
	"log"
	"net"

	"google.golang.org/grpc"

	"stub/hello-grpc/server"
	"stub/hello-grpc/hello/hello"
)

const (
	port = ":50051"
)

type helloServer struct {
	server.UnimplementedGreeterServer
}

// GreeterhelloServer implements
func (s *helloServer) SayHello(ctx context.Context, in *hello.HelloRequest) (*hello.HelloReply, error) {
	log.Printf("Received: %v", in.GetName())
	return &hello.HelloReply{Message: "Hello " + in.GetName()}, nil
}

func (*helloServer) mustEmbedUnimplementedGreeterServer() {}

func SayHelloHandler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(hello.HelloRequest)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(server.GreeterServer).SayHello(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/helloworld.Greeter/SayHello",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(server.GreeterServer).SayHello(ctx, req.(*hello.HelloRequest))
	}
	return interceptor(ctx, in, info, handler)
}

func main() {
	lis, err := net.Listen("tcp", port)
	if err != nil {
		log.Fatalf("failed to listen: %v", err)
	}
	s := grpc.NewServer()

	s.RegisterService(&grpc.ServiceDesc{
		ServiceName: "helloworld.Greeter",
		HandlerType: (*server.GreeterServer)(nil),
		Methods: []grpc.MethodDesc{
			{
				MethodName: "SayHello",
				Handler:    SayHelloHandler,
			},
		},
		Streams:  []grpc.StreamDesc{},
		Metadata: "examples/helloworld.proto",
	}, &helloServer{})

	log.Printf("server listening at %v", lis.Addr())
	if err := s.Serve(lis); err != nil {
		log.Fatalf("failed to serve: %v", err)
	}
}
