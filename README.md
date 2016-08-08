# Unity Simple Sprite Animation Plugin
A plugin to do simple sprite animations avoiding the big and tedious Mechanim system. Oriented to programmers, if you prefer visual scripting you maybe use Mechanim instead of this.

# Installation Guide
Simply copy the files on your project or use the UnityPackage included.

# Creating an animation
Use the animation editor to create new animation files. You can open it selecting **Sprite Animation Editor** on **Elendow Tools// tab.
- Give a name to the animation.
- Select the folder to save.
- Add frames manually or dropping the sprite to the Drag&Drop box.
- Any change is automatically saved.

# Configuring the animator
- **playOnAwake** will start playing the first animation of the list on the object awake.
- **framesPerSecond** is the speed of the animation, 0 will pause the animation.
- **animations** is a list with all the animations.

# Using the animations
Add the animator component to the object you want to animate and fill the animations list with the animations you want. 
This component requires a SpriteRenderer component to work. If the object doesn't have one, the animator will add it automatically.

# Methods
- **PlayOneShot(string animationName)** reproduces the animation only one time.
- **Play(string animationName)** plays the animation infinite times.
- **Stop()** stops the current animation.
- **Reset()** restarts the animation (playing or not) to its initial state.

# Properties
- **IsPlaying** returns true if the animation is playing and false if not.

# Events
- **onFinish** calls when the animation reach the last frame.
- **onPlay** calls when the animation starts playing.
- **onStop** calls when the animation is forced to stop.

# License
MIT
