using GameKit.Utilities.Types;
using IO.Swagger.Model;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.Assertions.Comparers;
using UnityEngine.InputSystem;

public class NewBaseCar : MonoBehaviour
{
    [Header("----- Gameplay -----")]
    public float hp = 100.0f;
    public float itemRange;
    public int curPlacement;
    public int finalPlacement;
    public int curLap;
    public bool finishedRace = false;
    public Transform itemPlace;
    public GameObject item;
    public GameObject aim;
    public bool aimAssist = true;
    public bool targetLocked { get; private set; }

    [Header("---- References ----")]
    public Vector3 NearestCar = Vector3.zero;
    public float curSpeed;
    public float curRot;
    public Vector3 curVelocity;
    public bool isGrounded = false;
    public bool isWheeling = false;
    public bool isBoosting = false;
    public bool isDrifting = false;
    public bool isSlowed = false;
    public bool raceStart = false;

    [Header("----- Car Settings -----")]
    [SerializeField] public float acceleration = 25f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float steeringStrength = 15f;

    [Header("----- Visual -----")]
    [SerializeField] private TrailRenderer[] skidMarks;
    [SerializeField] private float skidWidth = 0.052f;
    [SerializeField] private float minSkidVelocity = 10f;
    [SerializeField] private ParticleSystem[] driftSmoke;
    [SerializeField] private ParticleSystem[] boostTrail;
    [SerializeField] private ParticleSystem damageSmoke1;
    [SerializeField] private ParticleSystem damageSmoke2;
    [SerializeField] private ParticleSystem damageSmoke3;


    [Header("---- Car Parts ----")]
    [SerializeField] public Rigidbody carRB;
    [SerializeField] public GameObject carModel;
    [SerializeField] public List<GameObject> wheels;
    [SerializeField] public List<GameObject> frontWheels;
    [SerializeField] public MeshRenderer Body;
    [SerializeField] public List<Material> texture;

    float speed;
    float rot;
    float slowTimer = 3.0f;

    //EXTERNAL VALUES TO CHANGE HERE
    public float moveInput { get; set; }
    public float steerInput { get; set; }
    public float driftInput { get; set; }

    #region Internal Functions
    private void Update()
    {
        transform.position = carRB.transform.position - new Vector3(0, 1f, 0);
        Drive();
        Drift();
        Turn();
        Slowing();
    }
    private void FixedUpdate()
    {
        CheckIfOnGround();

        if (carRB.useGravity)
        {
            carRB.AddForce(Vector3.down * 32f, ForceMode.Acceleration);
        }

        carRB.AddForce(carModel.transform.forward * curSpeed, ForceMode.Acceleration);

        if (raceStart)
        {
           transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(0, transform.eulerAngles.y + curRot, 0), Time.deltaTime * 5f);
        }


