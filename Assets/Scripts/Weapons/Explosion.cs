using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float damage;
    [SerializeField] SphereCollider sphere;
    [SerializeField] float maxRadius;
    private void Update()
    {
        if (sphere.radius <= maxRadius)
        {
            sphere.radius += 2 * Time.deltaTime;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null)
        {
            if (other.transform.parent.TryGetComponent(out NewBaseCar car))
            {
               car.hp -= damage;
            }
        }
    }
}
