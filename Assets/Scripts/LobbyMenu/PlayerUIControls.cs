using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Diagnostics.Contracts;

public class PlayerUIControls : MonoBehaviour
{
    public GameObject playerCar;
    public GameObject playerCursor;
    public PlayerInput playerInput;
    public Button curButton;

    public bool selected;
    public bool everyoneReady;
    public bool onCharacterChoice = true;

    InputAction Up;
    InputAction Side;
    InputAction Menu;
    InputAction gameStart;


    public bool upPressed = false;
    public bool sidePressed = false;
    public bool menuPressed = false;

    private void Start()
    {
        Up = playerInput.actions.FindAction("Vertical");
        Side = playerInput.actions.FindAction("Horizontal");
        Menu = playerInput.actions.FindAction("Menu");
        gameStart = playerInput.actions.FindAction("Start");

        playerCursor.transform.position = curButton.transform.position;
    }

    private void FixedUpdate()
    {
        Confirm();
        GameStart();
        Movement();

        if(onCharacterChoice)
        UpdateCarColor();
    }

    void Movement()
    {
        if (selected || upPressed || sidePressed)
        {
            return;
        }

        float upDown = Up.ReadValue<float>();
        float side = Side.ReadValue<float>();

        if (upDown > 0)
        {
            if (curButton.navigation.selectOnUp != null)
            {
                curButton = curButton.navigation.selectOnUp.gameObject.GetComponent<Button>();
                playerCursor.transform.position = curButton.transform.position;
                upPressed = true;
            }
        }
        else if (upDown < 0)
        {
            if (curButton.navigation.selectOnDown != null)
            {
                curButton = curButton.navigation.selectOnDown.gameObject.GetComponent<Button>();
                playerCursor.transform.position = curButton.transform.position;
                upPressed = true;
            }

        }

        if (side > 0)
        {
            if (curButton.navigation.selectOnRight != null)
            {
                curButton = curButton.navigation.selectOnRight.gameObject.GetComponent<Button>();
                playerCursor.transform.position = curButton.transform.position;
                sidePressed = true;
            }
        }
        else if (side < 0)
        {
            if (curButton.navigation.selectOnLeft != null)
            {
                curButton = curButton.navigation.selectOnLeft.gameObject.GetComponent<Button>();
                playerCursor.transform.position = curButton.transform.position;
                sidePressed = true;
            }
        }

        if (upPressed || sidePressed)
        {
            Invoke(nameof(UnPressed), 0.25f);
        }
    }

    void UpdateCarColor()
    {
        var color = curButton.gameObject.GetComponent<Image>().color;
        playerCar.GetComponentInChildren<MeshRenderer>().material.color = color;
    }

    void Confirm()
    {
        if (menuPressed)
        {
            return;
        }

        float con = Menu.ReadValue<float>();

        if (con > 0)
        {
            selected = true;
            menuPressed = true;
            if (!onCharacterChoice)
            {
                curButton.onClick.Invoke();
            }
        }
        else if (selected && con < 0)
        {
            selected = false;
            menuPressed = true;
        }
        else if (!selected && con < 0)
        {
            Destroy(PlayerCarrier.instance.gameObject);
            SceneManager.LoadScene("MainMenu");
        }

        if (menuPressed)
        {
            Invoke(nameof(UnPressed), 0.25f);
        }
    }
    void GameStart()
    {
        if (everyoneReady)
        {
            float game = gameStart.ReadValue<float>();

            if (game > 0)
            {
                PlayerCarrier.instance.ColorSelectEnd();
                SceneManager.LoadScene("MultiplayerCourseSelect");
            }
        }
    }

    void UnPressed()
    {
        upPressed = false;
        sidePressed = false;
        menuPressed = false;
    }
}
