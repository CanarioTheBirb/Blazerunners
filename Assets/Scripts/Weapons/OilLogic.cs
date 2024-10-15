using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OilLogic : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject.transform.parent.gameObject, 7.0f);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.parent != null)
        {
            if (other.transform.parent.TryGetComponent(out NewBaseCar car))
            {
                car.ActiveSlow();
            }
        }
    }
}
