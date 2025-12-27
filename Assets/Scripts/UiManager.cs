using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    
    [SerializeField] private GameObject ui;

    private Image _orderTimerComponent;
    private Image _restockTimerComponent;
    private Transform _iconObject;
    private Transform _ordersElement;
    private Transform _gameOverObject;
    private Transform _orderListObject;
    private Transform _productDetailsObject;
    private Transform _daySummaryObject;
    private TMP_Text _currencyComponent;
    private TMP_Text _daySummaryHeaderComponent;
    private TMP_Text _goodsSoldComponent;
    private TMP_Text _bonusComponent;
    private TMP_Text _goodsPurchasedComponent;
    private TMP_Text _electricityComponent;
    private TMP_Text _rentComponent;
    private TMP_Text _netComponent;
    private TMP_Text _quantityComponent;
    private TMP_Text _buyPriceComponent;
    private TMP_Text _sellPriceComponent;
    private TMP_Text _clockComponent;
    private TMP_Text _gameOverComponent;
    
    private string _currentlyDisplayedProduct;
    private bool _npcTimerRunning;
    private int _npcTimeTotal;
    private float _npcTimeElapsed;


    private void Awake()
    {
        _clockComponent = ui.transform.Find("Clock").GetComponent<TMP_Text>();
        
        _currencyComponent = ui.transform.Find("Wallet").Find("Amount").GetComponent<TMP_Text>();
        
        _ordersElement = ui.transform.Find("Orders");
        _orderListObject = _ordersElement.Find("Items");
        _orderTimerComponent = _ordersElement.Find("Timer").Find("Progress").GetComponent<Image>();
        
        _productDetailsObject = ui.transform.Find("ItemDescription");
        _iconObject = _productDetailsObject.Find("Icon");
        _quantityComponent = _productDetailsObject.Find("QTY").GetComponent<TMP_Text>();
        _buyPriceComponent = _productDetailsObject.Find("BP").GetComponent<TMP_Text>();
        _sellPriceComponent = _productDetailsObject.Find("SP").GetComponent<TMP_Text>();
        _restockTimerComponent = _productDetailsObject.Find("Timer").Find("Progress").GetComponent<Image>();
        
        _daySummaryObject = ui.transform.Find("DaySummary");
        _daySummaryHeaderComponent = _daySummaryObject.Find("Header").GetComponent<TMP_Text>();
        _goodsSoldComponent = _daySummaryObject.Find("Earning").Find("GoodsSoldValue").GetComponent<TMP_Text>();
        _bonusComponent = _daySummaryObject.Find("Earning").Find("BonusValue").GetComponent<TMP_Text>();
        _goodsPurchasedComponent = _daySummaryObject.Find("Expenditure").Find("GoodsPurchasedValue").GetComponent<TMP_Text>();
        _electricityComponent = _daySummaryObject.Find("Expenditure").Find("ElectricityBillValue").GetComponent<TMP_Text>();
        _rentComponent = _daySummaryObject.Find("Expenditure").Find("RentValue").GetComponent<TMP_Text>();
        _netComponent = _daySummaryObject.Find("NetAmount").GetComponent<TMP_Text>();
        
        _gameOverObject = ui.transform.Find("GameOver");
        _gameOverComponent = _gameOverObject.GetComponent<TMP_Text>();
    }
    
    private Color ProgressColor(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
    
        float r = ratio < 0.5f ? 1f : 1f - (ratio - 0.5f) * 2f;
        float g = ratio < 0.5f ? ratio * 2f : 1f;
        float b = 0f;

        return new Color(r, g, b);
    }

    public void ShowProductDetails(ProductData product, bool force)
    {
        if (_currentlyDisplayedProduct != product.uniqueName || force)
        {
            _currentlyDisplayedProduct = product.uniqueName;
            foreach(Transform child in _iconObject.transform)
                Destroy(child.gameObject);
            var newIcon = product.GetUIImage();
            newIcon.transform.SetParent(_iconObject);
            newIcon.transform.localScale = Vector3.one;
            _quantityComponent.SetText(product.stockCount + " / " + product.maxStockCount);
            _buyPriceComponent.SetText(product.buyPrice.ToString());
            _sellPriceComponent.SetText(product.sellPrice.ToString());
            _productDetailsObject.gameObject.SetActive(true);
        }
    }

    public void HideProductDetails()
    {
        _currentlyDisplayedProduct = "";
        _productDetailsObject.gameObject.SetActive(false);
    }

    public void ClearOrderItem()
    {
        foreach (Transform child in _orderListObject)
            Destroy(child.gameObject);
    }
    
    public void AddOrderItem(ProductData product)
    {
        var newIcon = product.GetUIImage();
        newIcon.transform.SetParent(_orderListObject);
        newIcon.transform.localScale = Vector3.one;
    }

    public void RemoveOrderItem(string uniqueName)
    {
        var item = _orderListObject.Find(uniqueName);
        if(item != null)
            Destroy(item.gameObject);
    }

    public void StartOrderCountdown(int countdown)
    {
        _ordersElement.gameObject.SetActive(true);
        _npcTimeTotal = countdown;
        _npcTimeElapsed = 0;
        _npcTimerRunning = true;
    }
    
    public void StopOrderCountdown()
    {
        _npcTimerRunning = false;
    }
    
    public float OrderTimeRemainingRatio()
    {
        return 1 - _npcTimeElapsed/_npcTimeTotal;
    }
    
    public void HideOrderElements()
    {
        _ordersElement.gameObject.SetActive(false);
    }

    public void UpdateRestockTimer(float ratio)
    {
        _restockTimerComponent.fillAmount = ratio;
        _restockTimerComponent.color = ProgressColor(ratio);
    }

    public bool ShowDaySummary(int day, int goodsPurchased, int electricitySpent, int rentSpent, int goodsSold, int bonus)
    {
        _daySummaryHeaderComponent.SetText("Day " + day + " Summary");
        _goodsSoldComponent.SetText(goodsSold.ToString());
        _bonusComponent.SetText(bonus.ToString());
        _goodsPurchasedComponent.SetText(goodsPurchased.ToString());
        _electricityComponent.SetText(electricitySpent.ToString());
        _rentComponent.SetText(rentSpent.ToString());
        var profit = goodsSold - (goodsPurchased + electricitySpent + rentSpent);
        _netComponent.SetText(profit.ToString());
        _netComponent.color = profit > 0 ? Color.green:Color.red;
        _daySummaryObject.gameObject.SetActive(true);
        return profit > 0;
    }

    public void HideDaySummary()
    {
        _daySummaryObject.gameObject.SetActive(false);
    }

    public void UpdateTime(DateTime time)
    {
        _clockComponent.SetText(time.ToString("hh:mm tt"));
    }

    public void UpdateCurrency(int value)
    {
        _currencyComponent.SetText(value.ToString());
    }
    
    public void ShowGameOver(int days)
    {
        _gameOverObject.gameObject.SetActive(true);
        _gameOverComponent.SetText("You survived " + days + " days");
    }
    
    public void HideGameOver()
    {
        _gameOverObject.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!_npcTimerRunning) return;
        _npcTimeElapsed += 1000 * Time.deltaTime;
        var ratio = 1 - _npcTimeElapsed / _npcTimeTotal;
        _orderTimerComponent.fillAmount = ratio;
        _orderTimerComponent.color = ProgressColor(ratio);
    }
}
