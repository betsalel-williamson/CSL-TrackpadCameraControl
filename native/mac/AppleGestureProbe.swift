import AppKit
import CoreGraphics
import Foundation

/// Standalone spike: log AppKit trackpad gesture payloads. Not a camera backend.
enum ProbeLog {
    static func line(_ text: String) {
        fputs(text + "\n", stderr)
        fflush(stderr)
    }
}

enum TapState {
    static var port: CFMachPort?
}

func modifierNames(_ flags: NSEvent.ModifierFlags) -> String {
    var parts: [String] = []
    if flags.contains(.option) { parts.append("opt") }
    if flags.contains(.shift) { parts.append("shift") }
    if flags.contains(.command) { parts.append("cmd") }
    if flags.contains(.control) { parts.append("ctrl") }
    return parts.isEmpty ? "-" : parts.joined(separator: ",")
}

func phaseName(_ phase: NSEvent.Phase) -> String {
    if phase.isEmpty { return "none" }
    var parts: [String] = []
    if phase.contains(.began) { parts.append("began") }
    if phase.contains(.changed) { parts.append("changed") }
    if phase.contains(.ended) { parts.append("ended") }
    if phase.contains(.cancelled) { parts.append("cancelled") }
    if phase.contains(.stationary) { parts.append("stationary") }
    if phase.contains(.mayBegin) { parts.append("mayBegin") }
    return parts.isEmpty ? "other" : parts.joined(separator: "+")
}

func typeName(_ type: NSEvent.EventType) -> String {
    switch type {
    case .scrollWheel: return "scroll"
    case .magnify: return "magnify"
    case .rotate: return "rotate"
    case .swipe: return "swipe"
    case .beginGesture: return "begin"
    case .endGesture: return "end"
    case .gesture: return "gesture"
    case .smartMagnify: return "smart"
    default: return "other(\(type.rawValue))"
    }
}

func formatEvent(_ event: NSEvent, source: String) -> String {
    var fields = [
        "apple",
        "src=\(source)",
        "type=\(typeName(event.type))",
        "phase=\(phaseName(event.phase))",
        "mods=\(modifierNames(event.modifierFlags))",
    ]

    switch event.type {
    case .scrollWheel:
        fields.append("momentum=\(phaseName(event.momentumPhase))")
        fields.append(String(format: "sdx=%.4f", event.scrollingDeltaX))
        fields.append(String(format: "sdy=%.4f", event.scrollingDeltaY))
        fields.append(String(format: "dx=%.4f", event.deltaX))
        fields.append(String(format: "dy=%.4f", event.deltaY))
        fields.append("precise=\(event.hasPreciseScrollingDeltas ? 1 : 0)")
    case .magnify:
        fields.append(String(format: "mag=%.5f", event.magnification))
    case .rotate:
        fields.append(String(format: "rot=%.4f", event.rotation))
    case .swipe:
        fields.append(String(format: "dx=%.4f", event.deltaX))
        fields.append(String(format: "dy=%.4f", event.deltaY))
    default:
        break
    }

    return fields.joined(separator: " ")
}

func logEvent(_ event: NSEvent, source: String) {
    ProbeLog.line(formatEvent(event, source: source))
}

let gestureMask: NSEvent.EventTypeMask = [
    .scrollWheel,
    .magnify,
    .rotate,
    .swipe,
    .beginGesture,
    .endGesture,
    .gesture,
    .smartMagnify,
]

final class ProbeView: NSView {
    override var acceptsFirstResponder: Bool { true }

    override func viewDidMoveToWindow() {
        super.viewDidMoveToWindow()
        window?.makeFirstResponder(self)
    }

    override func magnify(with event: NSEvent) {
        logEvent(event, source: "view")
        super.magnify(with: event)
    }

    override func rotate(with event: NSEvent) {
        logEvent(event, source: "view")
        super.rotate(with: event)
    }

    override func swipe(with event: NSEvent) {
        logEvent(event, source: "view")
        super.swipe(with: event)
    }

    override func scrollWheel(with event: NSEvent) {
        logEvent(event, source: "view")
        super.scrollWheel(with: event)
    }

