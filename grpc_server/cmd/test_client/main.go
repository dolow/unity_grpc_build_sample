// https://github.com/grpc/grpc-go/blob/master/examples/helloworld/greeter_client/main.go
package main

import (
	"context"
	"crypto/tls"
	"crypto/x509"
	"fmt"
	"io/ioutil"
	"log"
	"os"
	"time"

	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials"

	"stub/hello-grpc/hello/hello"
)

var (
	//host = "localhost"
	host = "192.168.11.9"
)

const (
	port = "50051"
	//port = "443"
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

func (c *greeterClient) SayHelloEnum(ctx context.Context, in *hello.HelloEnumRequest, opts ...grpc.CallOption) (*hello.HelloEnumReply, error) {
	out := new(hello.HelloEnumReply)
	err := c.cc.Invoke(ctx, "/helloworld.Greeter/SayHelloEnum", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func (c *greeterClient) SayHelloOneOf(ctx context.Context, in *hello.HelloOneOfRequest, opts ...grpc.CallOption) (*hello.HelloOneOfReply, error) {
	out := new(hello.HelloOneOfReply)
	err := c.cc.Invoke(ctx, "/helloworld.Greeter/SayHelloOneOf", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func main() {
	var err error

	userHost := os.Getenv("TEST_GRPC_HOST")
	if userHost != "" {
		host = userHost
	}
	// Set up a connection to the server.
	caFile := "/Users/kuwabara_yuki/workspace/git/dolow/unity_grpc_build_sample/grpc_server/cmd/test_client/rootCA.pem"

	cert, err := tls.LoadX509KeyPair(
		"/Users/kuwabara_yuki/workspace/git/dolow/unity_grpc_build_sample/grpc_server/cmd/test_client/192.168.11.9.pem",
		"/Users/kuwabara_yuki/workspace/git/dolow/unity_grpc_build_sample/grpc_server/cmd/test_client/192.168.11.9-key.pem",
	)
	if err != nil {
		log.Fatalf("could not load certificate")
	}
	caCert, err := ioutil.ReadFile(caFile)
	if err != nil {
		log.Fatalf("could not load root certificate")
	}

	caCertPool := x509.NewCertPool()
	if ok := caCertPool.AppendCertsFromPEM(caCert); !ok {
		log.Fatalf("could not append certificate")
	}

	opts := []grpc.DialOption{
		grpc.WithTransportCredentials(credentials.NewTLS(&tls.Config{
			Certificates: []tls.Certificate{cert},
			RootCAs:      caCertPool,
		})),
		grpc.WithBlock(),
	}
fmt.Println("Dialing " + host + ":" + port)
	conn, err := grpc.Dial(host + ":" + port, opts...)
	if err != nil {
		log.Fatalf("did not connect: %v", err)
	}
	defer conn.Close()
	c := &greeterClient{conn}
fmt.Println("Connected")
	// Contact the server and print out its response.
	ctx, cancel := context.WithTimeout(context.Background(), time.Second)
	defer cancel()

	var r interface{}

	r, err = c.SayHello(ctx, &hello.HelloRequest{Name: "world"})
	if err != nil {
		log.Fatalf("could not greet: %v", err)
	}
	log.Printf("SayHello: %s", r.(*hello.HelloReply).GetMessage())

	r, err = c.SayHelloEnum(ctx, &hello.HelloEnumRequest{Enum: hello.HelloEnum_ONE})
	if err != nil {
		log.Fatalf("could not greet: %v", err)
	}
	log.Printf("SayHelloEnum: %s", r.(*hello.HelloEnumReply).GetEnum())


	var choose interface{}
	var ok bool

	r, err = c.SayHelloOneOf(ctx, &hello.HelloOneOfRequest{Choose: &hello.HelloOneOfRequest_First { First: "first one of" }})
	if err != nil {
		log.Fatalf("could not greet: %v", err)
	}
	choose, ok = r.(*hello.HelloOneOfReply).GetChoose().(interface{})
	if !ok {
		log.Fatalf("could not convert to interface")
	}
	switch choose.(type) {
	case *hello.HelloOneOfReply_First: {
		log.Printf("SayHelloOneOf: %s", r.(*hello.HelloOneOfReply).GetFirst())
	}
	case *hello.HelloOneOfReply_Second: {
		log.Printf("SayHelloOneOf: %d", r.(*hello.HelloOneOfReply).GetSecond())
	}
	}

	r, err = c.SayHelloOneOf(ctx, &hello.HelloOneOfRequest{Choose: &hello.HelloOneOfRequest_Second { Second: 32 }})
	if err != nil {
		log.Fatalf("could not greet: %v", err)
	}
	choose, ok = r.(*hello.HelloOneOfReply).GetChoose().(interface{})
	if !ok {
		log.Fatalf("could not convert to interface")
	}
	switch choose.(type) {
	case *hello.HelloOneOfReply_First: {
		log.Printf("SayHelloOneOf: %s", r.(*hello.HelloOneOfReply).GetFirst())
	}
	case *hello.HelloOneOfReply_Second: {
		log.Printf("SayHelloOneOf: %d", r.(*hello.HelloOneOfReply).GetSecond())
	}
	}
}
