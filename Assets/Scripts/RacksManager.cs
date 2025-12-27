using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class RackSlotData
{
    public GameObject slotModel;
    public GameObject productModel;
}

public class RacksManager : MonoBehaviour
{    
    
    [SerializeField] private List<GameObject> racks;
    private List<RackSlotData> _slots;
    private System.Random _random;
    
    private void Awake()
    {
        _random = new System.Random();
        _slots = new List<RackSlotData>();
        ReadChildrenData();
        Debug.Log("TOTAL RACK SLOTS: " + _slots.Count);
    }

    private void ReadChildrenData()
    {
        foreach (var rack in racks)
        {
            foreach (Transform slot in rack.transform)
            {
                Debug.Log("RACK SLOT REGISTERED: " + slot.name + " FROM RACK " + rack.name);
                _slots.Add(new RackSlotData
                {
                    slotModel = slot.gameObject,
                    productModel = null,
                });
            }
        }
    }
    
    public RackSlotData GetVacantSlot()
    {
        var vacantSlots = _slots.Where(s => { return s.productModel == null; }).ToList();
        if (vacantSlots.Count == 0) return null;
        var index = _random.Next(vacantSlots.Count);
        return vacantSlots[index];
    }
}
