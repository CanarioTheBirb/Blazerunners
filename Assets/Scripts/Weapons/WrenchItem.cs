using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrenchItem : ItemManager
{
    public override void Use()
    {
        AudioManager.instance.PlaySFX(sound,AudioManager.instance.repair);
        car.hp = Mathf.Clamp(car.hp + 50.0f, 0,100);
        Uses--;
    }
}
