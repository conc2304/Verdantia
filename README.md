# GreenCityBuilder Project

## Overview

**GreenCityBuilder** is an urban simulation game aimed at educating players about environmental impact, city planning, and sustainability. The game enables users to create and manage cities, balancing various metrics such as pollution, energy consumption, happiness, and urban heat. Players are tasked with designing and developing urban spaces while considering factors like green spaces, energy usage, and population growth.

This project integrates various gameplay mechanics, such as mission-based objectives, city-building, and dynamic environmental systems. Additionally, the project utilizes interactive elements and multiple display systems to enhance the user experience.

## Features

- **City Building & Management**: Players can build and manage their city infrastructure, focusing on sustainable growth, green spaces, and balancing environmental factors.
- **Mission System**: The game includes a variety of missions that provide objectives related to urban reforestation, pollution control, energy management, and more. Missions are dynamically tracked, and player progress is evaluated based on success criteria like budget, population, and sustainability.
- **Metrics & Data Visualization**: The game tracks key metrics such as city temperature, pollution levels, carbon emissions, and happiness. These metrics are presented to players in real-time to help them make informed decisions about their city's development.
- **Weather & Environmental Effects**: The game includes dynamic weather systems like winter weather, influencing gameplay elements such as temperature and snow coverage.
- **Multi-Display Support**: The project supports multiple displays for an immersive experience, including dedicated displays for city maps, metrics, and player interactions.
- **User Interface**: The UI provides detailed statistics and key metrics in an easy-to-read format. It also supports interactive elements like sliders, buttons, and touch controls for managing the game’s progression.
- **Dynamic Building and Road System**: The game includes an in-depth system for placing and connecting buildings and roads. Players can demolish and build structures dynamically within the city grid.
- **Save & Load System**: The game allows players to save their progress, with the ability to load saved cities for continued play. The system supports city-specific data storage, including building positions, rotations, and associated properties.

## Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/yourusername/GreenCityBuilder.git
   cd GreenCityBuilder


2. **Install dependencies**: Make sure you have Unity installed (recommended version: Unity 2021.3 or later). Open the project using the Unity Hub.

3. **Open the project**:
- Open Unity and load the project.
- Make sure all the required packages and dependencies are resolved automatically by Unity.
4. **Running the project**:
- You can test the game by hitting the play button in the Unity editor. The game will automatically start with a test city.

## Usage

### Gameplay Mechanics
- **Building Cities**: Start by creating a city using the provided building and road types. You will need to manage city growth, resources, and environmental factors like pollution and energy consumption.
- **Missions**: Complete various missions that challenge you to improve specific metrics such as reducing pollution, boosting population happiness, or balancing the city’s budget.
- **Metrics & Feedback**: Keep an eye on key metrics like city temperature, happiness, and pollution levels. These will help you make decisions about where to place buildings, roads, and green spaces.

### Controls
- **Touchscreen Kiosk**: Use the interactive UI elements to manage the city. You can adjust sliders, select buildings to place, and monitor your city’s progress.

### Data Saving and Loading
- **Save Progress**: The game automatically saves data related to your city’s state, including buildings and roads, allowing you to pick up where you left off.
- **Load Saved Data**: You can load saved cities from previous sessions and continue building upon them.

## Code/Script Structure

### Key Components
- **City Management**: Includes classes like CityMetricsManager, MissionManager, and Mission to handle city data and mission tracking.
- **Building and Road Systems**: The RoadGenerator and BuildingData classes manage city layout, allowing for dynamic road creation and modification.
- **UI Elements**: Various UI classes like LineChartRenderer, MissionCatalog, and MetricDisplay provide data visualization and user interaction elements.
- **Save/Load System**: SaveDataTrigger and SaveSystem classes manage the saving and loading of city data, allowing for persistent game progress.

### Enums 
- **TrackpadTargetType**: Specifies the possible actions the user can take (Building or Demolishing).
- **MetricTitle**: Defines the key metrics tracked in the game (e.g., Population, Budget, Pollution).
- **DifficultyLevel**: Represents the difficulty of missions (Easy, Medium, Hard, etc.).

### Utilities
- **NumbersUtils**: Provides helper methods for working with numbers, including remapping values, formatting, and rounding.
- **StringsUtils**: Handles string conversion between camel case and label format, along with other string utility functions.

## Credits

This project uses the following vendor packages from the Unity Asset Store:

- **[CityEngine](https://assetstore.unity.com/packages/templates/systems/city-engine-174406)**: A package/tool for bootstrapping city building games with ease. Complete with building prefabs, camera controls, grid layout, saving and loading, and population and traffic simulation.
- **[Stylized Solarpunk City](https://assetstore.unity.com/packages/3d/environments/sci-fi/stylized-solarpunk-city-267031)**: A set of building blocks to create solarpunk city environment, perfect for futuristic urban settings.
- **[Simple Button Set 01](https://assetstore.unity.com/packages/2d/gui/icons/simple-button-set-01-153979)**: A collection of simple, customizable buttons for UI design, used throughout the project for interactive elements.
- **[Joystick Pack](https://assetstore.unity.com/packages/tools/input-management/joystick-pack-107631)**: A comprehensive joystick input management system for enhanced user controls and gameplay interaction. Used for camera navigation from the touch kiosk.

I would like to thank the developers of these packages for providing these tools and resources.