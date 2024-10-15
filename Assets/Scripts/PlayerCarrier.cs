using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCarrier : MonoBehaviour
{
    public static PlayerCarrier instance = null;

    [SerializeField] public PlayerInputManager players;
    [SerializeField] public List<PlayerManager> playerCars;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
    }

    public void ColorSelectEnd()
    {
        foreach (var player in FindObjectsByType<PlayerManager>(FindObjectsSortMode.InstanceID))
        {
            player.transform.SetParent(this.transform);
            player.uiControls.onCharacterChoice = false;
            player.uiControls.selected = false;
            player.carRb.useGravity = false;
            playerCars.Add(player);
        }
        playerCars.Reverse();
    }
}
