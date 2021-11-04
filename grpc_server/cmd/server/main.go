// example from https://github.com/grpc/grpc-go
package main

import (
	"context"
	"errors"
	"fmt"
	"log"
	"net"
	"os"

	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials"

	"stub/hello-grpc/server"
	"stub/hello-grpc/hello/hello"
)

const (
	port = ":50051"
	//port = ":443"
)

type helloServer struct {
	server.UnimplementedGreeterServer
}

// GreeterhelloServer implements
func (s *helloServer) SayHello(ctx context.Context, in *hello.HelloRequest) (*hello.HelloReply, error) {
	log.Printf("SayHello Received: %v", in.GetName())
	return &hello.HelloReply{Message: fmt.Sprintf("Hello %s", in.GetName()), Status: "ok"}, nil
}

func (s *helloServer) SayHelloEnum(ctx context.Context, in *hello.HelloEnumRequest) (*hello.HelloEnumReply, error) {
	log.Printf("SayHelloEnum Received: %v", in.GetEnum())
	return &hello.HelloEnumReply{Enum: in.GetEnum()}, nil
}

func (s *helloServer) SayHelloOneOf(ctx context.Context, in *hello.HelloOneOfRequest) (*hello.HelloOneOfReply, error) {
	log.Printf("SayHelloOneOf Received: %v", in.GetChoose())

	requestChoose, ok := in.GetChoose().(interface{})
	if !ok {
		return nil, errors.New("con not convert to interface")
	}

	switch requestChoose.(type) {
	case *hello.HelloOneOfRequest_First: {
		return &hello.HelloOneOfReply{
			Choose: &hello.HelloOneOfReply_First{
				First: in.GetFirst(),
			},
		}, nil
	}
	case *hello.HelloOneOfRequest_Second:{
		return &hello.HelloOneOfReply{
			Choose: &hello.HelloOneOfReply_Second{
				Second: in.GetSecond(),
			},
		}, nil
	}
	}

	return nil, errors.New("unexpected choose")
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

func SayHelloEnumHandler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(hello.HelloEnumRequest)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(server.GreeterServer).SayHelloEnum(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/helloworld.Greeter/SayHelloEnum",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(server.GreeterServer).SayHelloEnum(ctx, req.(*hello.HelloEnumRequest))
	}
	return interceptor(ctx, in, info, handler)
}

func SayHelloOneOfHandler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(hello.HelloOneOfRequest)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(server.GreeterServer).SayHelloOneOf(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/helloworld.Greeter/SayHelloOneOf",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(server.GreeterServer).SayHelloOneOf(ctx, req.(*hello.HelloOneOfRequest))
	}
	return interceptor(ctx, in, info, handler)
}

func main() {
	lis, err := net.Listen("tcp", port)
	if err != nil {
		log.Fatalf("failed to listen: %v", err)
	}

	creds, err := credentials.NewServerTLSFromFile(
		"/Users/kuwabara_yuki/workspace/git/dolow/unity_grpc_build_sample/grpc_server/cmd/server/192.168.11.9.pem",
		"/Users/kuwabara_yuki/workspace/git/dolow/unity_grpc_build_sample/grpc_server/cmd/server/192.168.11.9-key.pem",
	)
	if err != nil {
		log.Fatalf("failed to load credentials")
	}

	s := grpc.NewServer(grpc.Creds(creds))

	s.RegisterService(&grpc.ServiceDesc{
		ServiceName: "helloworld.Greeter",
		HandlerType: (*server.GreeterServer)(nil),
		Methods: []grpc.MethodDesc{
			{
				MethodName: "SayHello",
				Handler:    SayHelloHandler,
			},
			{
				MethodName: "SayHelloEnum",
				Handler:    SayHelloEnumHandler,
			},
			{
				MethodName: "SayHelloOneOf",
				Handler:    SayHelloOneOfHandler,
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
