Visual Agent Design Interface
=============================

This repository will contain the Visual Agent Design Interface Specific Enabler (SE) of the FIcontent Pervasive Games Platform.

This SE gives access to the [Aseba network](http://thymio.org) and the [Visual Programming Language](https://aseba.wikidot.com/en:thymiovpl) to Unity developers.
It does this by providing the following C# libarires:
 * Aseba network interface in `LiveInspector/Assets/Aseba`
 * Visual Programming Language renderer in `LiveInspector/Assets/VPL` (including a library of various blitting function operating on arrays of Color32)

In addition, this repository provides a VPL live inspector which is an augmented reality visual debugger for VPL.
This inspector is used both as a demonstration and to run experiments.

Download
--------

You can clone this repository:

    git clone --recursive https://github.com/fi-content2-games-platform/FIcontent.Gaming.Enabler.VisualAgentDesignInterface.git

Usage
-----

You need Unity3D (tested with Pro version 4.5.3f3) to use this SE.
From Unity, simply opens the LiveInspector directory as a project, and open `LiveInspector/Assets/DefaultScene.unity`