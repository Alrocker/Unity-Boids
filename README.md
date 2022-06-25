# Unity-Boids

This is a brief showing of my work on the Boid's algorithm. To showcase my work, I have included here 3 videos showing my overall progress on my work:

# Earliest-Implementation
https://user-images.githubusercontent.com/34142204/174691318-da449989-c1f9-4f3a-b699-3197dd97a52c.mp4

This is my earliest implementation of boids, and is not included in this GitHub. It enforces the following behaviors:
- Enforcing the fundamental laws of boids that are: Separation, Cohesion, and Alignment
- Confine boids to a spherical zone they won't leave from
- Target and move towards another gameobject

# Without-Physics
[![Unity-Boids](https://img.youtube.com/vi/kTq0YDk32r8/0.jpg)](https://www.youtube.com/watch?v=kTq0YDk32r8)

This is the previous stage my Boids algorithm was at. It enforces the following behaviors:
- Everything from the previous "Earliest Implementation" video
- Boids can belong to different weight sets: the red boid targets, while the purple boids group up, roam randomly, and avoid the red boid
- Minimal collider detection of non-boid targets

# With-Physics
https://user-images.githubusercontent.com/34142204/175785154-a775ba3b-2fb1-4b07-8621-6dbc354545e4.mp4

My previous boid implementations controlled position. This iteration changes the outputs from position data to force data, allowing Unity's Physics system to do the heavy lifting. It enforces the following behavior:
- Everything from the previous "Earliest Implementation" and "Without Physics" videos
- Transitioning to a more open environment, including a floor, trees, buildings, etc.
- More advanced (but not finished) collider detection to avoid targets, other boids, and a general environment. This is my next goal
- Boids not only have different weight sets, but can change state to swap among these established weight sets. I hope to fine-tune this along with collision avoidance in   my next update.
  - The red boid targets a set object and ignores surrounding boids, and is in the "enemy" faction
  - Blue boids are in the "ally" faction, and free roam until approaching the enemy red boid. Upon doing so, blue boids become...
  - Yellow boids are in the "ally" faction, and actively avoid the enemy red boid. If they contact wandering blue boids, the blue boids are alerted of the red boid's   presence and become...
  - Green boids are in the "ally" faction, and try to target and ram the red boid. They will group with other green boids in this task.
