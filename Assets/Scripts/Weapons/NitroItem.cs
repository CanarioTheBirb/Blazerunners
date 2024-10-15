using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NitroItem : ItemManager
{
    public override void Use()
    {
        car.isBoosting = true;
        AudioManager.instance.PlaySFX(sound, AudioManager.instance.boost);
        Invoke(nameof(EndBoost), 1f);
    }
    void EndBoost()
    {
        car.isBoosting = false;
        Uses--;
    }
}
