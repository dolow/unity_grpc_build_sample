// https://github.com/grpc/grpc-go/blob/master/examples/helloworld/greeter_client/main.go
package main

import (
	"context"
	"log"
	"os"
	"time"

	"google.golang.org/grpc"

	"stub/hello-grpc/hello/hello"
)

var (
	host = "localhost"
)

const (
	port = "50051"
	defaultName = "world"
)

type greeterClient struct {
	cc grpc.ClientConnInterface
}

func (c *greeterClient) SayHello(ctx context.Context, in *hello.HelloRequest, opts ...grpc.CallOption) (*hello.HelloReply, error) {
	out := new(hello.HelloReply)
	err := c.cc.Invoke(ctx, "/helloworld.Greeter/SayHello", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func main() {
	userHost := os.Getenv("TEST_GRPC_HOST")
	if userHost != "" {
		host = userHost
	}
	// Set up a connection to the server.
	conn, err := grpc.Dial(host + ":" + port, grpc.WithInsecure(), grpc.WithBlock())
	if err != nil {
		log.Fatalf("did not connect: %v", err)
	}
	defer conn.Close()
	c := &greeterClient{conn}

	// Contact the server and print out its response.
	name := defaultName
	if len(os.Args) > 1 {
		name = os.Args[1]
	}
	ctx, cancel := context.WithTimeout(context.Background(), time.Second)
	defer cancel()
	r, err := c.SayHello(ctx, &hello.HelloRequest{Name: name})
	if err != nil {
		log.Fatalf("could not greet: %v", err)
	}
	log.Printf("Greeting: %s", r.GetMessage())
}
