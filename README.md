# Changes
- Fixed out of bound exception which occurred when viewing chunks at top of very right of the map.
- Chunk generation is now more intelligent and avoids regenerating chunks at the same detail it already has.

# Todo
- Use the Unity job system for chunk generation.

# Warnings
- 32-bit mesh index buffers aren't supported on all super-low-end devices. However, it's very widely supported.