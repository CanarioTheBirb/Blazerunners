using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapIcon : MonoBehaviour
{
    public Transform player;            // Reference to the player's transform
    public RectTransform icon;          // Reference to the RectTransform of the icon
    public Camera miniMapCamera;        // Reference to the mini-map camera
    public RectTransform miniMapRect;   // Reference to the mini-map RectTransform
    SSPlayerManager ssPlayersInsance;
    Transform player2;
    Transform player3;
    Transform player4;
    private void Awake()
    {
        //icon = GameObject.Find("MiniMapIcon1");
        player = GameObject.FindWithTag("FreeRacingCar1")?.transform;
        if(player != null )
        {
            player2 = GameObject.FindWithTag("FreeRacingCar2")?.transform;
            if(player2 != null )
            {
                player3 = GameObject.FindWithTag("FreeRacingCar3")?.transform;
                if (player3 != null)
                {
                    player4 = GameObject.FindWithTag("FreeRacingCar4")?.transform;
                }
            }
        }
    }
    void Update()
    {
      
        if (player != null)
        {
            Vector2 miniMapPos = MiniMapPosition(player.position);
            icon.anchoredPosition = ClampToMiniMap(miniMapPos);
        }
        if (player2 != null)
        {
            //player2 = GameObject.FindWithTag("FreeRacingCar2")?.transform;
            Vector2 miniMapPos2 = MiniMapPosition(player2.position);
            icon.anchoredPosition = ClampToMiniMap(miniMapPos2);
        }
        if (player3 != null)
        {
            //player3 = GameObject.FindWithTag("FreeRacingCar3")?.transform;
            Vector2 miniMapPos3 = MiniMapPosition(player3.position);
            icon.anchoredPosition = ClampToMiniMap(miniMapPos3);
        }
        if (player4 != null)
        {
            //player4 = GameObject.FindWithTag("FreeRacingCar4")?.transform;
            Vector2 miniMapPos4 = MiniMapPosition(player4.position);
            icon.anchoredPosition = ClampToMiniMap(miniMapPos4);
        }

        //Vector2 miniMapPos2 = MiniMapPosition(player2.position);
        //icon.anchoredPosition = ClampToMiniMap(miniMapPos2);
    }
    Vector2 MiniMapPosition(Vector3 worldPos)
    {
        // Convert world position to viewport position of the mini-map camera
        Vector3 viewportPos = miniMapCamera.WorldToViewportPoint(worldPos);

        // Convert the viewport position to mini-map coordinates
        float x = (viewportPos.x * miniMapRect.sizeDelta.x) - (miniMapRect.sizeDelta.x * 0.5f);
        float y = (viewportPos.y * miniMapRect.sizeDelta.y) - (miniMapRect.sizeDelta.y * 0.5f);

        return new Vector2(x, y);
    }

    Vector2 ClampToMiniMap(Vector2 position)
    {
        // Clamp the position within the mini-map RectTransform bounds
        float halfWidth = miniMapRect.sizeDelta.x * 0.5f;
        float halfHeight = miniMapRect.sizeDelta.y * 0.5f;

        float clampedX = Mathf.Clamp(position.x, -halfWidth, halfWidth);
        float clampedY = Mathf.Clamp(position.y, -halfHeight, halfHeight);

        return new Vector2(clampedX, clampedY);
    }
  
}
