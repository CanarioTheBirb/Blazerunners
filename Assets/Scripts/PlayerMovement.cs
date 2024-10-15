using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    //External Variables below
    [Header("----- References -----")]
    [SerializeField] CinemachineVirtualCamera playerCam;
    [SerializeField] public NewBaseCar car;
    [SerializeField] PlayerInput playerInput;
    [SerializeField] GameObject aim;
    [SerializeField] bool shot = false;

    //Internal Variables below
    InputAction Drive;
    InputAction Steering;
    InputAction Drift;
    InputAction Pause;
    InputAction Shoot;



    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;

        Drive = playerInput.actions.FindAction("Drive");
        Steering = playerInput.actions.FindAction("Steering");
        Drift = playerInput.actions.FindAction("Drift");
        Pause = playerInput.actions.FindAction("Pause");
        Shoot = playerInput.actions.FindAction("Shoot");

    }

    void FixedUpdate()
    {
        //Driving
        Movement();


        //Pausing
        Paused();
    }

    private void Update()
    {
        //Item Usage & Item Check
        Item();
        
    }
    private void Movement()
    {
        car.moveInput = Drive.ReadValue<float>();
        car.steerInput = Steering.ReadValue<float>();
        car.driftInput = Drift.ReadValue<float>();
        // accelslider.value = Mathf.Abs(car.carVelocityRatio);
    }

    void Item()
    {
        if (shot)
        {
            return;
        }

        float shooting = Shoot.ReadValue<float>();

        if ((Mouse.current.leftButton.wasPressedThisFrame || shooting > 0) && car.item != null)
        {
            car.item.GetComponent<ItemManager>().Use();
            shot = true;
            Invoke(nameof(Pressed), 0.25f);
        }
    }

    void Paused()
    {

    }

    void Pressed()
    {
        shot = false;
    }

}
