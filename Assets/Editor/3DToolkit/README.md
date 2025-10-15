# 3D Modeling & Texturing Toolkit for Unity

A comprehensive toolkit for creating, editing, and texturing 3D models directly within the Unity Editor. This Blender-like asset integrates modeling, UV unwrapping, texture painting, and material editing into a single unified workflow.

## Features

### Single Integrated Workspace
- All features accessible from one editor window
- Scene-like viewport with camera controls (orbit, pan, zoom)
- Consistent UI across all tool modes

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

### Texture Painting
- Direct painting on 3D models in the viewport
- Support for multiple texture channels (albedo, normal, metallic, etc.)
- Customizable brush settings (size, hardness, opacity)
- Layer-based texture editing

### Material Editing
- PBR material editing with real-time preview
- Adjust standard material properties (color, metallic, smoothness)
- Import and export textures
- Create and save materials

### Import/Export
- Import models from OBJ format
- Export models with UVs and materials
- Support for FBX and GLB (requires additional packages)

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

## License

This toolkit is provided under the MIT License. See LICENSE.md for details.

## Acknowledgments

- Built on Unity's Editor API
- Inspired by Blender's workflow
- Special thanks to the Unity community 