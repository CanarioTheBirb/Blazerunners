using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [Header("------ Controls -------")]
    [SerializeField] public PlayerUIControls uiControls;
    [SerializeField] public PlayerMovement gameplayControls;
    [SerializeField] public PlayerInput uiInput;
    [SerializeField] public PlayerInput gameplayInput;

    [Header("------ Components ------")]
    [SerializeField] public Camera playerCam;
    [SerializeField] public Rigidbody carRb;


    private void Start()
    {
        gameplayInput.enabled = false;
        playerCam.enabled = false;
    }

    public void SwitchtoGameplay()
    {
        uiControls.enabled = false;
        uiInput.enabled = false;
        gameplayControls.enabled = true;
        carRb.useGravity = true;
        gameplayInput.enabled = true;
        playerCam.enabled = true;
    }
}
