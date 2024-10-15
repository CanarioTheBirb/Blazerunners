using GameKit.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization.Formatters;
using UnityEngine;
using UnityEngine.Rendering;

public class AIMovement : MonoBehaviour
{
    [Header("----- Components -----")]
    [SerializeField] public NewBaseCar car;
    GameManager gameManager;

    [Header("----- AI Stuff -----")]
    [SerializeField] Transform AIPos;
    private Node[] currentPath;
    public int currentPlacement;
    public int GoalPlacement;
    public float catchUpBoost;
    [SerializeField] float itemUsageCooldown;
    bool isReadyToUseItem;
    public float carVelocityRatio;
    private float startingMaxSpeed;

    [Header("----- Sensors -----")]
    private List<RaycastHit> mysteryBoxHits = new List<RaycastHit>();
    [SerializeField] float mysteryBoxScanDist;
    private bool isMysteryBoxActive;

    [SerializeField] float respectedSpaceRadius;


    [Header("----- Ranges -----")]
    [SerializeField] float distBeforeTurn;
    [SerializeField] float angleForTurn;
    [SerializeField] float AICheckpointRadius;
    [SerializeField] float facingCheckpointAngle;
    [SerializeField] float idleTimer;
    [SerializeField] float angleTilDownhill;
    [SerializeField] float angleTilUphill;
    [SerializeField] float moveOnAngle;

    [Header("----- Tracking Values -----")]
    [SerializeField] int currMainPathObj;
    [SerializeField] int currSubPathObj;
    public int currPathObj;
    public bool isOnMainPath; // bool for checking which path if false then probably on a subpathbranch;
    public float timeIdle;
    public float steer;
    public bool isCarInWay = false;
    public Vector3 closestCarPos;
    public Vector3 pathDir;

    public bool isIdle = false;
    public bool isDriving = false;
    public bool isReversing = false;
    public bool isDrifting = false;

    public float distFromPath;
    public float distFromNextPath;
    public float distFromAToB;
    public float nextTurnAngle;
    public float dotProduct;
    public float AICarAngleX;

    public MovementState state;

    private int mysteryBoxFlags;
    private Vector3 mysteryBoxDir;

    public int carsInWay;

    public enum MovementState
    {
        idle,
        driving,
        reversing,
        drifting,
    }


    void Start()
    {
        gameManager = GameObject.FindAnyObjectByType<GameManager>();
        SetStartValues();
        GetSteer();
    }

    void FixedUpdate()
    {
        car.acceleration = startingMaxSpeed + catchUpBoost;
        carVelocityRatio = car.curSpeed / car.acceleration;
        stateCheck();
        GetSteer();
        Movement();
        UseItem();
    }
    void SetStartValues()
    {
        mysteryBoxFlags = 0;
        currMainPathObj = 1;
        currPathObj = currMainPathObj;
        currSubPathObj = 0;
        nextTurnAngle = 180;
        isDriving = false;
        car.moveInput = 0;
        car.steerInput = 0;
        car.driftInput = 0;
        currentPath = new Node[gameManager.setPath.path.Length];
        currentPath = gameManager.setPath.path;
        isOnMainPath = true;
        isMysteryBoxActive = true;
        isReadyToUseItem = true;
        startingMaxSpeed = car.acceleration;
        catchUpBoost = 0;
    }

    // AI State checker
    void stateCheck()
    {
        if (carVelocityRatio > 1 || carVelocityRatio < -1)
        {
            state = MovementState.idle;
            timeIdle += Time.deltaTime;
            if (timeIdle >= idleTimer)
            {
                isIdle = false;
                isReversing = true;
                Debug.Log("Repositioning...");
            }
        }
        else if (carVelocityRatio > .05f || isDriving)
        {
            if (isDrifting)
            {
                state = MovementState.drifting;
            }
            else
            {
                state = MovementState.driving;
            }
            timeIdle = 0;
        }
        else if (carVelocityRatio < -.05f || isReversing)
        {
            state = MovementState.reversing;
            timeIdle = 0;
        }

    }

