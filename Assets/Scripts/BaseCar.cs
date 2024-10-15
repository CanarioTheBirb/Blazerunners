using GameKit.Utilities.Types;
using IO.Swagger.Model;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.Assertions.Comparers;
using UnityEngine.InputSystem;

public class BaseCar : MonoBehaviour
{
    [Header("----- Gameplay -----")]
    public float hp = 100.0f;
    public float itemRange;
    public int position;
    public Transform itemPlace;
    public GameObject item;
    public GameObject aim;
    public bool aimAssist = true;
    public bool targetLocked { get; private set; }

    [Header("---- References ----")]
    [SerializeField] private LayerMask drivableLayer;
    [SerializeField] private Transform accelerationPoint;
    public Vector3 NearestCar = Vector3.zero;
    public int[] wheelsGrounded = new int[4];
    public bool isGrounded = false;
    public bool isWheeling = false;
    public bool isBoosting = false;

    [Header("----- Suspension Settings ------")]
    [SerializeField] private float sprintStiffness;
    [SerializeField] private float damperStiffnes;
    [SerializeField] private float restingLength;
    [SerializeField] private float springTravel;
    [SerializeField] private float wheelRadius;

    [Header("----- Car Settings -----")]
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float maxSpeed = 50f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float steeringStrength = 15f;
    [SerializeField] private float dragCoefficient = 1f;
    [SerializeField] private AnimationCurve turnCurve;

    [Header("----- Visuals -----")]
    [SerializeField] private float tyreRotationSpeed = 100f;
    [SerializeField] private float rotationValue = 30f;
    [SerializeField] private float rotationSpeed = 0.15f;
    [SerializeField] private float maxTurnAngle = 45f;
    [SerializeField] private ParticleSystem[] boostTrail;

    [Header("----- Skid Marks -----")]
    [SerializeField] private TrailRenderer[] skidMarks;
    [SerializeField] private float skidWidth = 0.052f;
    [SerializeField] private float minSkidVelocity = 10f;
    [SerializeField] private ParticleSystem[] driftSmoke;

    [Header("---- Raypoints/Wheels ----")]
    [SerializeField] private Transform[] wheelRayPoints;
    [SerializeField] private Transform[] wheelMeshes;
    [SerializeField] private GameObject[] frontWheels = new GameObject[2];

    [Header("---- Car Parts ----")]
    [SerializeField] private Rigidbody carRB;
    [SerializeField] public SphereCollider sight;
    [SerializeField] public MeshRenderer Body;
    [SerializeField] public List<Material> texture;

    private float oldSteering;
    private bool isDrifting;

    private Vector3 carVelocity = Vector3.zero;
    public float carVelocityRatio = 0;


    //EXTERNAL VALUES TO CHANGE HERE
    public float moveInput { get; set; }
    public float steerInput { get; set; }
    public float driftInput { get; set; }

    #region Internal Functions
    private void FixedUpdate()
    {
        CheckIfOnGround();
        Drive();
        Suspension();
        CalculateVelocity();

        //Visuals
        SkidMarks();
        UpdateTires();
        DriftSmoke();
        BoostTrail();

        FindEnemy();
    }

    private void Start()
    {
        foreach (TrailRenderer skid in skidMarks)
        {
            skid.startWidth = skidWidth;
            skid.emitting = false;
        }

        NearestCar = carRB.transform.forward;
    }

