using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class MysteryBoxPickup : MonoBehaviour
{
    [SerializeField] List<GameObject> items;
    [SerializeField] public AudioSource sound;

    [Header("---- Components ----")]
    [SerializeField] public GameObject mesh;
    [SerializeField] public BoxCollider box;


    void Start()
    {
        StartCoroutine(floatingObjects());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null)
        {
            var car = other.transform.parent.GetComponentInChildren<NewBaseCar>();
            if (car)
            {
                AudioManager.instance.PlaySFX(sound, AudioManager.instance.itemPickup);
                GiveItem(car);
                mesh.SetActive(false);
                box.enabled = false;
                Invoke(nameof(TurnOn), 3.0f);
            }
        }
    }
    private IEnumerator floatingObjects()
    {
        while (true)
        {
            transform.Rotate(Vector3.up, 60 * Time.deltaTime, Space.World);
            yield return null;
        }
    }
    void TurnOn()
    {
        mesh.SetActive(true);
        box.enabled = true;
    }
    void GiveItem(NewBaseCar car)
    {
        Debug.Log("Gave Item");
        if (car.item == null)
        {
            car.item = Instantiate(items[Random.Range(0, items.Count)], car.itemPlace);
        }
    }
}
