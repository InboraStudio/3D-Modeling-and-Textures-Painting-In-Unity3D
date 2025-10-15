# 3D Modeling and Textures Painting In Unity3D
```A Project That I build in Free Time  😊```
A comprehensive toolkit for creating, editing, and texturing 3D models directly within the Unity Editor. This Blender-like asset integrates modeling, UV unwrapping, texture painting, and material editing into a single unified workflow.

## Features

- **Integrated Modeling Environment**: Create and edit 3D models directly in Unity
- **UV Unwrapping**: Powerful UV editing and unwrapping tools
- **Texture Painting**: Direct painting on 3D models in the viewport
- **Material Editing**: PBR material editing with real-time preview
- **Single Workflow**: Seamlessly switch between modeling, UV editing, texturing, and material editing

## Graphic Engine 

![Recording 2025-10-15 221305](https://github.com/user-attachments/assets/236d3ef4-b0a5-4872-803c-cd6588695858)

## Vulkun Info
### Single Integrated Workspace
- All features accessible from one editor window
- Scene-like viewport with camera controls (orbit, pan, zoom)
- Consistent UI across all tool modes

## Vector Processing Formula
<img width="507" height="233" alt="image" src="https://github.com/user-attachments/assets/9e063617-7c4a-4ced-a014-77d32af01a6c" />

## How it's Working ? 
- Linear Algebra (Core Foundation)
Everything in 3D graphics is built on linear algebra.
- Vectors → Represent position, direction, velocity, color (RGB), normals.
```
 v =(x,y,z)
```
- Matrices → Represent transformations like translation, rotation, scaling, and projection.
```
Model-View-Projection: MMVP​=Mprojection​⋅Mview​⋅Mmodel​
```
- Dot Product → For lighting intensity and angle between vectors.
- Interpolation (slerp):

<img width="703" height="152" alt="image" src="https://github.com/user-attachments/assets/def1a65e-4469-49d0-a9d4-8fe0367f5d8f" />

```
I=L⋅N
```
- Cross Product → For normals and perpendicular directions.
```
N=A×B
```
Vectors → Represent position, direction, velocity, color (RGB), normals
### Modeling Tools
- Create basic primitives (cube, sphere, cylinder, plane)
- Select and edit vertices, edges, and faces
- Transform operations (move, rotate, scale)
- Advanced operations (extrude, bevel, loop cut)
- Mesh smoothing and subdivision
- Boolean operations (union, subtract, intersect)

### UV Editing
- Automatic UV unwrapping
- Multiple projection methods (planar, box, spherical, cylindrical)
- Manual UV adjustment with direct manipulation
- UV packing and optimization

## Realtime Sync To Unity Graphic Engine

![Recording 2025-10-15 223258](https://github.com/user-attachments/assets/61fa6b43-96e9-430d-afda-0b1133027ad6)



### Texture Painting
- Direct painting on 3D models in the viewport
- Support for multiple texture channels (albedo, normal, metallic, etc.)
- Customizable brush settings (size, hardness, opacity)
- Layer-based texture editing
<img width="2272" height="1094" alt="image" src="https://github.com/user-attachments/assets/b2cc3c5d-d4e2-4aec-baf3-670cf6f653a7" />

### Material Editing
- PBR material editing with real-time preview
- Adjust standard material properties (color, metallic, smoothness)
- Import and export textures
- Create and save materials
<img width="1024" height="768" alt="image" src="https://github.com/user-attachments/assets/f3ae05ec-e41e-451c-a664-f2d5cfebfef8" />

### Import/Export
- Import models from OBJ format
- Export models with UVs and materials
- Support for FBX and GLB (requires additional packages)

### Type Kernal
1. Navier–Stokes equations describe fluid motion

<img width="430" height="92" alt="image" src="https://github.com/user-attachments/assets/fcbaebbc-2ceb-464b-b525-c3f58b0b9352" />


2. Simplified for particle systems (SPH - Smoothed Particle Hydrodynamics).
- Used for smooth 3D rotation without gimbal lock.
- Represent rotation in 4D space:
  ```
  q=w+xi+yj+zk
  ```
- Integration → For motion over time, e.g. physics updates.
  ```
  x(t+Δt)=x(t)+v(t)Δt
  ```

## Getting Started

1. Open the toolkit window from:
   `Window > 3D Toolkit > Integrated Modeling Tool`

2. Select or import a 3D model to begin editing

3. Use the top toolbar to switch between tool modes:
   - **Model**: For mesh editing operations
   - **UV**: For UV unwrapping and editing
   - **Texture**: For texture painting
   - **Material**: For material editing

4. Use the right-click + drag to orbit the camera, scroll to zoom, and shift + right-click to pan

## Keyboard Shortcuts

- **F**: Focus on selected object
- **Q**: Switch to Select mode
- **W**: Switch to Move mode
- **E**: Switch to Rotate mode
- **R**: Switch to Scale mode
- **1-4**: Switch between Model, UV, Texture, and Material modes
- **Ctrl+Z**: Undo
- **Ctrl+Y**: Redo
- **Ctrl+S**: Save

## Requirements

- Unity 2020.3 or newer
- For FBX import/export: Unity's FBX Exporter package
- For GLB import/export: Unity's glTF importer/exporter package

## Known Limitations

- Boolean operations are placeholder implementations
- Some advanced modeling operations are simplified
- High-poly meshes may impact performance

## Extending the Toolkit

The toolkit is designed to be modular and extensible:

1. Add new mesh operations by extending the `MeshOperations` class
2. Create custom brushes by extending the `TexturePainter` class
3. Add new material shaders by modifying the `MaterialEditor` class

## Troubleshooting

- **Import/Export issues**: Ensure the required packages are installed
- **Performance issues**: Reduce mesh complexity or texture resolution
- **UV unwrapping problems**: Try different projection methods

## Getting Started

1. Open the toolkit from: `Window > 3D Toolkit > Integrated Modeling Tool`
2. Import or create a 3D model to begin editing
3. Use the top toolbar to switch between different tool modes

## Requirements

- Unity 2020.3 or newer
- For FBX import/export: Unity's FBX Exporter package
- For GLB import/export: Unity's glTF importer/exporter package

## Documentation

For detailed documentation, see the [in-tool README](Editor/3DToolkit/README.md).

## License

This toolkit is provided under the MIT License. 