    private void Suspension()
    {
        for (int i = 0; i < wheelRayPoints.Length; i++)
        {
            RaycastHit hit;
            float maxLen = restingLength + springTravel;

            if (Physics.Raycast(wheelRayPoints[i].position, -wheelRayPoints[i].up, out hit, maxLen + wheelRadius, drivableLayer))
            {
                wheelsGrounded[i] = 1;

                float currSpringLen = hit.distance - wheelRadius;
                float springCompression = (restingLength - currSpringLen) / springTravel;

                float springSpeed = Vector3.Dot(carRB.GetPointVelocity(wheelRayPoints[i].position), wheelRayPoints[i].up);

                float dampeningFeedback = damperStiffnes * springSpeed;
                float springFeedback = sprintStiffness * springCompression;

                float force = springFeedback - dampeningFeedback;


                carRB.AddForceAtPosition(force * wheelRayPoints[i].up, wheelRayPoints[i].position);

                //Set wheels to be on the spring
                wheelMeshes[i].position = wheelRayPoints[i].position + (currSpringLen - 0.02f) * -wheelRayPoints[i].up;

                Debug.DrawLine(wheelRayPoints[i].position, hit.point, Color.red);
            }
            else
            {
                wheelsGrounded[i] = 0;

                wheelMeshes[i].position = wheelRayPoints[i].position + (maxLen - 0.2f) * -wheelRayPoints[i].up;
                Debug.DrawLine(wheelRayPoints[i].position, wheelRayPoints[i].position + (wheelRadius + maxLen) * -wheelRayPoints[i].up, Color.yellow);
            }
        }
    }

    #region Visual Updates

    private void DriftSmoke()
    {
        for (int i = 0; i < skidMarks.Length; i++)
        {
            if (skidMarks[i].emitting)
            {
                driftSmoke[i].Emit(1);
            }
        }
    }

    private void SkidMarks()
    {
        if (Mathf.Abs(carVelocity.x) > minSkidVelocity && isDrifting && isGrounded)
        {
            foreach (TrailRenderer skid in skidMarks)
            {
                skid.emitting = true;
            }
        }
        else
        {
            foreach (TrailRenderer skid in skidMarks)
            {
                skid.emitting = false;
            }
        }
    }

    private void UpdateTires()
    {

        float steerAngle = maxTurnAngle * steerInput;

        for (int i = 0; i < wheelMeshes.Length; i++)
        {
            wheelMeshes[i].transform.Rotate(Vector3.right, Time.deltaTime * tyreRotationSpeed * carVelocity.z, Space.Self);

            if (i < 2)
            {
                frontWheels[i].transform.localEulerAngles = new Vector3(frontWheels[i].transform.localEulerAngles.x, steerAngle, frontWheels[i].transform.localEulerAngles.z);
            }


        }
        //Turning


    }

    void BoostTrail()
    {
        if (isBoosting)
        {
            carRB.AddForce(carRB.transform.forward * 100, ForceMode.Acceleration);
            foreach (var trail in boostTrail)
            {
                trail.Emit(1);
            }
        }
    }
    #endregion

    #region Car Checks
    private void CheckIfOnGround()
    {
        int groundWheels = 0;

        for (int i = 0; i < wheelsGrounded.Length; i++)
        {
            groundWheels += wheelsGrounded[i];
        }

        if (groundWheels == 2)
        {
            isGrounded = true;
            isWheeling = true;
        }
        else if (groundWheels > 1)
        {
            isGrounded = true;
            isWheeling = false;
        }
        else
        {
            isGrounded = false;
            isWheeling = false;
        }

    }

    private void CalculateVelocity()
    {
        carVelocity = carRB.transform.InverseTransformDirection(carRB.velocity);
        carVelocityRatio = carVelocity.z / maxSpeed;
    }

