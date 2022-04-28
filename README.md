# Changes
- Issues with chunk generation math fixed.
- Mesh index buffer changed to 32-bit (2,147,483,647 max vertices) from the default 16-bit (65,535 max vertices).

# Todo
- Use the Unity job system for chunk generation.

# Warnings:
- 32-bit mesh index buffers aren't supported on all super-low-end devices. However, it's very widely supported.