    override func beginGesture(with event: NSEvent) {
        logEvent(event, source: "view")
        super.beginGesture(with: event)
    }

    override func endGesture(with event: NSEvent) {
        logEvent(event, source: "view")
        super.endGesture(with: event)
    }

    override func draw(_ dirtyRect: NSRect) {
        NSColor.windowBackgroundColor.setFill()
        dirtyRect.fill()
        let text = """
        Apple Gesture Probe
        Gesture over this window. Lines go to stderr.

        Two-finger pan · Option+pan · pinch · twist · three-finger swipe
        """ as NSString
        let attrs: [NSAttributedString.Key: Any] = [
            .font: NSFont.systemFont(ofSize: 16),
            .foregroundColor: NSColor.labelColor,
        ]
        text.draw(in: bounds.insetBy(dx: 24, dy: 24), withAttributes: attrs)
    }
}

final class AppDelegate: NSObject, NSApplicationDelegate {
    private var window: NSWindow?

    func applicationDidFinishLaunching(_ notification: Notification) {
        let window = NSWindow(
            contentRect: NSRect(x: 200, y: 200, width: 640, height: 400),
            styleMask: [.titled, .closable, .resizable, .miniaturizable],
            backing: .buffered,
            defer: false
        )
        window.title = "Apple Gesture Probe"
        window.isReleasedWhenClosed = false
        window.contentView = ProbeView(frame: window.contentView?.bounds ?? .zero)
        window.makeKeyAndOrderFront(nil)
        window.makeFirstResponder(window.contentView)
        self.window = window

        NSEvent.addLocalMonitorForEvents(matching: gestureMask) { event in
            logEvent(event, source: "local")
            return event
        }

        installTapIfEnabled()
        ProbeLog.line(
            "apple src=probe type=ready tap=\(TapState.port == nil ? 0 : 1) — focus this window; no Accessibility needed. pinch, twist, two-finger scroll, Option+scroll, three-finger swipe"
        )
    }

    func applicationShouldTerminateAfterLastWindowClosed(_ sender: NSApplication) -> Bool {
        true
    }

    private func installTapIfEnabled() {
        // Default: window-local only (no Accessibility). TAP=1 is the optional other-app path.
        let enableTap = ProcessInfo.processInfo.environment["APPLE_GESTURE_PROBE_TAP"] == "1"
        if !enableTap {
            ProbeLog.line("apple src=probe type=info tap off (window-local only). Set APPLE_GESTURE_PROBE_TAP=1 to listen while another app is focused.")
            return
        }

        let cgMask = CGEventMask(gestureMask.rawValue)
        guard let tap = CGEvent.tapCreate(
            tap: .cgSessionEventTap,
            place: .headInsertEventTap,
            options: .listenOnly,
            eventsOfInterest: cgMask,
            callback: { _, type, event, _ in
                if type == .tapDisabledByTimeout || type == .tapDisabledByUserInput {
                    if let port = TapState.port {
                        CGEvent.tapEnable(tap: port, enable: true)
                    }
                    ProbeLog.line("apple src=tap type=info tap re-enabled after \(type.rawValue)")
                    return Unmanaged.passUnretained(event)
                }
                if let nsEvent = NSEvent(cgEvent: event) {
                    logEvent(nsEvent, source: "tap")
                }
                return Unmanaged.passUnretained(event)
            },
            userInfo: nil
        ) else {
            ProbeLog.line(
                "apple src=probe type=info tap failed — grant Accessibility to AppleGestureProbe, then relaunch. Window-local logging still works."
            )
            return
        }

        let source = CFMachPortCreateRunLoopSource(kCFAllocatorDefault, tap, 0)
        CFRunLoopAddSource(CFRunLoopGetMain(), source, .commonModes)
        CGEvent.tapEnable(tap: tap, enable: true)
        TapState.port = tap
    }
}

let app = NSApplication.shared
let appDelegate = AppDelegate()
app.delegate = appDelegate
app.setActivationPolicy(.regular)
app.run()
