using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using StarterAssets;
using UnityEngine;
using Newtonsoft.Json;

[Serializable]
public class GameData {
    public Dictionary<string, int> Data = new Dictionary<string, int>();
}


public class MainScript : MonoBehaviour
{
    [SerializeField] private StarterAssetsInputs starterAssetsInputs;
    [SerializeField] private GameObject playerEye;

    private System.Random _random;
    private static string _savePath;

    private Transform _playerEyeSpot;
    private AudioManager _audioManager;
    private RacksManager _racksManager;
    private ProductManager _productManager;
    private NpcManager _npcManager;
    private TableManager _tableManager;
    private UiManager _uiManager;
    private TimeManager _timeManager;
    private CurrencyManager _currencyManager;
    
    private NpcData _activeNpc;
    private string _currentlyRestocking;
    private Aimable _currentlyHovering;
    private string _currentRestockId;


    private void Start()
    {
        _currentlyRestocking = "";
        _currentlyHovering = null;
        _currentRestockId = "";
        _random = new System.Random();
        _savePath = Path.Combine(Application.persistentDataPath, "StationaryRushSavee.json");
        _audioManager = transform.GetComponentInChildren<AudioManager>();
        _racksManager = transform.GetComponentInChildren<RacksManager>();
        _productManager = transform.GetComponentInChildren<ProductManager>();
        _npcManager = transform.GetComponentInChildren<NpcManager>();
        _tableManager = transform.GetComponentInChildren<TableManager>();
        _uiManager = transform.GetComponentInChildren<UiManager>();
        _timeManager = transform.GetComponentInChildren<TimeManager>();
        _currencyManager = transform.GetComponentInChildren<CurrencyManager>();
        
        RegisterListeners();
        FillRacks();
        GameLoop();
    }

    private Aimable GetAimedProduct()
    {
        RaycastHit hit;

        if (Physics.Raycast(playerEye.transform.position, playerEye.transform.forward, out hit, 50f))
            return hit.transform.GetComponent<Aimable>();
        return null;
    }
    
    private void RegisterListeners()
    {
        starterAssetsInputs.ListenToClick(_ =>
        {
            var hit = GetAimedProduct();
            if (hit != null)
            {
                hit.Clicked();
                var data = _productManager.FetchData(hit.uniqueName);
                _uiManager.ShowProductDetails(data, true);
                _uiManager.UpdateRestockTimer((float)data.stockCount/data.maxStockCount);
            }
        });
        starterAssetsInputs.ListenToMouseMove(_ =>
        {
            var hit = GetAimedProduct();
            if (hit != _currentlyHovering && _currentlyHovering != null)
                _currentlyHovering.UnHovered();
            if (hit != null)
            {
                _currentlyHovering = hit;
                hit.Hovered();
                var data = _productManager.FetchData(hit.uniqueName);
                if (_currentlyRestocking != hit.uniqueName)
                    _uiManager.UpdateRestockTimer((float)data.stockCount / data.maxStockCount);
                _uiManager.ShowProductDetails(data, false);
            }
            else
            {
                _uiManager.HideProductDetails();
            }
        });
        starterAssetsInputs.ListenToPlayerMove(_ =>
        {
            var hit = GetAimedProduct();
            if (hit != _currentlyHovering && _currentlyHovering != null)
                _currentlyHovering.UnHovered();
            if (hit != null)
            {
                _currentlyHovering = hit;
                hit.Hovered();
                var data = _productManager.FetchData(hit.uniqueName);
                if (_currentlyRestocking != hit.uniqueName)
                    _uiManager.UpdateRestockTimer((float)data.stockCount / data.maxStockCount);
                _uiManager.ShowProductDetails(data, false);
            }
            else
                _uiManager.HideProductDetails();
        });
        starterAssetsInputs.ListenToRestock(async v =>
        {
            if (v)
            {
                var hit = GetAimedProduct();
                if (hit == null || !hit.restockable) return;
                var holdTimer = 0f;
                var holdId = Guid.NewGuid().ToString();
                _currentlyRestocking = hit.uniqueName;
                _currentRestockId = holdId;
                do
                {
                    hit = GetAimedProduct();
                    var data = _productManager.FetchData(_currentlyRestocking);
                    if (hit == null || data.stockCount >= data.maxStockCount || data.buyPrice > _currencyManager.currentBalance)
                    {
                        _currentlyRestocking = "";
                        _currentRestockId = "";
                        _uiManager.UpdateRestockTimer((float)data.stockCount/data.maxStockCount);
                        return;
                    }
                    holdTimer += Time.deltaTime;
                    if (holdTimer > data.restockCooldown)
                    {
                        _currencyManager.goodsPurchasedAmount += data.buyPrice;
                        _currencyManager.currentBalance -= data.buyPrice;
                        _uiManager.UpdateCurrency(_currencyManager.currentBalance);
                        _audioManager.PlayRestock();
                        _productManager.Restock(_currentlyRestocking, 1, false);
                        _uiManager.ShowProductDetails(data, true);
                        holdTimer %= data.restockCooldown;
                    }
                    data = _productManager.FetchData(_currentlyRestocking);
                    _uiManager.UpdateRestockTimer((data.stockCount + holdTimer / data.restockCooldown)/data.maxStockCount);
                    await Task.Yield();
                } while (_currentlyRestocking == hit.uniqueName && _currentRestockId == holdId);
            }
            else
            {
                if (_currentlyRestocking != "")
                {
                    _currentRestockId = "";
                    var data = _productManager.FetchData(_currentlyRestocking);
                    _uiManager.ShowProductDetails(data, true);
                    _uiManager.UpdateRestockTimer((float)data.stockCount/data.maxStockCount);
                    _currentlyRestocking = "";
                }
            }
        });
        _timeManager.ListenToTimeChange(time => _uiManager.UpdateTime(time));
    }
    