    void FindEnemy()
    {
        if (!aimAssist || item == null)
        {
            return;
        }

        item.GetComponent<ItemManager>().crosshair = false;

        //Look for cars
        Collider[] sphere = Physics.OverlapSphere(carRB.transform.position, itemRange);
        List<BaseCar> cars = new List<BaseCar>();

        for (int i = 0; i < sphere.Length; i++)
        {
            if (sphere[i].gameObject.GetComponentInParent<BaseCar>() != null)
            {
                if (sphere[i].gameObject.GetComponentInParent<BaseCar>() != this.GetComponent<BaseCar>())
                {
                    cars.Add(sphere[i].gameObject.GetComponentInParent<BaseCar>());
                }
            }
        }

        //no cars then straight
        if (cars.Count == 0)
        {
            NearestCar = carRB.transform.forward;
            targetLocked = false;
            item.transform.LookAt((item.transform.position + NearestCar) + new Vector3(0, -0.03f, 0));
            return;
        }

        //Looks for the closest car
        float closestDist = Mathf.Infinity;
        BaseCar closestCar = null;
        foreach (BaseCar car in cars)
        {
            Rigidbody rb = car.gameObject.GetComponentInChildren<Rigidbody>(); // grabs the position of the object moving around from this Rigidbody
            Transform target = rb.transform;
            float distance = Vector3.Distance(item.transform.position, target.position);
            if (distance < closestDist)
            {
                closestDist = distance;
                closestCar = car;
            }
        }

        NearestCar = PredictFuturePosition(closestCar);
        targetLocked = true;
        item.transform.LookAt(NearestCar);

        RaycastHit[] hits = Physics.RaycastAll(item.GetComponent<ItemManager>().shootPos.position, item.GetComponent<ItemManager>().shootPos.forward.normalized, Vector3.Distance(carRB.position, NearestCar));
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject.GetComponentInParent<Rigidbody>() == carRB)
            {
                item.transform.LookAt(closestCar.gameObject.GetComponentInChildren<Rigidbody>().position + new Vector3(0, -0.03f, 0));
                Debug.DrawLine(item.GetComponent<ItemManager>().shootPos.position, closestCar.GetComponentInChildren<Rigidbody>().position, Color.red);
            }
        }
        Debug.DrawLine(item.GetComponent<ItemManager>().shootPos.position, NearestCar, Color.red);
    }

    //helper function to give me a direction of the cars future position
    Vector3 PredictFuturePosition(BaseCar bCar)
    {
        Rigidbody closestRB = bCar.gameObject.GetComponentInChildren<Rigidbody>();
        Vector3 resultsPos = closestRB.position;
        //reset weapon so we can get accurate angles
        item.transform.LookAt(item.transform.position + carRB.transform.forward);
        ItemManager itemMan = item.GetComponent<ItemManager>();

        float dist = Vector3.Distance(item.transform.position, closestRB.position);
        float time = dist / (itemMan.bulletSpeed);
        resultsPos = closestRB.position + closestRB.velocity * closestRB.velocity.magnitude * time;
        resultsPos += new Vector3(0,-0.03f,0);
        return resultsPos;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(carRB.transform.position, itemRange);
    }
    #endregion

    #region Driving

    private void Drive()
    {
        Turn();

        if (isGrounded)
        {
            Acceleration();
            Deceleration();
            DragSideways();
            Drift();
        }
    }

    private void Acceleration()
    {
        if (carVelocityRatio < 1 && moveInput > 0)
        {
            if (!isWheeling)
            {
                carRB.AddForceAtPosition(acceleration * moveInput * carRB.transform.forward, accelerationPoint.position, ForceMode.Acceleration);
            }
            else
            {
                carRB.AddForceAtPosition(acceleration * moveInput * -carRB.transform.up, carRB.position, ForceMode.Acceleration);
            }
        }
    }

    private void Deceleration()
    {
        if (carVelocityRatio > -0.5 && moveInput < 0)
        {
            carRB.AddForceAtPosition(deceleration * moveInput * carRB.transform.forward, accelerationPoint.position, ForceMode.Acceleration);
        }
    }

    private void Drift()
    {
        if (driftInput == 1 && !isDrifting)
        {
            isDrifting = true;
            oldSteering = steeringStrength;
            steeringStrength *= 2;
        }

        if (isDrifting == true && driftInput == 0)
        {
            isDrifting = false;
            steeringStrength = oldSteering;
        }
    }

    private void Turn()
    {
        carRB.AddTorque(steeringStrength * steerInput * turnCurve.Evaluate(Mathf.Abs(carVelocityRatio)) * Mathf.Sign(carVelocityRatio) * carRB.transform.up, ForceMode.Acceleration);
    }

    private void DragSideways()
    {
        float sidewaysSpeed = carVelocity.x;

        float dragMag = -sidewaysSpeed * dragCoefficient;

        Vector3 forceDrag = carRB.transform.right * dragMag;
        carRB.AddForceAtPosition(forceDrag, carRB.worldCenterOfMass, ForceMode.Acceleration);
    }
    #endregion

    #endregion
}
