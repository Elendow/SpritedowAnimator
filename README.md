# Unity Simple Sprite Animation Plugin
A plugin to do simple sprite animations avoiding the big and tedious Mechanim system.

# Installation Guide
Simply copy the files on your project or use te UnityPackage included.

# Creating an animation
Use the animation editor to create new animation files. It's easy an simple to use.

# Configuring the animator
- **playOnAwake** will start playing the animation on the object awake
- **framesPerSecond** is the speed of the animation
- **animations** is a list with all the animations

# Using the animations
Add the animator component to the object you want to animate.

- **PlayOneShot(string animationName)** reproduces the animation only one time.
- **Play(string animationName)** plays the animation infinite times.
- **Stop()** stops the current animation.
- **Reset()** restarts the animation (playing or not) to its initial state.
- **IsPlaying** returns true if the animation is playing and false if not.

# Events
- **onFinish** calls when the animation reach the last frame.
- **onPlay** calls when the animation start playing.
- **onStop** calls when the animation is forced to stop.

# Licence
MIT
