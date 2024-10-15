using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using FishNet.Demo.AdditiveScenes;
using GameKit.Utilities;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using speedometer;

public class SSPlayerManager : MonoBehaviour
{
    private List<GameObject> players = new List<GameObject>();

    [SerializeField] private List<Transform> startingPoints;
    [SerializeField] private List<LayerMask> playerLayers;

    [SerializeField] private List<Color> playerColors; // List of colors for players

    [Header("Mini-map Settings")]
    [SerializeField] private Camera miniMapCameraPlayer1;
    [SerializeField] private Camera miniMapCameraPlayer2;
    [SerializeField] private Camera miniMapCameraPlayer3;
    [SerializeField] private Camera miniMapCameraPlayer4;
    [SerializeField] private RenderTexture miniMapRenderTexturePlayer1;
    [SerializeField] private RenderTexture miniMapRenderTexturePlayer2;
    [SerializeField] private RenderTexture miniMapRenderTexturePlayer3;
    [SerializeField] private RenderTexture miniMapRenderTexturePlayer4;
    [SerializeField] private Sprite player1IconSprite;
    [SerializeField] private Sprite player2IconSprite;
    [SerializeField] private Sprite player3IconSprite;
    [SerializeField] private Sprite player4IconSprite;

    [Header("Countdown Settings")]
    public List<RawImage> countdownImages;
    public List<Color> countdownColors;
    public List<float> countdownDurations;
    private int currentCountdownIndex = 0;
    private float timeLeft;
    private bool raceStarted = false;
    [SerializeField] private AudioClip countdownClip;

    //[SerializeField] private GameObject speedometerPrefab; // Prefab for speedometer UI
    //[SerializeField] private GameObject sliderPrefab;     // Prefab for invisible slider

    private List<Slider> sliders = new List<Slider>();    // List to store instantiated sliders
    private List<GameObject> speedometerUIs = new List<GameObject>();
    private PlayerInputManager playerInputManager;
    private MiniMapIcon miniIconInstance;
    GameManager gameManagerInstance;
    private Timer gameTimerInstance;
    private Dictionary<PlayerInput, Image> miniMapIcons = new Dictionary<PlayerInput, Image>(); // To store mini-map icons per player
    private void Awake()
    {
        playerInputManager = FindObjectOfType<PlayerInputManager>();
        gameManagerInstance = FindAnyObjectByType<GameManager>();
        foreach (var player in PlayerCarrier.instance.playerCars)
        {
            player.SwitchtoGameplay();
            AddPlayer(player.gameplayControls.gameObject);
        }
        playerInputManager.splitScreen = true;
    }
    //private void OnEnable()
    //{
    //    playerInputManager.onPlayerJoined += AddPlayer;
    //}
    private void Start()
    {
        if (countdownImages.Count != countdownColors.Count || countdownColors.Count != countdownDurations.Count)
        {
            Debug.LogError("Countdown settings lists must be of the same length.");
            return;
        }

        timeLeft = countdownDurations[currentCountdownIndex];
        countdownImages[currentCountdownIndex].color = countdownColors[currentCountdownIndex];
    }
    private void Update()
    {
        if (!raceStarted)
        {
            HandleCountdown();
        }
    }

    public void AddPlayer(GameObject player)
    {
        players.Add(player);

        Transform playerTransform = player.transform;
        playerTransform.position = startingPoints[players.Count - 1].position;
        playerTransform.rotation = startingPoints[players.Count - 1].rotation;
        Debug.Log("Player " + players.Count + " joined");

        // Assign player tag
        player.tag = "Player" + players.Count;
        player.name = "Player" + players.Count;
        gameManagerInstance.AddRacer($"{player.name}");
        // Find the player's existing camera named "PlayerCamera"
        Camera playerCamera = playerTransform.Find("PlayerCamera")?.GetComponent<Camera>();
        if (playerCamera != null)
        {
            // Assign camera layer
            int layerToAdd = (int)Mathf.Log(playerLayers[players.Count - 1].value, 2);
            playerCamera.cullingMask |= 1 << layerToAdd;
            playerCamera.gameObject.layer = layerToAdd;
            Debug.Log("Camera culling mask set to layer: " + layerToAdd);
        }

        // Create UI for player
        CreatePlayerUI(player);
        if (players.Count == 1)
        {
            Canvas playerCanvas = playerTransform.GetComponentInChildren<Canvas>();
            AddTimerUI(playerCanvas.transform);
        }
    }

