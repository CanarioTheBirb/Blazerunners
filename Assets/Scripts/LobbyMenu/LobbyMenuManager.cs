using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LobbyMenuManager : MonoBehaviour
{
    [SerializeField] public GameObject playerSprite;
    [SerializeField] List<Transform> spawnpoints;
    [SerializeField] Button startButton;
    [SerializeField] List<GameObject> ready;
    [SerializeField] List<GameObject> join;
    [SerializeField] List<GameObject> select;
    [SerializeField] GameObject start;

    public List<GameObject> curPlayers = new List<GameObject>();
    public int readyPlayers = 0;

    public PlayerInputManager players;

    private void Awake()
    {
        players = FindFirstObjectByType<PlayerInputManager>();
    }
    public void OnEnable()
    {
       players.onPlayerJoined += ConnectPlayer;
    }
    public void OnDisable()
    {
        players.onPlayerJoined -= ConnectPlayer;
    }

    void ConnectPlayer(PlayerInput player)
    {
        if (player.gameObject.name == "PlayerMovement")
        {
            return;
        }
        player.gameObject.transform.parent.TryGetComponent(out PlayerManager playerM);

        playerM.uiControls.playerCursor = playerSprite.transform.GetChild(players.playerCount - 1).gameObject;
        playerM.uiControls.playerCursor.SetActive(true);

        playerM.uiControls.curButton = startButton;

        playerM.transform.position = spawnpoints[players.playerCount - 1].position;
        playerM.transform.rotation = spawnpoints[players.playerCount - 1].rotation;

        join[players.playerCount - 1].SetActive(false);

        curPlayers.Add(playerM.uiControls.gameObject);
    }

    private void Update()
    {
        if (curPlayers.Count == 0)
        {
            return;
        }

        for (int i = 0; i < curPlayers.Count; i++)
        {
            if (curPlayers[i].gameObject.GetComponent<PlayerUIControls>().selected && !ready[i].activeInHierarchy)
            {
                ready[i].SetActive(true);
                readyPlayers++;
            }
            else if (!curPlayers[i].gameObject.GetComponent<PlayerUIControls>().selected && ready[i].activeInHierarchy)
            {
                ready[i].SetActive(false);
                readyPlayers--;
            }

            if (!curPlayers[i].gameObject.GetComponent<PlayerUIControls>().selected)
            {
                select[i].SetActive(true);
            }
            else if (curPlayers[i].gameObject.GetComponent<PlayerUIControls>().selected)
            {
                select[i].SetActive(false);
            }
        }

        if (readyPlayers == curPlayers.Count)
        {
            start.SetActive(true);
            foreach (var player in curPlayers)
            {
                player.gameObject.GetComponent<PlayerUIControls>().everyoneReady = true;
            }
        }
        else
        {
            start.SetActive(false);
            foreach (var player in curPlayers)
            {
                player.gameObject.GetComponent<PlayerUIControls>().everyoneReady = false;
            }
        }
    }
}
