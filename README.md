# Unity Simple Sprite Animation Plugin
A plugin to do simple sprite animations avoiding the big and tedious Mechanim system. Oriented to programmers, if you prefer visual scripting you maybe prefer using Mechanim instead of this.

# Installation Guide
Simply copy the files on your project or use the UnityPackage included.

# Creating an animation
Use the animation editor to create new animation files. You can open it selecting **Sprite Animation Editor** on **Elendow Tools** tab.
- Give a name to the animation. This will be also the asset name. This name will be the one used to play the animations.
- Select the folder to save.
- Add frames manually or dropping the sprite to the Drag&Drop box.
- Any change is automatically saved.

# Inspector properties
- **Play on Awake** will start playing the **Start Animation** when the object awakes.
- **Start Animation** is the animation that will plays when **Play on Awake** is true.
- **FPS** is the speed of the animation, <= 0 will pause the animation.
- **Animations** is a list with all the animations.

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