    // This determines which way the current node is
    void GetPathDirection()
    {
        pathDir = new Vector3(currentPath[currPathObj].pathPos.position.x, AIPos.position.y, currentPath[currPathObj].pathPos.position.z);

        if (nextTurnAngle >= angleForTurn || nextTurnAngle <= -angleForTurn)
        {
            pathDir = new Vector3(currentPath[currPathObj].pathPos.position.x, AIPos.position.y, currentPath[currPathObj].pathPos.position.z);
        }
        else
        {
            if (Vector3.Distance(AIPos.position, currentPath[currPathObj].pathPos.position) <= distBeforeTurn)
            {
                if (currPathObj == currentPath.Length - 1)
                {
                    pathDir = new Vector3(currentPath[1].pathPos.position.x, AIPos.position.y, currentPath[1].pathPos.position.z);
                }
                else
                {
                    pathDir = new Vector3(currentPath[currPathObj + 1].pathPos.position.x, AIPos.position.y, currentPath[currPathObj + 1].pathPos.position.z);
                }
            }
        }
        dotProduct = Vector3.Dot(AIPos.forward, pathDir);
    }

    // tells the the AI which direction to turn torwards based on the node the AI is on
    void GetSteer()
    {
        //move based on these scans
        MysteryBoxSensors();
        RespectCarSpace();

        //Update base values
        GetPathDirection();
        distFromPath = Vector3.Distance(AIPos.position, pathDir);
        steer = Vector3.Angle(AIPos.forward, pathDir - AIPos.position);

        if (isCarInWay) //if you're just cruisin on a road be careful of others space
        {
            if(steer >= 20) // you can move around a car but dont ignore your destination
            {
                car.steerInput = Mathf.Sign(Vector3.Cross(AIPos.forward, pathDir - AIPos.position).y);
            }
            else
            {
                car.steerInput = -(Mathf.Sign(Vector3.Cross(AIPos.forward, closestCarPos).y));
            }
        }
        else if (mysteryBoxFlags != 0 && car.item == null) // if a mystery box has been scanned by an ai
        {

            if (isMysteryBoxActive == true)
            {
                isMysteryBoxActive = false;
                int index = Random.Range(0, mysteryBoxHits.Count);
                RaycastHit hit = mysteryBoxHits[index];
                mysteryBoxDir = new Vector3(hit.point.x, AIPos.position.y, hit.point.z);
            }

            steer = Vector3.Angle(AIPos.forward, mysteryBoxDir - AIPos.position);

            if (steer >= 50)
            {
                mysteryBoxFlags = 0;
            }
            if (steer >= 2)
            {
                car.steerInput = Mathf.Sign(Vector3.Cross(AIPos.forward, mysteryBoxDir - AIPos.position).y);
            }
            else
            {
                car.steerInput = 0;
            }


        }
        else // Normal Behavior
        {
            isMysteryBoxActive = true;
            mysteryBoxFlags = 0;
            mysteryBoxHits.Clear();

            if (steer >= 5)
            {
                car.steerInput = Mathf.Sign(Vector3.Cross(AIPos.forward, pathDir - AIPos.position).y);
            }
            else
            {
                car.steerInput = 0;
            }
        }

        if (distFromPath <= currentPath[currPathObj].GetActivateNodeDist())
        {
            if (currentPath[currPathObj].GetDrift())
            {
                isDrifting = true;
                car.driftInput = 1;
                Invoke("EndDrift", currentPath[currPathObj].GetDriftTime());
            }
            GetNextPathPoint();
        }
    }

