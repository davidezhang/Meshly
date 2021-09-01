<!--
*** README for GSD6338 Final Project - Meshly: A VR mesh making app
*** Author: Davide Zhang
*** Date: 12/14/2020
-->


# Meshly: A VR Low-Poly Mesh Maker
Note: only scripts were uploaded here due to GitHub file size limits and potential issues of Git and Unity.


<!-- ABOUT THE PROJECT -->
## About The Project
Meshly is an intuitive low-poly mesh modeling application in VR.  It is developed with the goal of being adopted by designers in their workflow as well as hobbyists without training in 3D modeling software. The project was built in Unity, written in C#, and runs on the Oculus Quest 2 headset.

The motivation of the project stems from the need for intuitive, freeform 3D modeling methods and the steep learning curve of professional mesh modeling software. While software like Blender, Zbrush, and Maya, etc. is extremely powerful for both personal and commercial usage, it often requires a precise way of modeling that has to be learned and practiced.


On the other hand, freeform objects seem to be a natural result of gestural and imprecise mechanisms. Meshly is a more intuitive way of modeling low-poly meshes quickly that is more akin to the actual process of sculpting. An untrained user is able to pick this up and model something for fun (which could be 3D printed, for example) without the burden of the steep learning curve.

The interaction design is inspired by the small scale sculpting process where one holds and rotates the object in their non-dominant hand and uses their dominant hand to sculpt. In Meshly's modeling environment, the user could use the left controller as their “hand” that holds and orients the object. The right controller is then a metaphor for the “tool” that manipulates the object, much like a chisel or any other sculpting tool.  To stay true to the simplicity of this paradigm and achieve more from less, the player movement is confined and direct controller collision replaces the common raycast pointer model.


## Interface
There are 3 modes of interacting with the mesh; first is the create mode, where the user can create vertices, highlight and select, and make triangle faces through clockwise selection of 3 vertices. The user is also able to delete vertices, which will in turn delete triangle faces related to those vertices.

The second mode is sculpt mode, which could be toggled from the radial menu via the left controller thumbstick. It allows for selecting and moving vertices. In future iterations, more ways of sculpting the mesh could be added, such as attractor point deformation.

The third mode is color mode, where the user could color the vertices. Drag the Hue Ring selector and simply touch the Value triangle to finish color selection. The user is able to achieve a blended look or a discrete look depending on the number of vertices having the same color.

Aside from the menu, the left controller rotates and moves the entire mesh by pressing its index button.

Finally, there is also a simple file IO functionality with the left controller. The Y button imports an OBJ file from a specified path, while the X button exports the current mesh to the same path. The application runs via Oculus Link so the path is on the computer, but could be changed to an on device location if deployed as a standalone app.


<!-- [![Product Name Screen Shot][product-screenshot]](https://example.com) -->




### Built With

* []() Unity 2019.4.15f1
* []() Oculus Integration Package Version 23.0
* []() Deployed to Oculus Quest 2



<!-- GETTING STARTED -->
## Getting Started

To be updated.





<!-- USAGE EXAMPLES -->
## Usage

<!-- Use this space to show useful examples of how a project can be used. Additional screenshots, code examples and demos work well in this space. You may also link to more resources.

_For more examples, please refer to the [Documentation](https://example.com)_ -->



## Known Bugs
1. Can only delete one vertex per time
2. OBJ import might work with file exported from game, but not external
3. OBJ import export do not retain vertex color information

<!-- ROADMAP -->
## Roadmap

To be updated.

<!-- LICENSE -->
## License

Distributed under the MIT License. See `LICENSE` for more information.


<!-- ACKNOWLEDGEMENTS -->
## Acknowledgements
Acknowledgements of 3rd party scripts are in the scripts.
Radial menu and color picker assets are purchased from Unity Asset Store and modified by me.