    private void HandleCountdown()
    {
        PlayerMovement[] playerCars = GameObject.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        AIMovement[] aiCars = GameObject.FindObjectsByType<AIMovement>(FindObjectsSortMode.None);

        foreach (PlayerMovement car in playerCars)
        {
            car.enabled = false;
        }
        foreach (AIMovement car in aiCars)
        {
            car.isDriving = false;
        }

        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0)
        {
            currentCountdownIndex++;
            if (currentCountdownIndex < countdownImages.Count)
            {
                timeLeft = countdownDurations[currentCountdownIndex];
                countdownImages[currentCountdownIndex].color = countdownColors[currentCountdownIndex];
            }
            else
            {
                StartRace(playerCars, aiCars);
            }
        }
    }

    private void StartRace(PlayerMovement[] playerCars, AIMovement[] aiCars)
    {
        raceStarted = true;
        foreach (var image in countdownImages)
        {
            image.enabled = false;
        }

        foreach (PlayerMovement car in playerCars)
        {
            car.enabled = true;
            car.car.raceStart = true;
        }

        foreach (AIMovement car in aiCars)
        {
            car.isDriving = true;
            car.car.raceStart = true;
        }
    }

    private void CreatePlayerUI(GameObject player)
    {
        // Create a canvas for the player
        GameObject canvasObject = new GameObject("PlayerUICanvas" + players.Count);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();
        // Adjust canvas size and position based on the player number
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (players.Count == 1)
        {
            // Player 1's canvas takes up the whole screen initially
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.sizeDelta = Vector2.zero;
        }
        else if (players.Count == 2)
        {
            // Player 2's canvas will be positioned to the right
            canvasRect.anchorMin = new Vector2(0.5f, 0);
            canvasRect.anchorMax = Vector2.one;
            canvasRect.pivot = new Vector2(0.5f, 0.5f);
            canvasRect.sizeDelta = new Vector2(Screen.width / 2, Screen.height);
            canvasRect.anchoredPosition = new Vector2(Screen.width / 4, 0);
        }

        // Parent the canvas to the player
        canvasObject.transform.SetParent(player.transform, false);

        // Create placement UI
        CreatePlacementUI(canvas.transform);

        // Create mini-map
        CreateMiniMap(canvas.transform, player);

    }


    private void CreateMiniMap(Transform canvasTransform, GameObject player)
    {
        // Create a new GameObject for the mini-map raw image
        GameObject miniMapImageObject = new GameObject("MiniMapImage" + players.Count);
        miniMapImageObject.transform.SetParent(canvasTransform, false);

        // Add RawImage component to the GameObject
        RawImage miniMapRawImage = miniMapImageObject.AddComponent<RawImage>();
        miniMapImageObject.AddComponent<RectTransform>();


        // Assign the appropriate render texture based on the player number
        if (players.Count == 1)
        {

            miniMapRawImage.texture = miniMapRenderTexturePlayer1;
            miniMapCameraPlayer1.targetTexture = miniMapRenderTexturePlayer1;
            miniMapRawImage.rectTransform.anchoredPosition = new Vector2(785f, 365f);
            miniMapRawImage.rectTransform.sizeDelta = new Vector2(200f, 200f);
            //sets plaayer1 tag to its rigid body for icon transform
            GameObject player1Object = GameObject.FindGameObjectWithTag("Player1");
            Rigidbody childRigidbody = player1Object.GetComponentInChildren<Rigidbody>();
            Transform player1ObjectTransform = childRigidbody.transform;
            childRigidbody.tag = "FreeRacingCar1";

            miniMapImageObject.transform.SetParent(canvasTransform, false);
            Camera Camera1Transform = GameObject.Find("Player1miniMapCam")?.GetComponent<Camera>();

            miniMapImageObject.AddComponent<RectTransform>();
            // Player 1's mini-map icon
            Image player1IconImageOnMap1 = CreateMiniMapIconImage(miniMapRawImage.rectTransform, player1IconSprite, Color.red);
            CreateMiniMapIcon(player1IconImageOnMap1, Camera1Transform, player1ObjectTransform.transform);


        }
        else if (players.Count == 2)
        {

            miniMapRawImage.texture = miniMapRenderTexturePlayer1;
            miniMapCameraPlayer1.targetTexture = miniMapRenderTexturePlayer1;
            // sets location and size

            GameObject player1Object = GameObject.FindGameObjectWithTag("Player1");
            RawImage player1MiniMapRawImage = player1Object.GetComponentInChildren<RawImage>();
            player1MiniMapRawImage.rectTransform.anchoredPosition = new Vector2(785f, 365f);
            player1MiniMapRawImage.rectTransform.sizeDelta = new Vector2(200f, 200f);

            GameObject player2Object = GameObject.FindGameObjectWithTag("Player2");
            Rigidbody childRigidbody2 = player2Object.GetComponentInChildren<Rigidbody>();
            childRigidbody2.tag = "FreeRacingCar2";

            Transform player2Transform = GameObject.FindGameObjectWithTag("FreeRacingCar2").transform;
            Image player2IconImageOnMap2 = CreateMiniMapIconImage(miniMapRawImage.rectTransform, player2IconSprite, Color.blue);
            CreateMiniMapIcon(player2IconImageOnMap2, miniMapCameraPlayer2, player2Transform);



            if (player1Object != null)
            {
                if (player1MiniMapRawImage != null)
                {
                    Image player2IconImageOnMap1 = CreateMiniMapIconImage(player1MiniMapRawImage.rectTransform, player2IconSprite, Color.blue);
                    CreateMiniMapIcon(player2IconImageOnMap1, miniMapCameraPlayer1, player2Transform);

                }
            }
            GameObject miniMapImageObject2 = GameObject.Find("MiniMapImage2");
            DestroyObject(miniMapImageObject2);
        }
        else if (players.Count == 3)
        {
            //Renders mini map
            miniMapRawImage.texture = miniMapRenderTexturePlayer1;
            miniMapCameraPlayer1.targetTexture = miniMapRenderTexturePlayer1;
            // sets location and size

            GameObject player1Object = GameObject.FindGameObjectWithTag("Player1");
            RawImage player1MiniMapRawImage = player1Object.GetComponentInChildren<RawImage>();
            player1MiniMapRawImage.rectTransform.anchoredPosition = new Vector2(480f, -285f);
            player1MiniMapRawImage.rectTransform.sizeDelta = new Vector2(960f, 565f);


            GameObject player3Object = GameObject.FindGameObjectWithTag("Player3");
            Rigidbody childRigidbody3 = player3Object.GetComponentInChildren<Rigidbody>();
            childRigidbody3.tag = "FreeRacingCar3";
            Transform player3Transform = GameObject.FindGameObjectWithTag("FreeRacingCar3").transform;

            if (player1Object != null)
            {
                if (player1MiniMapRawImage != null)
                {
                    Image player3IconImageOnMap1 = CreateMiniMapIconImage(player1MiniMapRawImage.rectTransform, player3IconSprite, Color.green);
                    CreateMiniMapIcon(player3IconImageOnMap1, miniMapCameraPlayer1, player3Transform);

                }
            }
            //Deletes other mini maps
            GameObject miniMapImageObject1 = GameObject.Find("MiniMapImage3");
            DestroyObject(miniMapImageObject1);
            GameObject miniMapImageObject2 = GameObject.Find("MiniMapImage2");
            DestroyObject(miniMapImageObject2);
        }
        else if (players.Count == 4)
        {
            //Renders mini map
            miniMapRawImage.texture = miniMapRenderTexturePlayer1;
            miniMapCameraPlayer1.targetTexture = miniMapRenderTexturePlayer1;
            // sets location and size

            GameObject player1Object = GameObject.FindGameObjectWithTag("Player1");
            RawImage player1MiniMapRawImage = player1Object.GetComponentInChildren<RawImage>();
            player1MiniMapRawImage.rectTransform.anchoredPosition = new Vector2(8f, 8f);
            player1MiniMapRawImage.rectTransform.sizeDelta = new Vector2(200f, 200f);


            GameObject player4Object = GameObject.FindGameObjectWithTag("Player4");
            Rigidbody childRigidbody4 = player4Object.GetComponentInChildren<Rigidbody>();
            childRigidbody4.tag = "FreeRacingCar4";
            Transform player4Transform = GameObject.FindGameObjectWithTag("FreeRacingCar4").transform;

            if (player1Object != null)
            {
                if (player1MiniMapRawImage != null)
                {
                    Image player4IconImageOnMap1 = CreateMiniMapIconImage(player1MiniMapRawImage.rectTransform, player4IconSprite, Color.yellow);
                    CreateMiniMapIcon(player4IconImageOnMap1, miniMapCameraPlayer1, player4Transform);

                    //// assigning color to the player car
                    //Renderer carRenderer = playerParent.GetComponentInChildren<Renderer>();
                    //if (carRenderer != null)
                    //{
                    //    carRenderer.material.color = playerColors[players.Count - 1];
                    //    Debug.Log("Assigned color " + playerColors[players.Count - 1] + " to player " + players.Count);
                    //}

                    //-----CINEMACHINE STUFF------ assigning cameras to individual player using layers

                    //sets size and anchor
                    //miniMapRawImage.rectTransform.anchoredPosition = new Vector2(8f, 8f);
                    //miniMapRawImage.rectTransform.sizeDelta = new Vector2(200f, 200f);
                    //deletes other mini maps
                    GameObject miniMapImageObject1 = GameObject.Find("MiniMapImage4");
                    DestroyObject(miniMapImageObject1);
                    GameObject miniMapImageObject2 = GameObject.Find("MiniMapImage2");
                    DestroyObject(miniMapImageObject2);
                    GameObject miniMapImageObject3 = GameObject.Find("MiniMapImage3");
                    DestroyObject(miniMapImageObject3);
                }
            }
        }
    }

    private Image CreateMiniMapIconImage(RectTransform miniMapRect, Sprite iconSprite, Color iconColor)
    {
        GameObject iconObject = new GameObject("MiniMapIcon" + players.Count);
        iconObject.tag = "MiniMapIcon" + players.Count;
        iconObject.AddComponent<RectTransform>();
        iconObject.transform.SetParent(miniMapRect, false);
        Image iconImage = iconObject.AddComponent<Image>();
        iconImage.sprite = iconSprite;
        iconImage.rectTransform.sizeDelta = new Vector2(25f, 25f);
        iconImage.color = iconColor; // Adjust icon size as needed
        return iconImage;
    }

    MiniMapIcon CreateMiniMapIcon(Image iconImage, Camera miniMapCamera, Transform playerTransform)
    {
        MiniMapIcon miniMapIcon = iconImage.gameObject.AddComponent<MiniMapIcon>();
        miniMapIcon.player = playerTransform;  // Assign the player's transform to track
        miniMapIcon.icon = iconImage.rectTransform;  // Assign the icon's RectTransform
        miniMapIcon.miniMapCamera = miniMapCamera;  // Assign the correct mini-map camera
        miniMapIcon.miniMapRect = (RectTransform)iconImage.transform.parent;  // Assign the mini-map RectTransform
        return miniMapIcon;
    }
    private void AddTimerUI(Transform canvasTransform)
    {
        // Create a new GameObject for the Timer UI
        GameObject timerObject = new GameObject("TimerUI");
        timerObject.transform.SetParent(canvasTransform, false);

        // Add the Timer script to the Timer UI GameObject
        Timer timer = timerObject.AddComponent<Timer>();

        GameObject timerBackgroundObject = new GameObject("TimerBackground");
        timerBackgroundObject.transform.SetParent(timerObject.transform, false);
        RectTransform timerBackgroundRect = timerBackgroundObject.AddComponent<RectTransform>();
        timerBackgroundRect.sizeDelta = new Vector2(242, 65); // Adjust size as needed
        timerBackgroundRect.anchoredPosition = new Vector2(-847, 445);

        // Add and configure the Image component for the background
        Image timerBackgroundImage = timerBackgroundObject.AddComponent<Image>();
        Color backgroundColor = new Color(0, 0, 0, 0.8f); // Black with 50% transparency
        timerBackgroundImage.color = backgroundColor;

        // Create and set up the timer text
        GameObject timerTextObject = new GameObject("TimerText");
        timerTextObject.transform.SetParent(timerObject.transform, false);
        TextMeshProUGUI timerText = timerTextObject.AddComponent<TextMeshProUGUI>();
        timerText.fontSize = 50;
        timerText.alignment = TextAlignmentOptions.Center;
        timerText.rectTransform.anchoredPosition = new Vector2(-845, 445);
        timerText.text = "00:00"; // Default text
        timer.timerText = timerText;

        // Create and set up the pause menu panel
        GameObject pauseMenuObject = new GameObject("PauseMenuPanel");
        pauseMenuObject.transform.SetParent(timerObject.transform, false);
        RectTransform pauseMenuRect = pauseMenuObject.AddComponent<RectTransform>();
        pauseMenuRect.anchorMin = Vector2.zero;
        pauseMenuRect.anchorMax = Vector2.one;
        pauseMenuRect.sizeDelta = Vector2.zero;
        pauseMenuObject.SetActive(false); // Initially hidden
        timer.pauseMenuPanel = pauseMenuObject;

        // Create and set up the ammo UI elements
        GameObject ammoHolderObject = new GameObject("AmmoHolder");
        ammoHolderObject.transform.SetParent(timerObject.transform, false);
        RectTransform ammoHolderRect = ammoHolderObject.AddComponent<RectTransform>();
        ammoHolderRect.anchoredPosition = new Vector2(0, -200);
        timer.ammoHolder = ammoHolderObject;

        GameObject ammoTextObject = new GameObject("AmmoText");
        ammoTextObject.transform.SetParent(ammoHolderObject.transform, false);
        TextMeshProUGUI ammoText = ammoTextObject.AddComponent<TextMeshProUGUI>();
        ammoText.fontSize = 36;
        ammoText.alignment = TextAlignmentOptions.Center;
        ammoText.text = "0 / 0"; // Default text
        timer.ammoText = ammoText;
        timer.Initialize();

    }

    private void CreatePlacementUI(Transform canvasTransform)
    {
        // Creates placement object to canvas
        GameObject placementObject = new GameObject("PlacementUI" + players.Count);
        placementObject.transform.SetParent(canvasTransform, false);

        // Adds script to placementObject
        PlayerPlacementTracker ppTracker = placementObject.AddComponent<PlayerPlacementTracker>();

        #region Creates Placement
        // Creates and Initalizes a Background
        GameObject placementBgObject = new GameObject("PlacementBackground" + players.Count);
        RectTransform placementBgTransform = placementBgObject.AddComponent<RectTransform>();
        Image placementBgImage = placementBgObject.AddComponent<Image>();
        placementBgObject.transform.SetParent(placementObject.transform, false);

        // Creates and Initalizes a Text
        GameObject placementTextObject = new GameObject("PlacementText" + players.Count);
        RectTransform placementTextTransform = placementTextObject.AddComponent<RectTransform>();
        TextMeshProUGUI placementText = placementTextObject.AddComponent<TextMeshProUGUI>();
        placementTextObject.transform.SetParent(placementBgObject.transform, false);
        #endregion

        #region Creates Lap Counter
        // Creates and Initalizes a Background
        GameObject lapBgObject = new GameObject("LapBackground" + players.Count);
        lapBgObject.SetActive(false);
        RectTransform lapBgTransform = lapBgObject.AddComponent<RectTransform>();
        Image lapBgImage = lapBgObject.AddComponent<Image>();
        lapBgObject.transform.SetParent(placementObject.transform, false);

        // Creates and Initalizes a Text
        GameObject lapTextObject = new GameObject("LapText" + players.Count);
        RectTransform lapTextTransform = lapTextObject.AddComponent<RectTransform>();
        TextMeshProUGUI lapText = lapTextObject.AddComponent<TextMeshProUGUI>();
        lapTextObject.transform.SetParent(lapBgObject.transform, false);
        #endregion

        #region Creates Win Screen
        //Creates and Initalizes Background
        GameObject winBgObject = new GameObject("WinBackground" + players.Count);
        winBgObject.SetActive(false);
        RectTransform winBgTransform = winBgObject.AddComponent<RectTransform>();
        Image winBgImage = winBgObject.AddComponent<Image>();
        winBgObject.transform.SetParent(placementObject.transform, false);

        // Creates and Initalizes a Time Text
        GameObject finishedTimeObject = new GameObject("FinishedTimeText" + players.Count);
        RectTransform finishedTimeTextTransform = finishedTimeObject.AddComponent<RectTransform>();
        TextMeshProUGUI finishedTimeText = finishedTimeObject.AddComponent<TextMeshProUGUI>();
        finishedTimeObject.transform.SetParent(winBgObject.transform, false);

        // Creates and Initalizes a Finished Placement Text
        GameObject finishedPlacementObject = new GameObject("FinishedPlacementText" + players);
        RectTransform finishedPlacementTextTransform = finishedPlacementObject.AddComponent<RectTransform>();
        TextMeshProUGUI finishedPlacementText = finishedPlacementObject.AddComponent<TextMeshProUGUI>();
        finishedPlacementObject.transform.SetParent(winBgObject.transform, false);
        #endregion

        if (players.Count == 1)
        {
            #region Placement UI Initalization
            //Initalizes Background
            placementBgImage.color = new Color(0, 0, 0, 0.8f); // Black with 50% transparency;
            placementBgTransform.sizeDelta = new Vector2(300, 80); // Adjust size as needed
            placementBgTransform.anchoredPosition = new Vector2(790, -480);

            //Initalizes Text
            placementText.fontStyle = FontStyles.Italic;
            placementText.color = new Color(1, 0, 0, 1);
            placementText.fontSize = 35;
            placementText.alignment = TextAlignmentOptions.Center;
            placementTextTransform.sizeDelta = new Vector2(300, 80);
            #endregion

            #region Lap Popup UI Initialization
            //Initalizes Background
            lapBgImage.color = new Color(0, 0, 0, 0.8f); // Black with 50% transparency;
            lapBgTransform.sizeDelta = new Vector2(500, 200); // Adjust size as needed
            lapBgTransform.anchoredPosition = new Vector2(0, 400);

            //Initalizes Text
            lapText.fontStyle = FontStyles.Bold;
            lapText.color = new Color(1, 1, 1, 1);
            lapText.fontSize = 120;
            lapText.alignment = TextAlignmentOptions.Center;
            lapTextTransform.sizeDelta = new Vector2(500, 80);
            #endregion

            #region Win Popup UI Initalization
            //Initializes Background
            winBgImage.color = new Color(0, 0, 0, 1);
            winBgTransform.sizeDelta = new Vector2(1920, 1080);
            winBgTransform.anchoredPosition = new Vector2(0, 0);

            //Initializes Finished Time Text
            finishedTimeText.fontStyle = FontStyles.Bold;
            finishedTimeText.color = new Color(1, 1, 1, 1);
            finishedTimeText.fontSize = 40;
            finishedTimeText.alignment = TextAlignmentOptions.Center;
            finishedTimeTextTransform.sizeDelta = new Vector2(1000, 80);
            finishedTimeTextTransform.anchoredPosition = new Vector2(-500, 300);

            //Initializes Finished Position Text
            finishedPlacementText.fontStyle = FontStyles.Bold;
            finishedPlacementText.color = new Color(1, 1, 1, 1);
            finishedPlacementText.fontSize = 40;
            finishedPlacementText.alignment = TextAlignmentOptions.Center;
            finishedPlacementTextTransform.sizeDelta = new Vector2(1000, 80);
            finishedPlacementTextTransform.anchoredPosition = new Vector2(-500, 350);
            #endregion

            //Initalizes the PlayerPlacementTracker component
            GameObject player1 = GameObject.Find("Player1");

            ppTracker.car = player1.GetComponentInChildren<NewBaseCar>();

            ppTracker.placementText = placementText;

            ppTracker.lapCounterObject = lapBgObject;
            ppTracker.lapText = lapText;

            ppTracker.winScreenObject = winBgObject;
            ppTracker.finishedTimeText = finishedTimeText;
            ppTracker.finishedPlacementText = finishedPlacementText;
            ppTracker.startTime = Time.time;
        }
        else if (players.Count == 2)
        {
            #region Updated UI Properties
            // Updates how the Placement looks for player 1
            GameObject player1 = GameObject.Find("Player1");
            PlayerPlacementTracker player1PPTracker = player1.GetComponentInChildren<PlayerPlacementTracker>();
            GameObject player1PositionBgObject = GameObject.Find("PlacementBackground1");
            RectTransform player1PositionBgTransform = player1PositionBgObject.GetComponent<RectTransform>();
            player1PositionBgTransform.anchoredPosition = new Vector2(-170, -480);

            //Updates how the Lap Counter looks for player 1
            player1PPTracker.lapCounterObject.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 200);
            player1PPTracker.lapCounterObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-500, 300);
            player1PPTracker.lapText.fontSize = 80;

            //Updates how the Win Screen looks for player 1
            player1PPTracker.winScreenObject.GetComponent<RectTransform>().sizeDelta = new Vector2(1920 / 2, 1080);
            player1PPTracker.winScreenObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-480, 0);
            player1PPTracker.finishedTimeText.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            player1PPTracker.finishedPlacementText.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 50);
            player1PPTracker.startTime = Time.time;
            #endregion

            #region Placement UI Initalization
            //Initalizes Background
            placementBgImage.color = new Color(0, 0, 0, 0.8f); // Black with 50% transparency;
            placementBgTransform.sizeDelta = new Vector2(300, 80); // Adjust size as needed
            placementBgTransform.anchoredPosition = new Vector2(790, -480);

            //Initalizes Text
            placementText.fontStyle = FontStyles.Italic;
            placementText.color = new Color(1, 0, 0, 1);
            placementText.fontSize = 35;
            placementText.alignment = TextAlignmentOptions.Center;
            placementTextTransform.sizeDelta = new Vector2(300, 80);
            #endregion

            #region Lap Popup UI Initialization
            //Initalizes Background
            lapBgImage.color = new Color(0, 0, 0, 0.8f); // Black with 50% transparency;
            lapBgTransform.sizeDelta = new Vector2(500, 200); // Adjust size as needed
            lapBgTransform.anchoredPosition = new Vector2(500, 300);

            //Initalizes Text
            lapText.fontStyle = FontStyles.Bold;
            lapText.color = new Color(1, 1, 1, 1);
            lapText.fontSize = 80;
            lapText.alignment = TextAlignmentOptions.Center;
            lapTextTransform.sizeDelta = new Vector2(500, 80);
            #endregion

            #region Win Popup UI Initalization
            //Initializes Background
            winBgImage.color = new Color(0, 0, 0, 1);
            winBgTransform.sizeDelta = new Vector2(1920 / 2, 1080);
            winBgTransform.anchoredPosition = new Vector2(480, 0);

            //Initializes Finished Time Text
            finishedTimeText.fontStyle = FontStyles.Bold;
            finishedTimeText.color = new Color(1, 1, 1, 1);
            finishedTimeText.fontSize = 40;
            finishedTimeText.alignment = TextAlignmentOptions.Center;
            finishedTimeTextTransform.sizeDelta = new Vector2(1000, 80);
            finishedTimeTextTransform.anchoredPosition = new Vector2(0, 0);

            //Initializes Finished Position Text
            finishedPlacementText.fontStyle = FontStyles.Bold;
            finishedPlacementText.color = new Color(1, 1, 1, 1);
            finishedPlacementText.fontSize = 40;
            finishedPlacementText.alignment = TextAlignmentOptions.Center;
            finishedPlacementTextTransform.sizeDelta = new Vector2(1000, 80);
            finishedPlacementTextTransform.anchoredPosition = new Vector2(0, 50);
            #endregion

            //Initalizes the PlayerPlacementTracker component
            GameObject player2 = GameObject.Find("Player2");

            ppTracker.car = player2.GetComponentInChildren<NewBaseCar>();

            ppTracker.placementText = placementText;

            ppTracker.lapCounterObject = lapBgObject;
            ppTracker.lapText = lapText;

            ppTracker.winScreenObject = winBgObject;
            ppTracker.finishedTimeText = finishedTimeText;
            ppTracker.finishedPlacementText = finishedPlacementText;
            ppTracker.startTime = Time.time;
        }
        else if (players.Count == 3)
        {
            #region Updated UI Properties
            // Updates how the Placement looks for player 1
            GameObject player1 = GameObject.Find("Player1");
            PlayerPlacementTracker player1PPTracker = player1.GetComponentInChildren<PlayerPlacementTracker>();
            GameObject player1PositionBgObject = GameObject.Find("PlacementBackground1");
            RectTransform player1PositionBgTransform = player1PositionBgObject.GetComponent<RectTransform>();
            player1PositionBgTransform.sizeDelta = new Vector2(220, 60);
            player1PositionBgTransform.anchoredPosition = new Vector2(-120, 40);
            player1PPTracker.placementText.fontSize = 25;

            //Updates how the Lap Counter looks for player 1
            player1PPTracker.lapCounterObject.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 80);
            player1PPTracker.lapCounterObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-500, 450);
            player1PPTracker.lapText.fontSize = 60;

            //Updates how the Win Screen looks for player 1
            player1PPTracker.winScreenObject.GetComponent<RectTransform>().sizeDelta = new Vector2(1920 / 2, 1080 / 2);
            player1PPTracker.winScreenObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-480, 270);
            player1PPTracker.finishedTimeText.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            player1PPTracker.finishedPlacementText.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 50);
            player1PPTracker.startTime = Time.time;

            // Updates how the Placement looks for player 2
            GameObject player2 = GameObject.Find("Player2");
            PlayerPlacementTracker player2PPTracker = player2.GetComponentInChildren<PlayerPlacementTracker>();
            GameObject player2PositionBgObject = GameObject.Find("PlacementBackground2");
            RectTransform player2PositionBgTransform = player2PositionBgObject.GetComponent<RectTransform>();
            player2PositionBgTransform.sizeDelta = new Vector2(220, 60);
            player2PositionBgTransform.anchoredPosition = new Vector2(840, 40);
            player2PPTracker.placementText.fontSize = 25;

            //Updates how the Lap Counter looks for player 2
            player2PPTracker.lapCounterObject.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 80);
            player2PPTracker.lapCounterObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(500, 450);
            player2PPTracker.lapText.fontSize = 60;

            //Updates how the Win Screen looks for player 1
            player2PPTracker.winScreenObject.GetComponent<RectTransform>().sizeDelta = new Vector2(1920 / 2, 1080 / 2);
            player2PPTracker.winScreenObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(480, 270);
            player2PPTracker.finishedTimeText.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            player2PPTracker.finishedPlacementText.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 50);
            player2PPTracker.startTime = Time.time;
            #endregion

            #region Placement UI Initalization
            //Initalizes Background
            placementBgImage.color = new Color(0, 0, 0, 0.8f); // Black with 50% transparency;
            placementBgTransform.sizeDelta = new Vector2(220, 60); // Adjust size as needed
            placementBgTransform.anchoredPosition = new Vector2(-120, -500);

            //Initalizes Text
            placementText.fontStyle = FontStyles.Italic;
            placementText.color = new Color(1, 0, 0, 1);
            placementText.fontSize = 25;
            placementText.alignment = TextAlignmentOptions.Center;
            placementTextTransform.sizeDelta = new Vector2(200, 80);
            #endregion

            #region Lap Popup UI Initialization
            //Initalizes Background
            lapBgImage.color = new Color(0, 0, 0, 0.8f); // Black with 50% transparency;
            lapBgTransform.sizeDelta = new Vector2(300, 80); // Adjust size as needed
            lapBgTransform.anchoredPosition = new Vector2(-500, -90);

            //Initalizes Text
            lapText.fontStyle = FontStyles.Bold;
            lapText.color = new Color(1, 1, 1, 1);
            lapText.fontSize = 60;
            lapText.alignment = TextAlignmentOptions.Center;
            lapTextTransform.sizeDelta = new Vector2(500, 80);
            #endregion

            #region Win Popup UI Initalization
            //Initializes Background
            winBgImage.color = new Color(0, 0, 0, 1);
            winBgTransform.sizeDelta = new Vector2(1920 / 2, 1080 / 2);
            winBgTransform.anchoredPosition = new Vector2(-480, -270);

            //Initializes Finished Time Text
            finishedTimeText.fontStyle = FontStyles.Bold;
            finishedTimeText.color = new Color(1, 1, 1, 1);
            finishedTimeText.fontSize = 40;
            finishedTimeText.alignment = TextAlignmentOptions.Center;
            finishedTimeTextTransform.sizeDelta = new Vector2(1000, 80);
            finishedTimeTextTransform.anchoredPosition = new Vector2(0, 0);

            //Initializes Finished Position Text
            finishedPlacementText.fontStyle = FontStyles.Bold;
            finishedPlacementText.color = new Color(1, 1, 1, 1);
            finishedPlacementText.fontSize = 40;
            finishedPlacementText.alignment = TextAlignmentOptions.Center;
            finishedPlacementTextTransform.sizeDelta = new Vector2(1000, 80);
            finishedPlacementTextTransform.anchoredPosition = new Vector2(0, 50);
            #endregion

            //Initalizes the PlayerPlacementTracker component
            GameObject player3 = GameObject.Find("Player3");

            ppTracker.car = player3.GetComponentInChildren<NewBaseCar>();

            ppTracker.placementText = placementText;

            ppTracker.lapCounterObject = lapBgObject;
            ppTracker.lapText = lapText;

            ppTracker.winScreenObject = winBgObject;
            ppTracker.finishedTimeText = finishedTimeText;
            ppTracker.finishedPlacementText = finishedPlacementText;
            ppTracker.startTime = Time.time;
        }
        else if (players.Count == 4)
        {
            #region Updated UI Properties
            GameObject player1 = GameObject.Find("Player1");
            PlayerPlacementTracker player1PPTracker = player1.GetComponentInChildren<PlayerPlacementTracker>();
            player1PPTracker.startTime = Time.time;

            GameObject player2 = GameObject.Find("Player2");
            PlayerPlacementTracker player2PPTracker = player2.GetComponentInChildren<PlayerPlacementTracker>();
            player2PPTracker.startTime = Time.time;

            GameObject player3 = GameObject.Find("Player3");
            PlayerPlacementTracker player3PPTracker = player3.GetComponentInChildren<PlayerPlacementTracker>();
            player3PPTracker.startTime = Time.time;
            #endregion

            #region Placement UI Initalization
            // Gets base components
            placementBgImage.color = new Color(0, 0, 0, 0.8f); // Black with 50% transparency;
            placementBgTransform.sizeDelta = new Vector2(220, 60); // Adjust size as needed
            placementBgTransform.anchoredPosition = new Vector2(840, -500);

            //Initalizes Text
            placementText.fontStyle = FontStyles.Italic;
            placementText.color = new Color(1, 0, 0, 1);
            placementText.fontSize = 25;
            placementText.alignment = TextAlignmentOptions.Center;
            placementTextTransform.sizeDelta = new Vector2(200, 80);
            #endregion

            #region Lap Popup UI Initialization
            //Initalizes Background
            lapBgImage.color = new Color(0, 0, 0, 0.8f); // Black with 50% transparency;
            lapBgTransform.sizeDelta = new Vector2(300, 80); // Adjust size as needed
            lapBgTransform.anchoredPosition = new Vector2(500, -90);

            //Initalizes Text
            lapText.fontStyle = FontStyles.Bold;
            lapText.color = new Color(1, 1, 1, 1);
            lapText.fontSize = 60;
            lapText.alignment = TextAlignmentOptions.Center;
            lapTextTransform.sizeDelta = new Vector2(500, 80);
            #endregion

            #region Win Popup UI Initalization
            //Initializes Background
            winBgImage.color = new Color(0, 0, 0, 1);
            winBgTransform.sizeDelta = new Vector2(1920 / 2, 1080 / 2);
            winBgTransform.anchoredPosition = new Vector2(480, -270);

            //Initializes Finished Time Text
            finishedTimeText.fontStyle = FontStyles.Bold;
            finishedTimeText.color = new Color(1, 1, 1, 1);
            finishedTimeText.fontSize = 40;
            finishedTimeText.alignment = TextAlignmentOptions.Center;
            finishedTimeTextTransform.sizeDelta = new Vector2(1000, 80);
            finishedTimeTextTransform.anchoredPosition = new Vector2(0, 0);

            //Initializes Finished Position Text
            finishedPlacementText.fontStyle = FontStyles.Bold;
            finishedPlacementText.color = new Color(1, 1, 1, 1);
            finishedPlacementText.fontSize = 40;
            finishedPlacementText.alignment = TextAlignmentOptions.Center;
            finishedPlacementTextTransform.sizeDelta = new Vector2(1000, 80);
            finishedPlacementTextTransform.anchoredPosition = new Vector2(0, 50);
            #endregion

            //Initalizes the PlayerPlacementTracker component
            GameObject player4 = GameObject.Find("Player4");

            ppTracker.car = player4.GetComponentInChildren<NewBaseCar>();

            ppTracker.placementText = placementText;

            ppTracker.lapCounterObject = lapBgObject;
            ppTracker.lapText = lapText;

            ppTracker.winScreenObject = winBgObject;
            ppTracker.finishedTimeText = finishedTimeText;
            ppTracker.finishedPlacementText = finishedPlacementText;
            ppTracker.startTime = Time.time;
        }
    }

}

/// <summary>
/// Try 2
/// 
/// 
/// </summary>
