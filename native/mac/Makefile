# macOS TrackpadBridge
CC ?= clang
CFLAGS ?= -Wall -Wextra -O2
FRAMEWORKS = -framework CoreFoundation
INCLUDES = -I../../shared/protocol

.PHONY: all clean run

all: TrackpadBridge

TrackpadBridge: TrackpadBridge.c ../../shared/protocol/gesture_frame.h
	$(CC) $(CFLAGS) $(INCLUDES) -o $@ TrackpadBridge.c $(FRAMEWORKS)

run: TrackpadBridge
	./TrackpadBridge

clean:
	rm -f TrackpadBridge