    // Tells the AI to Drive forward or backwards
    void Movement()
    {
        if (isDriving)
        {
            if (carVelocityRatio >= currentPath[currPathObj].GetNodeSpeed() && distFromPath < currentPath[currPathObj].GetDecelerateNodeDist())
            {
                car.moveInput = -1;
            }
            else if (nextTurnAngle >= angleForTurn || nextTurnAngle <= -angleForTurn)
            {
                car.moveInput = 1 + catchUpBoost;
            }
            else
            {
                if (carVelocityRatio <= currentPath[currPathObj].GetNodeSpeed())
                {
                    car.moveInput = 1 + catchUpBoost;
                }
                else
                {
                    car.moveInput = 0;
                }
            }
        }
        else if (isReversing)
        {
            car.moveInput = -1;
            if (dotProduct <= facingCheckpointAngle)
            {
                isReversing = false;
                isDriving = true;
            }
        }
    }

    //Just sets the next position to drive torwards
    void GetNextPathPoint()
    {
        if (isOnMainPath)
        {
            if (currentPath[currMainPathObj] == currentPath[currentPath.Length - 1])
            {
                currMainPathObj = 1;
            }
            else
            {
                currMainPathObj++;
            }
            currPathObj = currMainPathObj;
        }
        else
        {
            currSubPathObj++;
            currPathObj = currSubPathObj;
        }
        SetCurrentPath();
    }

    //This is used for setting the pathway if a node has multiple pathways
    void SetCurrentPath()
    {
        if (currentPath[currPathObj].GetPathWays() > 1 && isOnMainPath)
        {
            //Calculates the total cost of the path's points together
            int totalCost = 0;
            for (int i = 0; i < currentPath[currPathObj].GetDerivedNodes().Length; i++)
            {
                if (currentPath[currPathObj].GetDerivedNodes()[i].HasMysteryBox() && car.item != null)
                {
                    totalCost += (currentPath[currPathObj].GetDerivedNodes()[i].pathPoints - 200); //200 = however much a mystery box cost
                }
                else
                {
                    totalCost += currentPath[currPathObj].GetDerivedNodes()[i].pathPoints;
                }
            }

            int randomNum = Random.Range(0, totalCost); // the number that picks 
            int choice = -1;
            // choose which path to take based on calculating their points
            // this makes the choice based on what random number picks
            // (the one with more points will have a higher probablility
            // to get picked becuase it takes more space in the totalCost)
            for (int i = 0; i < currentPath[currPathObj].GetDerivedNodes().Length; i++)
            {
                choice++;
                if (currentPath[currPathObj].GetDerivedNodes()[i].HasMysteryBox() && car.item != null)
                {
                    randomNum -= (currentPath[currPathObj].GetDerivedNodes()[i].pathPoints - 200);
                }
                else
                {
                    randomNum -= currentPath[currPathObj].GetDerivedNodes()[i].pathPoints;
                }

                if (randomNum <= 0)
                {
                    break;
                }
            }

            //sets the current path to the choice
            currentPath = new Node[gameManager.setPath.path[currPathObj].GetDerivedNodes()[choice].GetDerivedNodes().Length];
            currentPath = gameManager.setPath.path[currPathObj].GetDerivedNodes()[choice].GetDerivedNodes();
            isOnMainPath = false;
            currSubPathObj = 0;
            currPathObj = currSubPathObj;
        }
        else if (!isOnMainPath && currentPath[currSubPathObj] == currentPath[currentPath.Length - 1])
        {
            Debug.Log("Back On Track");
            currentPath = new Node[gameManager.setPath.path.Length];
            currentPath = gameManager.setPath.path;
            isOnMainPath = true;
            currSubPathObj = 0;
            currMainPathObj += 1; // for the ai to go to the next node instead of back to the main node that had all of the sub-nodes
            currPathObj = currMainPathObj;
        }
    }

    //Called to stop the ai from drifting
    void EndDrift()
    {
        isDrifting = false;
        car.driftInput = 0;
        GetNextPathPoint();
    }

    //makes the AI used the current item
    void UseItem()
    {
        if (car.item != null && car.targetLocked == true && isReadyToUseItem)
        {
            isReadyToUseItem = false;
            car.item.GetComponent<ItemManager>().Use();
            Invoke("ItemCoolDown", itemUsageCooldown);
        }
    }