    private void FillRacks()
    {
        foreach (var (productName, data) in _productManager.FetchAll())
        {
            var rackVacantSlot = _racksManager.GetVacantSlot();
            if (rackVacantSlot == null)
                return;
            var rackProductModel = data.InsertIntoRack(rackVacantSlot);
            if (rackProductModel == null)
                return;
            var rackModelAimable = rackProductModel.GetComponent<Aimable>();
            rackModelAimable.restockable = true;
            rackModelAimable.OnClickAction = () =>
            {
                var vacantTableSlot = _tableManager.GetVacantSlot();
                if (vacantTableSlot == null)
                {
                    _audioManager.PlayInvalid();
                    return;
                }
                var consumed = _productManager.Consume(productName);
                if (!consumed)
                {
                    _audioManager.PlayInvalid();
                    return;
                }
                _audioManager.PlayRackItemClick();
                var tableProductModel = data.InsertIntoTable(vacantTableSlot);
                var tableModelAimable = tableProductModel.GetComponent<Aimable>();
                tableModelAimable.restockable = false;
                tableModelAimable.OnClickAction = () =>
                {
                    if (_activeNpc != null && _npcManager.GiveItem(data.uniqueName, _activeNpc))
                    {
                        _audioManager.PlayTableItemClick();
                        _uiManager.RemoveOrderItem(data.uniqueName);
                        var bonus = (int)(data.sellPrice * _uiManager.OrderTimeRemainingRatio() / 2f);
                        _currencyManager.bonusAmount += bonus;
                        _currencyManager.currentBalance += bonus;
                        _currencyManager.goodsSoldAmount += data.sellPrice;
                        _currencyManager.currentBalance += data.sellPrice;
                        _uiManager.UpdateCurrency(_currencyManager.currentBalance);
                        Destroy(tableProductModel);
                        if (_activeNpc.demands.Count == 0)
                            _activeNpc.Cts.Cancel();
                    }
                    else
                    {
                        _audioManager.PlayInvalid();
                    }
                };
            };
        }
    }

    private void StartNew()
    {
        File.Delete(_savePath);
    }
    
