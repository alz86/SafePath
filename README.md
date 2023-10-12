

# SafePath - Making Every Step Count in Women's Safety

## Introduction

**SafePath** envisions a world where every path is truly safe. Rooted in the idea that no woman should feel unsafe walking on any street, we've developed a platform leveraging community collaboration and state-of-the-art technology. Going beyond traditional GPS systems, SafePath's app takes into account various safety factors like public lighting, commercial activity, crime rates, and community feedback. By doing so, it doesn't just find the fastest route but the safest one.

The strength of SafePath lies in its dual approach. It serves as a tool for the community to find and contribute to safer routes. At the same time, it empowers local governments to actively participate in public safety. By providing them with tools for confidential data upload and API integrations, we aim for a perpetually updated safety map that benefits everyone.

As an open-source initiative, SafePath is more than just a tool—it's a movement. We invite developers and contributors worldwide to join us in this journey, making safety not just a local concern but a global endeavor.

## Project Structure Overview

SafePath is built with a modular architecture, ensuring scalability and ease of understanding for developers and contributors. Here's a high-level overview of our project's main components and directories:

-   **`/src`**: The root directory housing all the source code of the project. Within this directory, you will find:
    
    -   **`/itinero`**: This folder contains a fork of the C# project [Itinero](https://github.com/itinero/routing). Itinero is a library geared towards route planning and processing based on OpenStreetMap data. We've opted to fork Itinero to introduce some custom modifications, specifically to integrate our unique safety score when determining routes.
        
    -   **`/server`**: This is where our server-side code resides, comprising:
        
        -   **API**: Providing core functionalities and serving data.
        -   **Website**: An administrative interface for the SafePath project.
    -   **`/mobile`**: Here lies our React Native mobile app, which we fondly refer to as the "reference app". It serves a dual purpose:
        
        -   To act as a guide showcasing the optimal way to interact with our API.
        -   To offer a ready-to-use application, which, if desired, can be rebranded and utilized directly.

