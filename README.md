# Autonomous Digital Terrarium 🌿

> *"Life, uh... finds a way."* (Even if that life is currently just a white pixel trying very hard not to starve.)

## What is this?
This is a zero-player simulation and digital petri dish built with C# and MonoGame. It's an autonomous ecosystem where primitive digital organisms are dropped onto a grid and forced to figure it out. 

There is no winning, no user-controlled protagonist, and no mercy. The system is governed by a strict energy economy and genetic mutation. My job is just to set the rules, turn the dials on the dev panel, and watch natural selection take the wheel. Sometimes they optimize perfectly; sometimes they form a hive-mind herd to strip-mine the local food supply; sometimes they just die. 

## Current State (The Primordial Soup)
Right now, the simulation is in its infancy. We have basic survival mechanics running:
* **The Economy:** Entities burn energy to move and replenish it by eating randomly regenerating food. Hit zero energy, and you're evicted from the grid.
* **Reproduction:** Eat enough, and an entity splits asexually, passing its traits (with a little random mutation) to the next generation.
* **The God Mode Panel:** A set of developer knobs to pause time, fast-forward, and tweak the laws of physics on the fly.

## The Roadmap (What's Next)
The goal is to introduce enough friction into the environment to force divergent evolution. Upcoming iterations will focus on:
* **The Food Chain:** Introducing predators, prey, and the desperate arms race of speed vs. efficiency.
* **Hostile Biomes:** Generating varied terrain (like energy-sapping mud or barren sand) to force geographic isolation and specialized adaptations.
* **Sensory Processing:** Giving entities a "vision" radius so they can hunt—but taxing their energy pool to power their new brains. 
* **Dynamic Visuals:** Evolving the graphics so an organism's genetic makeup (diet, speed, metabolism) natively dictates its shape and color. 

## Tech Stack
* **Language:** C#
* **Framework:** MonoGame (.NET Desktop GL)

---
*If you leave this running in the background for three hours and your CPU fan sounds like a jet engine, the simulation is working as intended.*