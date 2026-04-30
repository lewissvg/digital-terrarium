# Project Blueprint: Autonomous Digital Terrarium

## Project Description
A zero-player, continuously running digital ecosystem. The application generates a procedural tile-based terrarium where primitive digital organisms compete for resources. Governed by a strict energy economy and genetic mutation, the organisms adapt over time through natural selection. The core experience is observing the evolutionary drift and tinkering with the underlying mathematical rules of the environment.

## Tech Stack
* **Language:** C#
* **Framework:** MonoGame (.NET Desktop GL)
* **Architecture Concept:** Game Loop / Simple Entity Management
* **UI:** Custom code-centric UI (or optional NuGet packages like Myra/GeonBit.UI)

---

## Version 1.0 Requirements (MVP)

### 1. The Environment (Petri Dish)
* **The Grid:** A 2D tilemap representing the world boundaries.
* **Procedural Spawning:** A baseline noise function or random scatter to distribute initial 'Food' (e.g., green tiles/pixels).
* **The Global Clock:** A tick-based system allowing Food to naturally regenerate on empty tiles over time.

### 2. The Organisms (Entities)
* **Visuals:** Primitive 2D shapes drawn via MonoGame's `SpriteBatch` (e.g., circles or squares).
* **AI State Machine:** * *Wander:* Move randomly to search for food.
  * *Target:* Move directly toward detected food in range.
  * *Rest:* Halt movement to conserve energy.
* **Energy Economy:** Entities possess a finite energy pool. Moving drains energy ($E_{drain} = v^2 \times m$). Eating replenishes it. Hitting 0 energy results in death/removal.

### 3. The Genetic Engine (Evolution)
* **Core Traits:** Speed vs. Metabolism.
* **Reproduction:** Asexual splitting when an entity reaches maximum energy capacity.
* **Mutation:** Offspring inherit parent traits with a ±5% random drift.

### 4. The Observer UI
* **Time Controls:** Play, Pause, and Fast-Forward (simulation tick multiplier).
* **Data Dashboard:** Minimal text overlay tracking active population, the oldest living entity, and the global average for the 'Speed' trait.

---

## Environment Setup & Scaffolding

### 1. Install Prerequisites
Ensure you have the latest [.NET SDK](https://dotnet.microsoft.com/download) installed. 

### 2. Install MonoGame Templates
Open your terminal and install the official MonoGame project templates globally:
`dotnet new install MonoGame.Templates.CSharp`

### 3. Scaffold the Project
Navigate to your desired workspace directory and run the following commands to generate a cross-platform Desktop OpenGL project:

```bash
# Create the new project
dotnet new mgdesktopgl -n DigitalTerrarium

# Navigate into the project folder
cd DigitalTerrarium

# Create the foundational directory structure
mkdir Entities Systems UI Core