using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LMGItem : ItemManager
{
    [SerializeField] GameObject projectile;
    override public void Use()
    {
        AudioManager.instance.PlaySFX(sound,AudioManager.instance.lmgShot);
        var bullet = Instantiate(projectile, shootPos.position,shootPos.rotation);
        bullet.GetComponent<Rigidbody>().velocity = shootPos.forward * bulletSpeed;
        bullet.GetComponent<BulletLogic>().weaponParent = this;
        Debug.Log("LMG Used");
        Uses--;
    }
}