    void ItemCoolDown()
    {
        isReadyToUseItem = true;
    }

    // Scans for Mystery Boxes
    void MysteryBoxSensors()
    {
        if (car.item != null || !isMysteryBoxActive)
        {
            return;
        }
        mysteryBoxFlags = 0;
        mysteryBoxHits.Clear();
        RaycastHit hit;
        var rightAngle = Quaternion.AngleAxis(25.0f, AIPos.up) * AIPos.forward;
        var leftAngle = Quaternion.AngleAxis(-25.0f, AIPos.up) * AIPos.forward;

        //Front Sensor
        if (Physics.Raycast(AIPos.position + new Vector3(0.0f, .2f, 0.0f), AIPos.forward, out hit, mysteryBoxScanDist))
        {
            Debug.DrawLine(AIPos.position + new Vector3(0.0f, .2f, 0.0f), hit.point, Color.red);
            if (hit.collider.gameObject.CompareTag("MysteryBox"))
            {
                mysteryBoxFlags++;
                mysteryBoxHits.Add(hit);
            }
        }

        //Front Right Sensor
        if (Physics.Raycast(AIPos.position + new Vector3(0.0f, .2f, 0.0f), rightAngle, out hit, mysteryBoxScanDist))
        {
            Debug.DrawLine(AIPos.position + new Vector3(0.0f, .2f, 0.0f), hit.point, Color.red);
            if (hit.collider.gameObject.CompareTag("MysteryBox"))
            {
                mysteryBoxFlags++;
                mysteryBoxHits.Add(hit);
            }
        }

        //Front Left Sensor
        if (Physics.Raycast(AIPos.position + new Vector3(0.0f, .2f, 0.0f), leftAngle, out hit, mysteryBoxScanDist))
        {
            Debug.DrawLine(AIPos.position + new Vector3(0.0f, .2f, 0.0f), hit.point, Color.red);
            if (hit.collider.gameObject.CompareTag("MysteryBox"))
            {
                mysteryBoxFlags++;
                mysteryBoxHits.Add(hit);
            }
        }

    }

    private void RespectCarSpace()
    {
        carsInWay = 0;
        Collider[] colliders = Physics.OverlapSphere(AIPos.position, respectedSpaceRadius);
        List<NewBaseCar> cars = new List<NewBaseCar>();
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject.GetComponentInParent<NewBaseCar>())
            {
                cars.Add(colliders[i].gameObject.GetComponentInParent<NewBaseCar>());
            }
        }
        cars.Remove(car);

        //finding cars in front of the car
        List<NewBaseCar> carsToMoveFrom = new List<NewBaseCar>();
        for (int i = 0; i < cars.Count; i++)
        {
            float angle = Vector3.Angle(AIPos.forward, cars[i].carModel.transform.position - AIPos.position);
            if (angle <= 90f)
            {
                carsToMoveFrom.Add(cars[i]);
                carsInWay++;
            }
        }

        NewBaseCar closestCar = FindClosestCar(carsToMoveFrom);
        if (closestCar != null)
        {
            Vector3 dir = closestCar.carModel.transform.position - AIPos.position;
            Debug.DrawRay(AIPos.position, dir, Color.magenta);

            closestCarPos = dir;
            isCarInWay = true;
        }
        else
        {
            isCarInWay = false;
        }
    }

    private NewBaseCar FindClosestCar(List<NewBaseCar> cars)
    {
        if (cars.Count == 0) return null;

        float distance = Mathf.Infinity;
        NewBaseCar closestCar = null;
        foreach (var vroom in cars)
        {
            float dist = Vector3.Distance(AIPos.position, vroom.carModel.transform.position);
            if (distance >= dist)
            {
                closestCar = vroom;
                distance = dist;
            }
        }

        return closestCar;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(AIPos.position, respectedSpaceRadius);
    }
}
