# Unity Simple Sprite Animation Plugin
A plugin to do simple sprite animations avoiding the big and tedious Unitys's Mecanim system. Oriented to programmers, if you prefer visual scripting you maybe prefer using Mecanim instead of this.

# Installation Guide
Simply copy the files on your project or use the UnityPackage included.

# Creating an animation
Use the animation editor to create new animation files. You can open it selecting **Sprite Animation Editor** on **Elendow Tools** tab.
- Give a name to the animation. This will be also the asset name. This name will be the one used to play the animations.
- Select the folder to save.
- Add frames manually or dropping the sprite to the Drag&Drop box.
  - You can change the duration of each frame, 1 by default, to any number greater than 0. 
- Any change is automatically saved.

# Inspector properties
- **Play on Awake** will start playing the **Start Animation** when the object awakes.
- **Start Animation** is the animation that will plays when **Play on Awake** is true.
- **FPS** is the speed of the animation, <= 0 will pause the animation.
- **Animations** is a list with all the animations.

# Using the animations
Add the spriteAnimator or UIAnimator component to the object you want to animate and fill the animations list with the animations you want. 
This component requires a SpriteRenderer or Image component to work. If the object doesn't have one, the animator will add it automatically.
On your code, use **GetComponent** to get the reference and start using it.

# Methods
- **Play(string animationName, bool oneShot = false, bool backwards = false)** plays the animation infinite times if oneShot = false, only one time if true, fordward if backwards = false and backwards if its true.
  * If the animation is the same that is playing, nothing will happend but the **oneShot** attribute will update.
  * If the animation is the same that was playing but its not playing now, the animation will **Reset** and **Resume** and the **oneShot** attribute will update.
  * If the animation is different, it will play the new animation from the start.
  * If the animation name is "" will play the first on the animation list.
- **PlayRandom(bool playOneShot = false)** plays a random animation from the animation list.
- **Resume()** resumes the current animation.
- **Stop()** stops the current animation.
- **Reset()** restarts the animation (playing or not) to its initial state. If the animation is not playing, the restart will be applied only when it start playing again.
- **SetActiveRenderer(bool active)** enable/disables the renderer.
- **FlipSpriteX(bool flip)** flips the sprite on the X axis.

# Properties
- **IsPlaying {get;}** returns true if the animation is playing and false if not.
- **CurrentAnimation {get;}** returns a string with the current animation name.
- **DisableRenderOnFinish {set;}** sets the disableRenderer attribute. This will disable the renderer when the animation ends.

# Events
You can suscribe to the animation events using the AddListener(Listener) method of the UnityEvent class.
- **onFinish** calls when the animation reach the last frame.
- **onPlay** calls when the animation starts playing.
- **onStop** calls when the animation is forced to stop.
- You can add an event to a specific frame of the animation using the method **AddCustomEvent(int frame)**.
  - Ex: animation.AddCustomEvent(3).AddListener(StepFrame). Now on the frame 3 of the animation the method StepFrame will be called.
  - **Note** this system is provisional.
# License
MIT
