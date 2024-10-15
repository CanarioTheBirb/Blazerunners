 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject Player;  // Reference to the player gameobject
    public GameObject CameraConstraint;
    public float lerpspeed = 5f;  // Lerp Speed of the camera movement

    private void Awake()
    {

    }

    private void FixedUpdate()
    {
        FollowPlayer();
    }

    private void FollowPlayer()
    {
        gameObject.transform.position = Vector3.Lerp(transform.position, CameraConstraint.transform.position, Time.deltaTime * lerpspeed);
        gameObject.transform.LookAt(Player.gameObject.transform.position);
    }
}
