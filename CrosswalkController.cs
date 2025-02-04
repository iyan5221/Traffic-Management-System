using UnityEngine;
using UnityEngine.AI;

public class CrosswalkController : MonoBehaviour
{
    public Collider[] carColliders; // Array for multiple cars
    public Collider[] crossroadColliders; // Array for multiple crosswalks
    public Transform characterTransform;
    public float safeDistance = 10f;
    public float characterMoveThreshold = 5f;
    public float walkAfterCrossingDistance = 10f; // Distance to walk after crossing

    private NavMeshAgent characterAgent;
    private Animator characterAnimator;
    private bool isSafeToCross = false;
    private bool isCrossing = false;
    private Vector3 targetCrossroad;
    private bool hasCrossed = false;

    void Start()
    {
        characterAgent = characterTransform.GetComponent<NavMeshAgent>();
        characterAnimator = characterTransform.GetComponent<Animator>();

        // Ensure the character doesn't move at the start
        characterAgent.isStopped = true;
        characterAnimator.SetBool("IsWalking", false);
        characterAnimator.SetBool("IsRunning", false);
    }

    void Update()
    {
        if (!hasCrossed)
        {
            // Check if it's safe to cross at any crossroad
            foreach (var crossroad in crossroadColliders)
            {
                float minDistanceToCars = float.MaxValue;

                foreach (var car in carColliders)
                {
                    float distanceCarToCrossroad = Vector3.Distance(car.bounds.center, crossroad.bounds.center);
                    minDistanceToCars = Mathf.Min(minDistanceToCars, distanceCarToCrossroad);
                }

                // If safe distance is satisfied for all cars
                if (minDistanceToCars > safeDistance && !isCrossing)
                {
                    isSafeToCross = true;
                    targetCrossroad = crossroad.bounds.center;
                    Debug.Log("Safe to cross!");
                }
            }

            // Start crossing if safe
            if (isSafeToCross && !isCrossing)
            {
                isCrossing = true;
                characterAgent.isStopped = false;
                characterAnimator.SetBool("IsWalking", true);
                characterAgent.SetDestination(targetCrossroad);
            }

            // Check if the character is already crossing
            if (isCrossing)
            {
                // Check for danger again while crossing
                foreach (var car in carColliders)
                {
                    float distanceCarToCharacter = Vector3.Distance(car.bounds.center, characterTransform.position);

                    if (distanceCarToCharacter <= safeDistance)
                    {
                        // Danger! Start running animation
                        characterAnimator.SetBool("IsWalking", false); // Stop walking animation
                        characterAnimator.SetBool("IsRunning", true);  // Start running animation
                        Debug.Log("Danger! Running to escape!");
                    }
                }

                // Stop running and switch to walking after crossing
                if (Vector3.Distance(characterTransform.position, targetCrossroad) <= 1f)
                {
                    isCrossing = false;
                    isSafeToCross = false;
                    hasCrossed = true;
                    characterAgent.isStopped = true;
                    characterAnimator.SetBool("IsWalking", false);
                    characterAnimator.SetBool("IsRunning", false);
                    Debug.Log("Crossing complete.");

                    // Continue walking after crossing
                    WalkAfterCrossing();
                }
            }
        }
    }

    // Method to continue walking after crossing the road
    void WalkAfterCrossing()
    {
        // Reset the NavMeshAgent destination to avoid being stuck
        characterAgent.ResetPath();

        // Set the character to walk forward after crossing the road
        characterAnimator.SetBool("IsWalking", true); // Start walking again

        // Move forward based on character's current facing direction
        Vector3 walkAfterDestination = characterTransform.position + characterTransform.forward * walkAfterCrossingDistance;
        characterAgent.SetDestination(walkAfterDestination);

        // Unstop the agent to let it move
        characterAgent.isStopped = false;
    }

    void OnDrawGizmos()
    {
        if (carColliders != null && crossroadColliders != null)
        {
            Gizmos.color = Color.red;
            foreach (var car in carColliders)
            {
                foreach (var crossroad in crossroadColliders)
                {
                    Gizmos.DrawLine(car.bounds.center, crossroad.bounds.center);
                }
            }

            Gizmos.color = Color.green;
            foreach (var crossroad in crossroadColliders)
            {
                Gizmos.DrawWireSphere(crossroad.bounds.center, safeDistance);
            }
        }
    }
}
