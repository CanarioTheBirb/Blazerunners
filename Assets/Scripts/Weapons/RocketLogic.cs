using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLogic : MonoBehaviour
{
    public ItemManager weaponParent;
    [SerializeField] GameObject explosion;
    [SerializeField] AudioSource sound;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.GetComponent<CheckpointSystem>())
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        Destroy(gameObject, 3.0f);
    }
    private void OnDestroy()
    {
        var boom = Instantiate(explosion, transform.position, transform.rotation);
        boom.GetComponent<Explosion>().damage = weaponParent.damage;
        AudioManager.instance.PlaySFX(sound, AudioManager.instance.rocketBoom);
    }
}
