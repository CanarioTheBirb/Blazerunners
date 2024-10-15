using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemManager : MonoBehaviour
{
    [Header("---- Stats ----")]
    [SerializeField] public float damage;
    [SerializeField] public float bulletSpeed;
    [SerializeField] public int Uses;
    [SerializeField] public bool crosshair;
    [SerializeField] public Transform shootPos;
    [SerializeField] public Image icon;
    [SerializeField] public AudioSource sound;

    public NewBaseCar car { get; private set; }

    private void Start()
    {
        car = transform.parent.parent.parent.GetComponent<NewBaseCar>();
        if (shootPos == null && car != null)
        {
            shootPos = car.transform;
        }

        if (transform.parent.TryGetComponent(out AudioSource source))
        {
            sound = source;
        }
        else
        {
            Debug.Log("Missing Audio Source");

        }
    }
    virtual public void Use()
    {
        Debug.Log("Item Used");
    }
    private void Update()
    {
        if (Uses <= 0)
        {
            Destroy(gameObject);
        }
    }
}
