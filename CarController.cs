using UnityEngine;
using System.Collections;

public class CarController : MonoBehaviour
{
    public float maxSpeed = 5f;
    public float currentSpeed = 0f;
    public float accelerationRate = 2f;
    public float decelerationRate = 4f;
    public TrafficLightManager trafficLightManager;
    public TrafficLightManager.Direction travelDirection;
    public float stoppingDistance = 2f;
    public LayerMask carLayer;
    public float carLength = 4f;
    public float minDistanceBetweenCars = 0.5f;

    private enum ZoneType { None, Approach, Crosswalk, Intersection, Exit }
    private ZoneType currentZone = ZoneType.None;
    private bool isMoving = true;
    private bool isStopping = false;
    private bool isWaitingForLight = false;
    private bool isInQueue = false;

    private void Start()
    {
        if (trafficLightManager != null)
        {
            trafficLightManager.OnTrafficLightChanged += HandleTrafficLightChanged;
        }
    }

    private void Update()
    {
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        float distanceToCarInFront = GetDistanceToCarInFront();
        bool canMove = CanMove(distanceToCarInFront);

        if (canMove && !isStopping)
        {
            currentSpeed = Mathf.Min(currentSpeed + accelerationRate * Time.deltaTime, maxSpeed);
        }
        else
        {
            float targetSpeed = 0;
            if (!isWaitingForLight && distanceToCarInFront > minDistanceBetweenCars)
            {
                targetSpeed = Mathf.Max(0, (distanceToCarInFront - minDistanceBetweenCars) / stoppingDistance * maxSpeed);
            }
            currentSpeed = Mathf.Max(currentSpeed - decelerationRate * Time.deltaTime, targetSpeed);
        }

        float movement = currentSpeed * Time.deltaTime;
        if (!WillCollideWithCarInFront(movement))
        {
            transform.Translate(Vector3.forward * movement);
            if (currentSpeed > 0)
            {
                isMoving = true;
                isStopping = false;
            }
        }
        else
        {
            currentSpeed = 0;
            isMoving = false;
            isStopping = false;
        }

        // Check if we're in a queue
        isInQueue = IsInQueue();
    }

    private bool CanMove(float distanceToCarInFront)
    {
        if (isWaitingForLight && currentZone == ZoneType.Approach)
        {
            return false;
        }

        if (distanceToCarInFront <= minDistanceBetweenCars)
        {
            return false;
        }

        return true;
    }

    private float GetDistanceToCarInFront()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        float rayLength = 100f;

        if (Physics.Raycast(rayOrigin, transform.forward, out hit, rayLength, carLayer))
        {
            return hit.distance - carLength / 2;
        }

        return float.MaxValue;
    }

    private bool WillCollideWithCarInFront(float movement)
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        float rayLength = movement + minDistanceBetweenCars + carLength / 2;

        return Physics.Raycast(rayOrigin, transform.forward, out hit, rayLength, carLayer);
    }

    private bool IsInQueue()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        float rayLength = stoppingDistance * 2;

        return Physics.Raycast(rayOrigin, transform.forward, out hit, rayLength, carLayer);
    }

    private void StopCar()
    {
        isStopping = true;
        isMoving = false;
        Debug.Log($"Car stopping at {transform.position}");
    }

    private void StartCar()
    {
        isMoving = true;
        isStopping = false;
        isWaitingForLight = false;
        Debug.Log("Car started moving");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ApproachZone"))
        {
            currentZone = ZoneType.Approach;
            Debug.Log("Entered Approach Zone");
            CheckTrafficLightAndRespond();
        }
        else if (other.CompareTag("CrosswalkZone"))
        {
            currentZone = ZoneType.Crosswalk;
            Debug.Log("Entered Crosswalk Zone");
        }
        else if (other.CompareTag("IntersectionZone"))
        {
            currentZone = ZoneType.Intersection;
            Debug.Log("Entered Intersection Zone");
        }
        else if (other.CompareTag("ExitZone"))
        {
            currentZone = ZoneType.Exit;
            Debug.Log("Entered Exit Zone");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ApproachZone"))
        {
            Debug.Log("Exited Approach Zone");
            isWaitingForLight = false;
        }
    }

    private void HandleTrafficLightChanged(TrafficLightManager.Direction direction, TrafficLightManager.TrafficLightState newState)
    {
        if (direction == travelDirection)
        {
            CheckTrafficLightAndRespond();
        }
    }

    private void CheckTrafficLightAndRespond()
    {
        TrafficLightManager.TrafficLightState lightState = trafficLightManager.GetTrafficLightState(travelDirection);

        if (lightState == TrafficLightManager.TrafficLightState.Red || lightState == TrafficLightManager.TrafficLightState.Yellow)
        {
            if (currentZone == ZoneType.Approach || isInQueue)
            {
                StopCar();
                isWaitingForLight = true;
            }
        }
        else
        {
            StartCar();
        }
    }

    private void OnDestroy()
    {
        if (trafficLightManager != null)
        {
            trafficLightManager.OnTrafficLightChanged -= HandleTrafficLightChanged;
        }
    }
}