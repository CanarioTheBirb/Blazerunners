using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum PlayerMode
    {
        SinglePlayer = 0,
        MultiPlayer
    }
    [SerializeField]
    public PlayerMode playerMode = PlayerMode.SinglePlayer;

    //Changeable Values in Inspector
    [Header("----- Manager Values -----")]
    [SerializeField] public bool raceOver = false;
    [SerializeField] private GameObject checkpoints;
    [SerializeField] int maxRacers;
    public int curRacerAmount = 0;
    [SerializeField] public float laps = 1;
    [SerializeField] public float CheckpointForgiveness = 2;
    [SerializeField] private TMP_Text positionText;
    [SerializeField] private GameObject winMenu;
    [SerializeField] private GameObject wrongWayPopup;
    [SerializeField] private GameObject lapPopup;
    [SerializeField] public Transform racerFinishedBoxTransform; // place where the players disapear into
    [SerializeField] private List<string> racerPositions = new List<string>();
    [SerializeField] private List<float> racerPoints = new List<float>();
    [SerializeField] private List<GameObject> checkpointList = new List<GameObject>();
    [SerializeField] GameObject startingPositions;

    [Header("----- Racer Manager -----")]
    [SerializeField] GameObject playerPref;
    [SerializeField] GameObject AIRacer;
    [SerializeField] public SetPath setPath;
    public List<PlayerMovement> players = new List<PlayerMovement>();
    private List<AIMovement> agents = new List<AIMovement>();
    private int racerFinalPositions = 1;
    [SerializeField] public List<Color> racerColors = new List<Color>();

    //Sad that Unity doesn't support Inspector to see this :(
    private Dictionary<string, float> racers = new Dictionary<string, float>();
    private float checkpointsLength = 0;

    // Start is called before the first frame update
    void Start()
    {
        CheckpointSystem[] cPoints = checkpoints.GetComponentsInChildren<CheckpointSystem>();
        foreach (CheckpointSystem obj in cPoints)
        {
            checkpointList.Add(obj.gameObject);
            checkpointsLength++;
        }

        if (playerMode == PlayerMode.SinglePlayer)
        {
            PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
            player.name = "Player1";
            players.Add(player);
            AddRacer(player.name);
        }

        AddAgents();

        InvokeRepeating("UpdatePositions", 2.0f, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        //Respawn on death
        foreach (PlayerMovement player in players)
        {
            if (player.car.hp <= 0)
            {
                player.car.hp = 100;
                Respawn(player.gameObject);
            }
        }

        foreach (AIMovement agent in agents)
        {
            if (agent.car.hp <= 0)
            {
                agent.car.hp = 100;
                Respawn(agent.gameObject);
            }
        }

        //Position Text on plr
        if (playerMode == PlayerMode.SinglePlayer)
        {
            if (players[0].GetComponentInChildren<NewBaseCar>().finishedRace)
            {
                positionText.text = $"finshed in {players[0].GetComponentInChildren<NewBaseCar>().finalPlacement}";
            }
            else
            {
                positionText.text = GetPosition(players[0].gameObject).ToString() + " place / " + racers.Count() + " racers";
            }
        }
        AIBoostAssistant();
    }

    #region Internal Functions

    private void UpdatePositions()
    {
        ///Gets all the players/AI in the game and orders them based off how many points they have
        ///But will be overriden if they have the same amount of points, to which they will
        ///Go off of who is closer to the next checkpoint
        if (playerMode == PlayerMode.MultiPlayer)
        {
            if (racers.Count <= 1) return;
        }
        racerPositions.Clear();
        racerPoints.Clear();

        // Order racers by points (descending)
        var orderedRacers = racers.OrderByDescending(x => x.Value);

        foreach (var kv in orderedRacers)
        {
            racerPoints.Add(kv.Value);
            racerPositions.Add(kv.Key);
        }

        for (int i = 1; i < racerPoints.Count; i++)
        {
            // Skip if points are not the same
            if (racerPoints[i] != racerPoints[i - 1])
            {
                continue;
            }

            var first = GameObject.Find(racerPositions[i - 1]);
            var second = GameObject.Find(racerPositions[i]);

            int racer1Checkpoint = (int)((racers[first.name] + checkpointsLength) % checkpointsLength);
            int racer2Checkpoint = (int)((racers[second.name] + checkpointsLength) % checkpointsLength);
            if (Vector3.Distance(first.GetComponentInChildren<Rigidbody>().transform.position, checkpointList[racer1Checkpoint].transform.position) >
                Vector3.Distance(second.GetComponentInChildren<Rigidbody>().transform.position, checkpointList[racer2Checkpoint].transform.position))
            {
                // Swap positions in racerPositions
                string tempName = racerPositions[i - 1];
                racerPositions[i - 1] = racerPositions[i];
                racerPositions[i] = tempName;
            }
        }


        for (int i = 0; i < racerPositions.Count; i++)
        {
            NewBaseCar car = GameObject.Find(racerPositions[i]).GetComponentInChildren<NewBaseCar>();
            if (car != null)
            {
                car.curPlacement = i + 1;
            }
        }
    }

    private float CheckpointAfter(string racerName, GameObject checkpoint)
    {
        //General thought process
        //Make a for loop that will count through the list of checkpoints
        //Until it finds the given one,
        //When it does check if it's greater than the current value
        //If not then don't give them point, etc.
        int currCheckpoint = 0;

        for (int i = 0; i < checkpointsLength; i++)
        {
            if (checkpointList[i] == checkpoint)
            {
                currCheckpoint = i;
                break;
            }
        }

        int racersCheckpoint = (int)((racers[racerName] + checkpointsLength) % checkpointsLength);

        if (currCheckpoint >= racersCheckpoint && (currCheckpoint - racersCheckpoint) <= CheckpointForgiveness)
        {
            return currCheckpoint - racersCheckpoint;
        }

        return -1;
    }

    // Adds Agents to the race
    public void AddAgents()
    {
        Transform[] pos = startingPositions.GetComponentsInChildren<Transform>();
        int currAgent = 1;
        for (int i = players.Count + 1; i <= maxRacers; i++)
        {
            var agent = Instantiate(AIRacer, pos[i].position, pos[i].rotation);
            agent.name = $"AI-{currAgent}";
            agent.GetComponentInChildren<MeshRenderer>().material.color = racerColors[Random.Range(0, racerColors.Count)];
            AddRacer(agent.name);
            agents.Add(agent.GetComponent<AIMovement>());
            currAgent++;
        }

        //Initializes agents
        for (int i = 1; i < agents.Count; i++)
        {
            // Sets what placement this AI wants to be when it finishes the race
            agents[i].GoalPlacement = i;
        }
    }

    // Summary: This Helps the Agents have a fair chance against players,
    // and organizes how the AI respects each other.
    private void AIBoostAssistant()
    {
        if (agents.Count == 0) return;

        foreach (var agent in agents)
        {
            if (agent.GetComponentInChildren<NewBaseCar>().curPlacement > agent.GoalPlacement) // if this agent is behind
            {
                //Gives a boost based on how far back this agent is from where he plans to be
                agent.catchUpBoost = 2.0f * (agent.GetComponentInChildren<NewBaseCar>().curPlacement - agent.GoalPlacement);
            }
            else
            {
                agent.catchUpBoost = 0;
            }
        }
    }

    private void WrongWay(GameObject racer, GameObject checkpoint)
    {
        float angle = -1 * Vector3.SignedAngle(checkpoint.transform.forward, checkpoint.transform.position - racer.transform.position, Vector3.up);

        if (racer.transform.root.CompareTag("Untagged") && angle < 0)
        {
            wrongWayPopup.SetActive(true);
        }
        else if (racer.transform.root.CompareTag("Untagged") && angle > 0 && wrongWayPopup.activeSelf == true)
        {
            wrongWayPopup.SetActive(false);
        }

    }

    private void LapPopupOff()
    {
        lapPopup.SetActive(false);
    }

    private int CalculatePlayerLap(float currPoints)
    {
        for (int i = 0; i <= laps; ++i)
        {
            if (currPoints == ((checkpointsLength - 1) * i))
            {
                return i + 1;
            }
        }
        return 1;
    }

    #endregion

    #region External Functions

    public void Respawn(GameObject racer)
    {
        int racersCheckpoint = (int)((racers[racer.name] + checkpointsLength) % checkpointsLength);
        Transform transformRacer = racer.GetComponentInChildren<Rigidbody>().transform.parent.transform;
        transformRacer.position = checkpointList[racersCheckpoint].transform.position;
        transformRacer.localRotation = checkpointList[racersCheckpoint].transform.localRotation;
        racer.GetComponentInChildren<Rigidbody>().velocity = Vector3.zero;
    }

    public int GetPosition(GameObject racer)
    {
        for (int i = 0; i < racerPositions.Count; i++)
        {
            if (racerPositions[i] == racer.name)
            {
                return i + 1;
            }
        }

        return -1;
    }

    public void AddRacer(string racerName)
    {
        if (playerMode == PlayerMode.MultiPlayer)
        {
            //A way to grab the player cars for splitscreen
            PlayerMovement playerTemp = GameObject.Find(racerName).GetComponentInChildren<PlayerMovement>();
            if (playerTemp != null)
            {
                if (!players.Contains(playerTemp))
                    players.Add(playerTemp);
            }
        }

        racers[racerName] = 0;
        racerPositions.Add(racerName);
        curRacerAmount++;
    }

    public void AddPoints(GameObject racer, GameObject checkpointHit)
    {
        var racerName = racer.name;
        NewBaseCar curCar = racer.GetComponentInChildren<NewBaseCar>();
        if (curCar == null) return;

        if (racers[racerName] < checkpointsLength * laps && checkpointHit.name != "Final")
        {
            float returnedValue = CheckpointAfter(racerName, checkpointHit);

            if (returnedValue == 0)
            {
                racers[racerName] += 1;
                //WrongWay(racer, checkpointHit);
            }
            else if (returnedValue > 0)
            {
                racers[racerName] += returnedValue + 1;
                //WrongWay(racer, checkpointHit);
            }
            else if (returnedValue < 0)
            {
                // WrongWay(racer, checkpointHit);
            }

            //float temp = CalculatePlayerLap(racers[racerName]);
            //if (temp != 1)
            //{
            //    if (curCar != null)
            //    {
            //        curCar.curLap = (int)temp;
            //        if (racer.GetComponentInChildren<PlayerMovement>() != null)
            //        {
            //            if (playerMode == PlayerMode.SinglePlayer)
            //            {
            //                if (temp == laps)
            //                {
            //                    lapPopup.GetComponentInChildren<TMP_Text>().text = "Final Lap";
            //                }
            //                else
            //                {
            //                    lapPopup.GetComponentInChildren<TMP_Text>().text = $"Lap: {temp}/{laps}";
            //                }
            //                lapPopup.SetActive(true);
            //                Invoke("LapPopupOff", 1.5f);
            //            }
            //        }
            //    }
            //}
        }
        else if (racers[racerName] >= (checkpointsLength - 1) * curCar.curLap && checkpointHit.name == "Final" && !curCar.finishedRace)
        {
            curCar.curLap++;
            if (racer.GetComponentInChildren<PlayerMovement>() != null)
            {
                if (playerMode == PlayerMode.SinglePlayer)
                {
                    if (curCar.curLap == laps)
                    {
                        lapPopup.GetComponentInChildren<TMP_Text>().text = "Final Lap";
                    }
                    else
                    {
                        lapPopup.GetComponentInChildren<TMP_Text>().text = $"Lap: {curCar.curLap}/{laps}";
                    }

                    if (curCar.curLap <= laps)
                    {
                        //Ensures that the lap popup doesn't show when you finish the race
                        lapPopup.SetActive(true);
                        Invoke("LapPopupOff", 1.5f);
                    }
                }
            }

            if (curCar.curLap > laps)
            {
                //The Racer has finished
                curCar.finishedRace = true;
                curCar.finalPlacement = racerFinalPositions;
                racerFinalPositions++;

                //Spawns players into a box when they finish the race so they can't continue racing
                if (playerMode == PlayerMode.MultiPlayer)
                    curCar.transform.parent.gameObject.transform.position = racerFinishedBoxTransform.position;


                //Checks to see how many players finished the race
                int finishedPlayers = 0;
                for (int i = 0; i < players.Count; i++)
                {
                    NewBaseCar car = players[i].GetComponentInChildren<NewBaseCar>();
                    if (car.finishedRace)
                    {
                        finishedPlayers++;
                    }

                }

                //if all of the players finished the race then put up win menu
                if (finishedPlayers == players.Count)
                {
                    //Single Player condition
                    if (playerMode == PlayerMode.SinglePlayer)
                    {
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        winMenu.SetActive(true);
                    }
                    raceOver = true;

                    if (playerMode == PlayerMode.MultiPlayer)
                    {
                        Invoke(nameof(EndRace), 5.0f);
                    }
                }
            }
        }
    }

    void EndRace()
    {
        Destroy(PlayerCarrier.instance.gameObject);
        SceneManager.LoadScene("MainMenu");
    }
    #endregion
}
