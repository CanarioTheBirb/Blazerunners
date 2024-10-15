using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGItem : ItemManager
{
    [SerializeField] GameObject projectile;
    override public void Use()
    {
        AudioManager.instance.PlaySFX(sound, AudioManager.instance.rocketShoot);
        var bullet = Instantiate(projectile, shootPos.position, shootPos.rotation * projectile.transform.rotation);
        bullet.GetComponent<Rigidbody>().velocity = shootPos.forward * bulletSpeed;
        bullet.GetComponent<RocketLogic>().weaponParent = this;
        Debug.Log("RPG Used");
        Uses--;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(shootPos.position, shootPos.forward);
    }
}
