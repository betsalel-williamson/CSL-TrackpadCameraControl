.PHONY: probe clean

probe: AppleGestureProbe

AppleGestureProbe: AppleGestureProbe.swift
	swiftc -O -framework AppKit -framework CoreGraphics -o AppleGestureProbe AppleGestureProbe.swift

clean:
	rm -f AppleGestureProbe
