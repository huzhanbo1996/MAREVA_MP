# AR game for an experiment

## Description

A mini-project for testing the effectiveness and mental charge of different signals given for obstacles and dangers. 

[Demo video](https://youtu.be/6zeUbu4NaLw)

## Getting Started

Well install git, git-lfs and Unity 2020. Importing via Unity HUB should work.

Any possible missing dependencies relating to:
+ [MRTK](https://docs.microsoft.com/en-us/windows/mixed-reality/develop/install-the-tools?tabs=unity)
+ [Nuget Unity](https://github.com/GlitchEnzo/NuGetForUnity)

Program can be directly tested in Unity play mode or on Hololens 2.


## How to use

### Editing Mode:

    Activate DevMenu1/DevMenu2 in Unity Editor. Uncheck property `IsPlayerMode` of script `Scene Manager` under Inspector in object `Scene` under Hierarchy.

    In Hololens on these menus show buttoms allowing adding/removing objects and loading/storing scene in realtime. The objects new created with property `IsPlayerMode` off can be grabbed in Hololens and resized, relocated, and rotated by hands.

    The saved file can also be read in Unity Editor via Inspector of object `Scene`. All objects is under `Scene/Arrangement`, thus the scene can also be configured in Unity Editor. Sometimes in Unity Editor a `Force reload` is needed after Editor's refresh or script compiling.

    This editing via game in real world and editor can be iterated for obtaining a satisfying result.

### Play Mode:

    After adjustement, check `IsPlayerMode` of `Scene Manager` in Unity Editor, click `Read Scene`, the objects will be replaced by the true objects containing only gaming logics.

## Final Version:

    1. Load/Save in SceneManager will not restore the scene accurately since the objects in scene have been adjusted in details not only in transform of root object but also in the settings of children objects.

    2. Although lots of mesh colliders have been removed, it exists always the performance issue due to possible eye tracking which's very CPU consuming by their collision calculation.

    3. Improvement is possible by making a slide-window scene loading mechanism instead of loading all stuff in the very beginning.

    4. Sometimes app fails rendering due to high CPU usage, espacially when we start by point 2. It can occur like this: after the adjustement by QR code, suddenly everythings varnished. Or it's also possible that the rendering failure is due to location tracking failure caused by Hololens itself.


## Acknowledgments

[yl-msft/QRTracking](https://github.com/yl-msft/QRTracking)