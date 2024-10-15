using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletLogic : MonoBehaviour
{
    public ItemManager weaponParent;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.GetComponent<CheckpointSystem>())
        {
            if (other.transform.parent != null)
            {
                if (other.transform.parent.TryGetComponent(out NewBaseCar car))
                {
                    car.hp -= weaponParent.damage;
                }
            }
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        Destroy(gameObject, 3.0f);
    }
}