        //Visuals
        SkidMarks();
        UpdateTires();
        DriftSmoke();
        BoostTrail();
        HealthIndication();

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
        curLap = 1;
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
        if (steerInput != 0 && isDrifting && isGrounded)
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
        for (int i = 0; i < wheels.Count; i++)
        {
            if (i < 2)
            {
                frontWheels[i].transform.localEulerAngles = new Vector3(frontWheels[i].transform.localEulerAngles.x, steerInput * 30, frontWheels[i].transform.localEulerAngles.z);
            }

            wheels[i].transform.localEulerAngles += new Vector3(curSpeed, 0, 0);
        }
    }

    void BoostTrail()
    {
        if (isBoosting)
        {
            carRB.AddForce(carModel.transform.forward * 200, ForceMode.Acceleration);
            foreach (var trail in boostTrail)
            {
                trail.Emit(1);
            }
        }
    }

    void HealthIndication()
    {
        if (hp <= 75 && hp > 50)
        {
            damageSmoke1.Emit(1);
        }
        else if (hp <= 50 && hp > 25)
        {
            damageSmoke2.Emit(1);
        }
        else if (hp <= 25)
        {
            damageSmoke3.Emit(1);
        }

    }
    #endregion

    #region Car Checks
    private void CheckIfOnGround()
    {
        RaycastHit hit;

        if (Physics.Raycast(carModel.transform.position, -carModel.transform.up, out hit, 5f))
        {
            isGrounded = true;

            Quaternion RotToGround = Quaternion.FromToRotation(carModel.transform.up, hit.normal);
            carModel.transform.rotation = Quaternion.Slerp(carModel.transform.rotation, RotToGround * carModel.transform.rotation, 10);

        }
        else
        {
            isGrounded = false;
        }

    }

    void Slowing()
    {
        if (isSlowed)
        {
            curSpeed = curSpeed / 2;
            Invoke(nameof(DisableSlow), slowTimer);
        }
    }
    public void ActiveSlow()
    {
        isSlowed = true;
    }
    void DisableSlow()
    {
        isSlowed = false;
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
        List<NewBaseCar> cars = new List<NewBaseCar>();

        for (int i = 0; i < sphere.Length; i++)
        {
            if (sphere[i].gameObject.GetComponentInParent<NewBaseCar>() != null)
            {
                if (sphere[i].gameObject.GetComponentInParent<NewBaseCar>() != this)
                {
                    cars.Add(sphere[i].gameObject.GetComponentInParent<NewBaseCar>());
                }
            }
        }

        //no cars then straight
        if (cars.Count == 0)
        {
            NearestCar = carModel.transform.forward;
            targetLocked = false;
            item.transform.LookAt((item.transform.position + NearestCar) + new Vector3(0, -0.03f, 0));
            return;
        }

        //Looks for the closest car
        float closestDist = Mathf.Infinity;
        NewBaseCar closestCar = null;
        foreach (NewBaseCar car in cars)
        {
            //BoxCollider box = car.gameObject.GetComponentInChildren<BoxCollider>(); // grabs the position of the object moving around from this Rigidbody
            Transform target = car.carModel.transform;
            float distance = Vector3.Distance(item.transform.position, target.position);
            if (distance < closestDist)
            {
                closestDist = distance;
                closestCar = car;
            }
        }

        NearestCar = closestCar.transform.position;
        targetLocked = true;
        item.transform.LookAt(NearestCar + new Vector3(0, -0.03f, 0));

        RaycastHit[] hits = Physics.RaycastAll(item.GetComponent<ItemManager>().shootPos.position, item.GetComponent<ItemManager>().shootPos.forward.normalized, Vector3.Distance(carRB.position, NearestCar));
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject.GetComponentInParent<BoxCollider>() == carModel.GetComponent<BoxCollider>())
            {
                item.transform.LookAt(closestCar.carRB.position + new Vector3(0, -0.03f, 0));
                Debug.DrawLine(item.GetComponent<ItemManager>().shootPos.position, closestCar.carModel.transform.position, Color.red);
            }
        }
        Debug.DrawLine(item.GetComponent<ItemManager>().shootPos.position, NearestCar, Color.red);
    }

    //helper function to give me a direction of the cars future position
    Vector3 PredictFuturePosition(NewBaseCar bCar)
    {
        Rigidbody closestRB = bCar.carRB;

        Vector3 resultsPos = bCar.carModel.transform.position;
        //reset weapon so we can get accurate angles
        item.transform.LookAt(item.transform.position + carModel.transform.forward);
        ItemManager itemMan = item.GetComponent<ItemManager>();

        float dist = Vector3.Distance(item.transform.position, bCar.carModel.transform.position);
        float time = dist / (itemMan.bulletSpeed);
        resultsPos = closestRB.position + closestRB.velocity * closestRB.velocity.magnitude * time;
        resultsPos += new Vector3(0, -0.03f, 0);
        return resultsPos;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(carRB.transform.position, itemRange);
    }
    #endregion

    #region Driving
    void Drive()
    {
        if (moveInput > 0)
        {
            speed = acceleration;
        }

        if (moveInput < 0)
        {
            speed = -deceleration;
        }

        curSpeed = Mathf.SmoothStep(curSpeed, speed, Time.deltaTime * 12f);
        speed = 0f;
    }

    private void Turn()
    {
        if (!isDrifting)
        {
            if (moveInput >= 0)
            {
                rot = steeringStrength * steerInput;
            }
            else
            {
                rot = steeringStrength * -steerInput;
            }
        }
        else
        {
            rot = steeringStrength * 2f * steerInput;
        }

        curRot = Mathf.Lerp(curRot, rot, Time.deltaTime * 4f);
        rot = 0f;
    }

    private void Drift()
    {
        if (driftInput != 0)
        {
            isDrifting = true;
        }
        else
        {
            isDrifting = false;
        }
    }
    #endregion

    #endregion
}
