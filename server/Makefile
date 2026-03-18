.PHONY: all skynet lua-protobuf clean

all: skynet lua-protobuf

skynet:
	cd skynet && make linux

lua-protobuf:
	cd 3rd/lua-protobuf && make

clean:
	cd skynet && make clean
	cd 3rd/lua-protobuf && make clean
