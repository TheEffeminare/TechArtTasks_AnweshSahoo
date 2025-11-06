# 🎨 Tech Art Test — Anwesh Sahoo

Welcome to my Tech Art Test submission!  
This project showcases my approach to building **modular, scalable, and visually polished UI systems** in Unity.  
It includes three interconnected tasks — each focusing on different aspects of UI architecture, responsiveness, and creative implementation.

---

## 🧭 Overview

As a **Technical Artist**, my goal throughout this project was to design a foundation that is:
- Clean, modular, and easy to extend  
- Artist-friendly, with visually rich transitions and responsive layouts  
- Technically sound, balancing performance with polish  

Developed in **Unity 2022.3.31f1 (URP)**, this project includes:
1. **Home Screen & Bottom Bar**
2. **Settings Popup System**
3. **Level Completed Screen**

---

## 📱 Task 1: Home Screen & Bottom Bar

### 🎯 Goal
Implement a responsive home screen UI with adaptive top and bottom bars and animated navigation buttons.

### 🧩 Features
- **Dynamic Canvas Scaling:**  
  `ResponsiveCanvasMatch.cs` automatically adjusts the CanvasScaler’s match ratio between width and height for phones and tablets.
  
- **Smart Padding System:**  
  `ResponsiveBarPadding.cs` interpolates left/right padding and spacing for both the top and bottom bars, ensuring proportional layouts across devices.

- **Safe Area Support:**  
  `SafeAreaFitter.cs` and `UIRectNaNGuard.cs` ensure the UI stays within device safe zones and heals any corrupted RectTransform values during prefab editing.

- **Animated Bottom Bar:**  
  Custom toggle logic via:
  - `ToggleKnob.cs` (knob animation)
  - `ToggleBackgroundSwap.cs` (sprite swapping for ON/OFF states)
  
- Fully prefabbed for **scalability and reuse** in future screens.

---

## ⚙️ Task 2: Settings Popup

### 🎯 Goal
Develop a reusable popup architecture with animation, dimming, and localization-ready structure.

### 🧩 Features
- **Modular Popup System:**  
  `PopupDriver.cs` powers all popup behaviors — open/close triggers, canvas fade, and dimmer management.  
  Other popups (like Help or Shop) can inherit this logic instantly.

- **Dynamic Scaling:**  
  `ScaleToSafeHeight.cs` keeps popup content proportionate on any device.

- **Background Blur Prototype:**  
  `UIBlurGrabFeature.cs` is a custom URP renderer feature that grabs the screen into `_UIBlurTex`, laying the groundwork for Gaussian or DoF-based blur effects.

- **Row-based Layout:**  
  Each setting row (`Sound`, `Music`, `Vibration`, etc.) is modular with independent toggles, dividers, and icons.

- **Smooth Interactions:**  
  Toggles slide with curves, buttons highlight with feedback, and the popup feels alive.

---

## 🏆 Task 3: Level Completed Screen

### 🎯 Goal
Design a celebratory “Level Complete” experience that combines UI, animation, and VFX for a rewarding moment.

### 🧩 Features
- **Main Controller:**  
  `LevelCompleteDriver.cs` orchestrates all transitions — fading in UI, triggering confetti and bursts, animating score counters, and fading out gracefully.

- **Particle Control:**  
  Built-in safety ensures no particle ghosting. All VFX reset and restore material colors cleanly on re-open.

- **Score Counter Animation:**  
  Coroutine-based number tick with easing for a satisfying reward reveal.

---

## 💬 Final Thoughts

This project captures how I approach real-world game UI challenges — blending **artistic sensitivity with technical depth**.  
From modular prefabs to shader experimentation, every detail reflects my intent to create systems that are **delightful to use, easy to extend, and built for the future**.

Thanks for taking the time to review my work! 🌟

— **Anwesh Sahoo**  
🎮 Technical Artist | [anweshsahoo.com](https://anweshsahoo.com)