Furthermore, it's worth noting that SafePath's foundation is laid upon the [ABP.IO](https://abp.io/) framework. ABP.IO provides a solid, modular structure, facilitating the rapid development of enterprise-level applications. For a deeper dive into its capabilities and functionalities, please refer to the official [ABP.IO documentation](https://docs.abp.io/).

## Installation

### Prerequisites

1.  **ASP.NET Core**: Ensure you have the ASP.NET Core runtime and SDK installed. Check the official [ASP.NET Core documentation](https://docs.microsoft.com/en-us/aspnet/core/) for installation guides.
2.  **React Native**: Before working with the mobile app, you'll need to have React Native set up on your machine. Follow the [React Native getting started guide](https://reactnative.dev/docs/getting-started) for installation and setup.
3.  **Node.js and npm**: React Native requires Node.js and npm. Ensure they are installed. If not, download and install them from [here](https://nodejs.org/).

### Setting Up the Project

1.  **Clone the Repository**:
    
    `git clone https://github.com/alz86/SafePath/safepath.git` 
    
2.  **Navigate to the Server Directory**:
    
    `cd safepath/src/server` 
    
3.  **Install Server Dependencies**:
    
    `dotnet restore` 
    
4.  **Navigate to the Mobile App Directory**:
    
    `cd ../mobile` 

5. **Install the project dependencies:**

	If you haven't installed Yarn, install it first:

	`npm install -g yarn` 

	Then install dependencies using Yarn

	`yarn install` 

6. **Setup Expo:**

	If you haven't installed Expo CLI, do so with:

	`npm install -g expo-cli` 

7. **Configure Environment Variables**:

	In the `mobile` directory, there's a file named `.env.template`. Copy this file and rename the copy to `.env`. You can then edit the `.env` file to adjust the configuration settings as needed:

	`cp .env.template .env` 

	Open `.env` in your preferred editor and set the configurations explained below.

## Configuring API Keys

### Google API Keys:

SafePath uses a component for place autocomplete utilizing Google Services.

1.  **Obtaining a Google API Key**:

    -   You will need a Google API key for this service. You can see the notes in the component's repo [here](https://github.com/FaridSafi/react-native-google-places-autocomplete#installation) (step **2**). 

2. **Configuring the Google API Key in SafePath**:

	-   Add the Google API key to `/src/mobile/.env`.
>In case you want to contribute and have trouble obtaining the Google API Key contact us so we can share our own with you.

## Running the Projects

### ASP.NET Core Server:

>Note: Running the `dotnet run` command will occupy the terminal to display logs and messages from the running application. To run both the API and the backend website simultaneously, open a new terminal window or tab for each or append the character `&` at the end of the command to run the projects in background.

1.  **Navigate to the API Directory**:
    
    `cd safepath/src/server/src/SafePath.HttpApi.Host` 
    
2.  **Run the API**:
    
    `dotnet run` 
    
    This will start the API service. By default, it should be accessible at `https://localhost:44385`. 
    
3.  **Navigate to the Backend Website Directory**:
        
    `cd ../SafePath.Blazor` 
    
4.  **Run the Backend Website**:
    
    `dotnet run` 
    
    This will start the backend website at `https://localhost:44359`. 
    

### React Native Mobile App (with Expo):

1.  **Navigate to the Mobile App Directory**:
    
    `cd ../../../mobile` 
    
2.  **Start the Mobile App with Expo**:
    
    `expo start` 
    
    A new browser window will open displaying a QR code. Use the Expo Go app on your Android or iOS device to scan the QR code and view the app.

## Usage

### Using the Admin Website:

Upon first accessing the admin website, you can log in using the default credentials:

**Username**: `admin`  
**Password**: `1q2w3E*`

> **Security Warning**: These are default credentials. For security reasons, we strongly recommend changing the password (and username, if possible) once you've accessed the admin interface, especially if deploying in a production environment.

## Contributing

Contributions to SafePath are more than welcome! Whether it's feature enhancements, bug fixes, or documentation improvements, your input is valuable.

1.  **Fork and Clone**: Start by forking this repository. Once done, clone your forked repository to your local machine.
    
    `git clone https://github.com/<YourUsername>/SafePath.git` 
    
2.  **Branch**: Create a new branch for your contribution.
    
    `git checkout -b <branch-name>` 
    
3.  **Commit and Push**: Make your changes and commit them. Push them to your forked repository.
    
4.  **Pull Request**: Open a pull request to the `master` branch of this repository. Please ensure your pull request describes what you changed and references any related issues.

## Credits

SafePath was conceived and developed by [alz86](https://github.com/alz86). 

The initial inspiration for this project was drawn from the challenge issued by the United Nations Office (UNO) and the European Union (UE) called the [Open Source Software for SDG - OSS4SDG](https://ideas.unite.un.org/sdg5/Page/Overview), focusing on SDG #5 (Gender Equality). This challenge aspired to foster open-source solutions addressing the Sustainable Development Goals (SDGs).

SafePath is our response to [Challenge #4](https://ideas.unite.un.org/sdg5/Page/challenge4) of the competition, aimed at creating a collaborative walking-safety map for women.

### Special Acknowledgments:

-   **Design Contributions**: Gratitude is extended to Fernando Debernardi, whose design expertise significantly shaped the visual identity of SafePath. Discover more of his  designs on [his webpage](https://www.movapps.com.ar/).
    
-   **Itinero**: This project utilizes a fork of the C# project [Itinero](https://github.com/itinero/routing) — an excellent routing engine tailored for OpenStreetMap data. Our modifications enable the integration of our unique safety score when determining routes.
    
-   **ABP.IO**: The foundational architecture of SafePath is constructed upon the [ABP.IO](https://abp.io/) framework. This framework provides a comprehensive, modular structure, aiding in the swift development of enterprise-level applications.
