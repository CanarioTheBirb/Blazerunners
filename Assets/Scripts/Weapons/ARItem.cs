using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARItem : ItemManager
{
    [SerializeField] GameObject projectile;
    override public void Use()
    {
        AudioManager.instance.PlaySFX(sound, AudioManager.instance.rifleShot);
        var bullet = Instantiate(projectile, shootPos.position, shootPos.rotation);
        bullet.GetComponent<Rigidbody>().velocity = shootPos.forward * bulletSpeed;
        bullet.GetComponent<BulletLogic>().weaponParent = this;
        Debug.Log("Assault Rifle Used");
        Uses--;
    }
}
