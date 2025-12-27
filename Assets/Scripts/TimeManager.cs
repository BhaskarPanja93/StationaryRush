using System;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    
    [SerializeField] public int daySpeedMultiplier;
    [SerializeField] public int nightSpeedMultiplier;
    [SerializeField] public int shopOpenTime = 9;
    [SerializeField] public int shopClosedTime = 21;
    
    [HideInInspector] public int CurrentDay;
    [HideInInspector] public float timeMultiplier;
    public DateTime CurrentTime = DateTime.Today;
    private List<Action<DateTime>> _timeChangeListeners;

    private void Awake()
    {
        _timeChangeListeners = new List<Action<DateTime>>();
    }

    public void ListenToTimeChange(Action<DateTime> listener)
    {
        _timeChangeListeners.Add(listener);
    }
    
    private bool IsAfter(int hour, int minute = 0)
    {
        var target = new TimeSpan(hour, minute, 0);
        return CurrentTime.TimeOfDay > target;
    }

    private bool IsBefore(int hour, int minute = 0)
    {
        var target = new TimeSpan(hour, minute, 0);
        return CurrentTime.TimeOfDay < target;
    }

    public bool IsShopOpen()
    {
        return IsAfter(shopOpenTime) && IsBefore(shopClosedTime);
    }

    public void SetSpeed(float multiplier)
    {
        timeMultiplier = multiplier;
    }

    public void Freeze()
    {
        Time.timeScale = 0;
    }

    public void Unfreeze()
    {
        Time.timeScale = 1;
    }

    private void FixedUpdate()
    {
        CurrentTime = CurrentTime.AddSeconds(Time.deltaTime * timeMultiplier);
        foreach (var timeChangeListener in _timeChangeListeners)
            timeChangeListener(CurrentTime);
    }
}
