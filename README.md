# Traffic-Management-System
 Traffic Management System, made for Unity 3D modelled with C# Scripts and the instructions for application is given in the README file.
There are three C# files: 
 1. Traffic Light Manager
 2. Crosswalk controller
 3. Car controller

 -> From these, TrafficLightManager.cs is the main system in which there can be 4 systems in a junction, working synchronously and making the traffic flow easier.
 -> I have made the traffic lights in opposites to eachother i.e., EAST & WEST to work synchronously, become green and red at the same time as it wouldn't affect the traffic.
 -> CrosswalkController.cs and CarController.cs work in accordance with TrafficLiightManager.cs as it subscribes to it.

 **Set up in Unity 3D:**
 -> Set up the traffic light manager with 3 lights: Red, Green and Yellow.
 -> Set up 4 in a intersection (East, West, North and South).
 -> Attach the lights to the Traffic Light Manager property in accordance with directions.
