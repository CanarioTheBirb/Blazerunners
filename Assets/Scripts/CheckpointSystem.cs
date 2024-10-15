using GameKit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointSystem : MonoBehaviour
{
    GameManager Manager;
    [SerializeField] Color debugColor;

    private void Start()
    {
        Manager = GameObject.FindAnyObjectByType<GameManager>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = debugColor;
        Gizmos.DrawCube(transform.position, transform.GetScale());
    }

    private void OnTriggerEnter(Collider other)
    {
        //Checks if a car went through the checkpoint
       NewBaseCar car = other.transform.parent.GetComponentInChildren<NewBaseCar>();
        if (car != null)
        {
            //adds points to the player or ai GameObject
            Manager.AddPoints(car.transform.parent.gameObject,gameObject); 
        }
    }

}
