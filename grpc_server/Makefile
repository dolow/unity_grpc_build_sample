.PHONY: all
all:
	make server
	make client

.PHONY: server
server:
	mkdir -p ./bin/darwin_amd64
	mkdir -p ./bin/linux_amd64
	mkdir -p ./bin/windows_386
	GOOS=darwin  GOARCH=amd64 go build -o ./bin/darwin_amd64/server ./cmd/server/main.go
	GOOS=linux   GOARCH=amd64 go build -o ./bin/linux_amd64/server ./cmd/server/main.go
	GOOS=windows GOARCH=386   go build -o ./bin/windows_386/server ./cmd/server/main.go


.PHONY: client
client:
	mkdir -p ./bin/darwin_amd64
	mkdir -p ./bin/linux_amd64
	mkdir -p ./bin/windows_386
	GOOS=darwin  GOARCH=amd64 go build -o ./bin/darwin_amd64/client ./cmd/test_client/main.go
	GOOS=linux   GOARCH=amd64 go build -o ./bin/linux_amd64/client ./cmd/test_client/main.go
	GOOS=windows GOARCH=386   go build -o ./bin/windows_386/client ./cmd/test_client/main.go
