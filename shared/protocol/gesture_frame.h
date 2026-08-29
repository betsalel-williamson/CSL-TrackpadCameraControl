#ifndef TRACKPAD_GESTURE_FRAME_H
#define TRACKPAD_GESTURE_FRAME_H

#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

#define GESTURE_FRAME_MAGIC 0x54435046u /* TCPF */
#define GESTURE_FRAME_VERSION 1
#define GESTURE_FRAME_SIZE 48

enum GesturePhase {
    GesturePhase_Began = 0,
    GesturePhase_Changed = 1,
    GesturePhase_Ended = 2,
    GesturePhase_Cancelled = 3,
};

enum GestureModifier {
    GestureModifier_Option = 1u << 0,
    GestureModifier_Shift = 1u << 1,
    GestureModifier_Command = 1u << 2,
    GestureModifier_Control = 1u << 3,
};

#pragma pack(push, 1)
typedef struct GestureFrame {
    uint32_t magic;
    uint16_t version;
    uint16_t flags;
    int64_t timestampNs;
    int32_t fingerCount;
    int32_t phase;
    float centroidDeltaX;
    float centroidDeltaY;
    float pinchScaleDelta;
    float rotateDelta;
    uint32_t modifiers;
    uint32_t reserved;
} GestureFrame;
#pragma pack(pop)

#ifdef __cplusplus
}
#endif

#endif /* TRACKPAD_GESTURE_FRAME_H */
