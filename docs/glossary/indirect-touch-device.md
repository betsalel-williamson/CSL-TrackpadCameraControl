# Indirect touch device

A multi-finger surface whose coordinate space is **its own**, not the screen's: a laptop touchpad, a Magic Trackpad, or a virtual trackpad created by a streaming host. Contrast a **direct** device (touchscreen), where a contact means the pixel under the finger.

The distinction decides what an app receives on Linux. libinput interprets pinch and swipe for indirect devices only; touchscreens deliver raw touchpoints and leave interpretation to the caller. On X11 and XWayland those interpreted gestures reach ordinary clients as XInput2 2.4 gesture events — see the features guide _Linux gesture backend_.

It also decides who owns a stream of input: gestures from an indirect device feed the mod, while a mouse stays on vanilla camera paths, which is how a trackpad and a plugged-in mouse coexist without a mode. Streaming hosts matter here because they choose which kind of device to create — see the features guide _Remote streaming gestures_.
