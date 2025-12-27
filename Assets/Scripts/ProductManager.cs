using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class ProductManager : MonoBehaviour
{
    
    private Dictionary<string, ProductData> _products;
    private System.Random _random;
    
    private void Awake()
    {
        _random = new System.Random();
        _products = new Dictionary<string, ProductData>();
        ReadChildrenData();
        Debug.Log("TOTAL PRODUCTS: " + _products.Count);
    }

    private void ReadChildrenData()
    {
        foreach (Transform product in transform)
        {
            _products.Add(product.name, product.GetComponent<ProductData>());
            Debug.Log("PRODUCT REGISTERED: " + product.name);
        }
    }

    public ProductData FetchData(string productName)
    {
        return _products.GetValueOrDefault(productName, _products.First().Value).GetComponent<ProductData>();
    }
    
    public Dictionary<string,ProductData> FetchAll()
    {
        return _products;
    }
    
    public string FetchRandom()
    {
        var totalDemand = _products.Sum(pair => pair.Value.demand);
        var roll = _random.Next(0, totalDemand);
        foreach (var pair in _products)
        {
            roll -= pair.Value.demand;
            if (roll < 0)
            {
                return pair.Key;
            }
        }
        return _products.First().Key;
    }
    
    public bool Restock(string uniqueName, int count, bool replace)
    {
        var data = FetchData(uniqueName);
        if (replace) 
        {
            data.stockCount = count;
            Debug.Log("PRODUCT RESTOCKED: " + count + " " + uniqueName);
            return true;
        }
        if (data.stockCount < data.maxStockCount)
        {
            data.stockCount += count;
            Debug.Log("PRODUCT RESTOCKED: " + count + " " + uniqueName);
            return true;
        }
        return false;
    }

    public bool Consume(string uniqueName)
    {
        var data = FetchData(uniqueName);
        if (data.stockCount > 0)
        {
            data.stockCount--;
            Debug.Log("PRODUCT CONSUMED: " + uniqueName);
            return true;
        }
        return false;
    }
}
