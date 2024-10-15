using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLItem : ItemManager
{
    [SerializeField] GameObject projectile;
    override public void Use()
    {
        var bullet = Instantiate(projectile, shootPos.position, shootPos.rotation);
        bullet.GetComponent<Rigidbody>().velocity = shootPos.forward * bulletSpeed; 
        bullet.GetComponent<BulletLogic>().weaponParent = this;
        Debug.Log("Grenade Launcher Used");
        Uses--;
    }
}
