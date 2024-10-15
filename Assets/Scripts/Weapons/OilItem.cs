using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OilItem : ItemManager
{
    [SerializeField] GameObject oils;
    public override void Use()
    {
        Instantiate(oils, shootPos.position,shootPos.rotation);
        Uses--;
    }
}
