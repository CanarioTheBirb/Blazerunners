using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeLogic : MonoBehaviour
{
    public ItemManager weaponParent;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject.transform.parent.gameObject, 6.0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null)
        {
            if (other.transform.parent.TryGetComponent(out NewBaseCar car))
            {
                car.hp -= weaponParent.damage;
                car.ActiveSlow();
                Destroy(gameObject.transform.parent.gameObject);
            }
        }
    }
}
