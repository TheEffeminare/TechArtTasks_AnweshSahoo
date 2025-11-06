# Tech Art Test - Anwesh Sahoo

This project represents my approach to building modular, scalable, and thoughtfully designed UI systems within Unity.  
Each task focuses on a different aspect of how I think about technical art - the intersection of structure, responsiveness, and visual expression.



## Overview

As a Technical Artist, I have always been drawn to the space where design and engineering meet.  
My intent throughout this project was to create a system that feels both artistically refined and technically sound - one that can adapt, scale, and endure within a production pipeline.

Developed in Unity 2022.3.31f1 (URP), the project includes three main tasks:

1. Home Screen and Bottom Bar  
2. Settings Popup System  
3. Level Completed Screen

Every decision - from naming conventions to visual transitions - was made with an eye for clarity, extensibility, and purpose.



## Task 1: Home Screen and Bottom Bar

### Goal
To implement a responsive home screen interface that adapts seamlessly across devices, supported by an animated and modular bottom navigation bar.

### Implementation
- **Responsive Canvas System:**  
  The `ResponsiveCanvasMatch.cs` script dynamically adjusts the Canvas Scaler’s ratio between width and height, ensuring the layout maintains visual balance from tall phones to tablets.

- **Adaptive Padding:**  
  Using `ResponsiveBarPadding.cs`, both top and bottom bars interpolate their spacing and padding based on aspect ratio, maintaining proportional composition across devices.

- **Safe Area Integration:**  
  The combination of `SafeAreaFitter.cs` and `UIRectNaNGuard.cs` ensures the interface always respects device safe zones, automatically correcting invalid transforms caused by prefab or resolution changes.

- **Animated Navigation:**  
  Each bottom bar button includes toggle feedback built using `ToggleKnob.cs` and `ToggleBackgroundSwap.cs`. The result is a set of controls that feel responsive, tactile, and consistent.

This structure allows any designer or developer to introduce new navigation buttons or reskin the bar without reworking the underlying logic - a key part of how I think about maintainability in UI systems.



## Task 2: Settings Popup

### Goal
To build a reusable popup architecture that feels cohesive, scalable, and ready for future extensions such as localization and theming.

### Implementation
- **Popup Core Architecture:**  
  The base system, built around `PopupDriver.cs`, manages open and close states through animator triggers. It also handles canvas fading, input blocking, and background dimming, allowing any new popup (for example, Shop or Help) to inherit the same structure effortlessly.

- **Content Scaling:**  
  `ScaleToSafeHeight.cs` ensures popup content maintains visual proportion regardless of device resolution or safe area height.

- **Row and Layout Design:**  
  Each settings row (Sound, Music, Vibration, Notification, Language) was built as a self-contained component with modular toggles, icons, and dividers.  
  This enables the popup to grow organically as new features are introduced.

- **Interaction Design:**  
  The interface favors subtlety. Toggles slide with controlled easing, and buttons respond visually to touch - small choices that make the system feel deliberate and complete.

The result is a system that can scale to accommodate more functionality without losing its structural clarity or aesthetic coherence.


## Task 3: Level Completed Screen

### Goal
To create a visually rich and emotionally rewarding moment that celebrates player achievement, combining animation, UI, and particle effects in harmony.

### Implementation
- **Central Controller:**  
  The `LevelCompleteDriver.cs` script orchestrates the sequence - fading in the canvas, triggering confetti and background effects, animating score counters, and then closing the sequence smoothly.

- **VFX and Particle Logic:**  
  All particle systems are cached and reset cleanly through utility functions that manage emission, color restoration, and state clearing.  
  This ensures the screen behaves predictably even after multiple activations.

- **Animated Score Feedback:**  
  The score counter uses coroutine-based interpolation to gradually reveal the final value, timed to align with the peak of the visual effects for maximum impact.

- **Visual Cohesion:**  
  Elements such as the confetti burst, circular rays, and top highlights are timed and layered intentionally, building a moment that feels both joyful and controlled.

This section was a chance to explore how subtle design choices - timing, fade duration, particle rhythm - can transform a simple UI screen into an experience.



## Technical and Artistic Considerations

- **Code Structure:**  
  Each script follows a component-based architecture with serialized fields and clear naming conventions.  
  The folder hierarchy was designed for legibility and scalability:


- **Performance Practices:**  
I minimized per-frame updates in favor of event-driven logic.  
Particles are toggled, not destroyed.  
Animations are GPU-efficient and compatible with lightweight URP rendering.

- **Testing and Responsiveness:**  
All components were validated across multiple aspect ratios (from 21:9 phones to 4:3 tablets).  
The adaptive padding, safe area fitters, and canvas scaling ensure the layout remains cohesive regardless of resolution.



## Reflections

This project is more than a test - it represents my design philosophy.  
I believe that great technical art is not just about solving problems but about **solving them beautifully**.  
Every choice, from animation curves to folder structure, was intentional.  

In future iterations, I’d like to explore:
- Depth-of-field blur using URP volume profiles  
- Sparkle and shimmer layers for the Level Complete sequence  
- A unified event-driven UI manager for all popups  
- Localization support for dynamic text components

This exercise reaffirmed how much I enjoy bridging creativity and precision - shaping systems that are efficient for engineers and expressive for artists.



### Anwesh Sahoo  
Technical Artist  
[https://anweshsahoo.com](https://anweshsahoo.com)
