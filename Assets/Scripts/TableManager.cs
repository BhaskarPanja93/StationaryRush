using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class TableSlotData
{
    public GameObject slotModel;
    public GameObject productModel;
}

public class TableManager : MonoBehaviour
{
    
    [SerializeField] private GameObject table;
    private List<TableSlotData> _slots;


    private void Awake()
    {
        _slots = new List<TableSlotData>();
        ReadChildrenData();
        Debug.Log("TOTAL TABLE SLOTS: " + _slots.Count);
    }
    
    private void ReadChildrenData()
    {
        foreach (Transform model in table.transform)
        {
            var tableSlot = new TableSlotData
            {
                slotModel = model.gameObject,
                productModel = null,
            };
            _slots.Add(tableSlot);
        }
    }
    
    public TableSlotData GetVacantSlot()
    {
        var vacantSlots = _slots.Where(s => { return s.productModel == null; }).ToList();
        if (vacantSlots.Count == 0) return null;
        return vacantSlots.First();
    }
}
