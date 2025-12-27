using UnityEngine;

public class ProductData : MonoBehaviour
{
    [SerializeField] public int demand = 1;
    [SerializeField] public int sellPrice = 1;
    [SerializeField] public int buyPrice = 1;
    [SerializeField] public int restockCooldown = 1;
    [SerializeField] public int maxStockCount = 1;
    [SerializeField] public int startingStockCount = 1;
    
    [HideInInspector] public string uniqueName;
    [HideInInspector] public Transform dump;
    [HideInInspector] public int stockCount;
    [HideInInspector] public GameObject model;
    [HideInInspector] public GameObject image;

    
    private void Awake()
    {
        uniqueName = transform.name;
        dump = transform.parent.parent.Find("DUMP");
        model = transform.Find("Model").gameObject;
        image = transform.Find("Image").gameObject;
    }
    
    public GameObject InsertIntoRack(RackSlotData rackSlot)
    {
        if (rackSlot == null) return null;
        var clone = Instantiate(model, dump);
        clone.GetComponent<Aimable>().uniqueName =  uniqueName;
        rackSlot.productModel = clone;
        clone.transform.position = rackSlot.slotModel.transform.position;
        return clone;
    }

    public GameObject InsertIntoTable(TableSlotData tableSlot)
    {
        if (tableSlot == null) return null;
        var clone = Instantiate(model, dump);
        clone.GetComponent<Aimable>().uniqueName =  uniqueName;
        tableSlot.productModel = clone;
        clone.transform.position = tableSlot.slotModel.transform.position;
        return clone;
    }

    public GameObject GetUIImage()
    {
        var clone = Instantiate(image);
        clone.name = uniqueName;
        return clone;
    }
}