    private void SaveGame()
    {
        var data = new GameData();
        data.Data["__currentDay"] = _timeManager.CurrentDay;
        data.Data["__currentBalance"] = _currencyManager.currentBalance;
        foreach (var pair in _productManager.FetchAll())
            data.Data[pair.Key] = pair.Value.stockCount;
        string json = JsonConvert.SerializeObject(data);
        File.WriteAllText(_savePath, json);
    }

    private void LoadGame()
    {
        if (File.Exists(_savePath))
        {
            var json = File.ReadAllText(_savePath);
            var data = JsonConvert.DeserializeObject<GameData>(json);
            if (data.Data != null)
            {
                foreach (var pair in data.Data)
                {
                    if (pair.Key == "__currentDay")
                    {
                        _timeManager.CurrentDay = pair.Value;
                    }
                    else if (pair.Key == "__currentBalance")
                    {
                        _currencyManager.currentBalance = pair.Value;
                        _uiManager.UpdateCurrency(_currencyManager.currentBalance);
                    }
                    else
                    {
                        _productManager.Restock(pair.Key, pair.Value, true);
                    }
                }
                return;
            }
        }
        foreach (var pair in _productManager.FetchAll())
        {
            _productManager.Restock(pair.Key, _productManager.FetchData(pair.Key).startingStockCount, true);
        }

        _timeManager.CurrentDay = 1;
        _currencyManager.currentBalance = _currencyManager.startingBalance;
        _uiManager.UpdateCurrency(_currencyManager.currentBalance);
        SaveGame();
    }

    private async void GameLoop()
    {
        _activeNpc = null;
        _uiManager.HideGameOver();
        _uiManager.HideDaySummary();
        _uiManager.HideOrderElements();
        LoadGame();
        _timeManager.SetSpeed(_timeManager.nightSpeedMultiplier);
        while(!_timeManager.IsShopOpen())
            await Task.Delay(1000); 
        _timeManager.SetSpeed(_timeManager.daySpeedMultiplier);
        while (true)
        {
            await Task.Delay(1000);
            var demands = Enumerable.Range(0,_random.Next(1,5)).Select(_ => _productManager.FetchRandom()).ToList();
            _activeNpc = _npcManager.InviteNpc(demands);
            await _npcManager.WalkToTable(_activeNpc);
            foreach (var demand in demands)
                _uiManager.AddOrderItem(_productManager.FetchData(demand));
            var npcCountdown = demands.Count*_random.Next(5000,8000)+2000;
            _uiManager.StartOrderCountdown(npcCountdown);
            try { await Task.Delay(npcCountdown, _activeNpc.Cts.Token); }
            catch (TaskCanceledException) { }
            _uiManager.StopOrderCountdown();
            await _npcManager.WalkAwayFromTable(_activeNpc);
            _activeNpc = null;
            _uiManager.ClearOrderItem();
            _uiManager.HideOrderElements();
            if (!_timeManager.IsShopOpen())
            {
                _timeManager.SetSpeed(_timeManager.nightSpeedMultiplier);
                _currencyManager.currentBalance -= _currencyManager.electricitySpentAmount + _currencyManager.rentalSpentAmount;
                _uiManager.UpdateCurrency(_currencyManager.currentBalance);
                var hadProfit = _uiManager.ShowDaySummary(_timeManager.CurrentDay, _currencyManager.goodsPurchasedAmount, _currencyManager.electricitySpentAmount, _currencyManager.rentalSpentAmount, _currencyManager.goodsSoldAmount,_currencyManager.bonusAmount);
                if (hadProfit) _audioManager.PlayDayProfit();
                else _audioManager.PlayDayLoss();
                _timeManager.CurrentDay++;
                _currencyManager.Nullify();
                SaveGame();
                await Task.Delay(5000);
                _uiManager.HideDaySummary();
                if (_currencyManager.currentBalance < 0)
                {
                    _audioManager.PlayGameOver();
                    _uiManager.ShowGameOver(_timeManager.CurrentDay);
                    await Task.Delay(5000);
                    StartNew();
                    GameLoop();
                    return;
                }
                while (!_timeManager.IsShopOpen())
                    await Task.Delay(1000);
                _timeManager.SetSpeed(_timeManager.daySpeedMultiplier);
            }
        }
    }
}
