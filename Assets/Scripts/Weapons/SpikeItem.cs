using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeItem : ItemManager
{
    [SerializeField] GameObject spikes;
    public override void Use()
    {
        var spike = Instantiate(spikes, shootPos.position, shootPos.rotation);
        spike.GetComponentInChildren<SpikeLogic>().weaponParent = this;
        Uses--;
    }
}
