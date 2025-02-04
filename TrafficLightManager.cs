using System;
using System.Collections;
using UnityEngine;

public class TrafficLightManager : MonoBehaviour
{
    [System.Serializable]
    public class TrafficLight
    {
        public Light redLight;
        public Light yellowLight;
        public Light greenLight;
        public TrafficLightState currentState;
    }
    

    [System.Serializable]
    public class PedestrianLight
    {
        public Light stopLight;    // Pedestrian Red (Stop)
        public Light walkLight;    // Pedestrian Green (Walk)
    }

    public enum TrafficLightState { Red, Yellow, Green }
    public enum Direction { North, South, East, West }

    public TrafficLight[] trafficLights = new TrafficLight[4];  // Array of 4 traffic lights
    public PedestrianLight[] pedestrianLights = new PedestrianLight[8];  // Array of 8 pedestrian lights

    public float vehicleGreenDuration = 30f;
    public float vehicleYellowDuration = 5f;
    public float pedestrianGreenDuration = 25f;
    public float allRedDuration = 2f;

    // Define an event that cars can subscribe to
    public event Action<Direction, TrafficLightState> OnTrafficLightChanged;

    private void Start()
    {
        InitializeLights();
        StartCoroutine(IntersectionSequence());
    }

    private void InitializeLights()
    {
        for (int i = 0; i < trafficLights.Length; i++)
        {
            if (trafficLights[i] == null)
            {
                trafficLights[i] = new TrafficLight();
            }
            SetTrafficLightState((Direction)i, TrafficLightState.Red);
        }

        for (int i = 0; i < pedestrianLights.Length; i++)
        {
            if (pedestrianLights[i] == null)
            {
                pedestrianLights[i] = new PedestrianLight();
            }
            SetPedestrianLightState((Direction)(i / 2), false);
        }
    }

    private IEnumerator IntersectionSequence()
    {
        while (true)
        {
            // North-South Green, East-West Red
            SetTrafficLightState(Direction.North, TrafficLightState.Green);
            SetTrafficLightState(Direction.South, TrafficLightState.Green);
            SetTrafficLightState(Direction.East, TrafficLightState.Red);
            SetTrafficLightState(Direction.West, TrafficLightState.Red);
            SetPedestrianLightState(Direction.East, true);
            SetPedestrianLightState(Direction.West, true);
            yield return new WaitForSeconds(vehicleGreenDuration);

            // North-South Yellow
            SetTrafficLightState(Direction.North, TrafficLightState.Yellow);
            SetTrafficLightState(Direction.South, TrafficLightState.Yellow);
            SetPedestrianLightState(Direction.East, false);
            SetPedestrianLightState(Direction.West, false);
            yield return new WaitForSeconds(vehicleYellowDuration);

            // All Red
            SetAllTrafficLights(TrafficLightState.Red);
            yield return new WaitForSeconds(allRedDuration);

            // East-West Green, North-South Red
            SetTrafficLightState(Direction.East, TrafficLightState.Green);
            SetTrafficLightState(Direction.West, TrafficLightState.Green);
            SetTrafficLightState(Direction.North, TrafficLightState.Red);
            SetTrafficLightState(Direction.South, TrafficLightState.Red);
            SetPedestrianLightState(Direction.North, true);
            SetPedestrianLightState(Direction.South, true);
            yield return new WaitForSeconds(vehicleGreenDuration);

            // East-West Yellow
            SetTrafficLightState(Direction.East, TrafficLightState.Yellow);
            SetTrafficLightState(Direction.West, TrafficLightState.Yellow);
            SetPedestrianLightState(Direction.North, false);
            SetPedestrianLightState(Direction.South, false);
            yield return new WaitForSeconds(vehicleYellowDuration);

            // All Red
            SetAllTrafficLights(TrafficLightState.Red);
            yield return new WaitForSeconds(allRedDuration);
        }
    }

    private void SetTrafficLightState(Direction direction, TrafficLightState state)
    {
        TrafficLight light = trafficLights[(int)direction];
        if (light == null) return;

        // Turn off all lights first
        SetLightState(light.redLight, false);
        SetLightState(light.yellowLight, false);
        SetLightState(light.greenLight, false);

        // Turn on the appropriate light
        switch (state)
        {
            case TrafficLightState.Red:
                SetLightState(light.redLight, true);
                break;
            case TrafficLightState.Yellow:
                SetLightState(light.yellowLight, true);
                break;
            case TrafficLightState.Green:
                SetLightState(light.greenLight, true);
                break;
        }

        light.currentState = state;

        // Notify subscribers (like the CarController) about the traffic light state change
        OnTrafficLightChanged?.Invoke(direction, state);
    }

    private void SetPedestrianLightState(Direction direction, bool canWalk)
    {
        int index = (int)direction * 2;
        for (int i = 0; i < 2; i++)
        {
            PedestrianLight light = pedestrianLights[index + i];
            if (light == null) continue;

            SetLightState(light.stopLight, !canWalk);
            SetLightState(light.walkLight, canWalk);
        }
    }

    private void SetLightState(Light light, bool state)
    {
        if (light != null)
        {
            light.enabled = state;
        }
    }

    private void SetAllTrafficLights(TrafficLightState state)
    {
        foreach (Direction direction in System.Enum.GetValues(typeof(Direction)))
        {
            SetTrafficLightState(direction, state);
        }
    }

    public TrafficLightState GetTrafficLightState(Direction direction)
    {
        return trafficLights[(int)direction].currentState;
    }

    public bool IsPedestrianWalkSignalOn(Direction direction)
    {
        return pedestrianLights[(int)direction * 2].walkLight.enabled;
    }
}
