using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    [SerializeField] public int startingBalance;
    [SerializeField] public int electricitySpentAmount;
    [SerializeField] public int rentalSpentAmount;
    
    [HideInInspector] public int currentBalance;
    [HideInInspector] public int goodsSoldAmount;
    [HideInInspector] public int goodsPurchasedAmount;
    [HideInInspector] public int bonusAmount;
    
    public void Nullify()
    {
        goodsSoldAmount = 0;
        goodsPurchasedAmount = 0;
        bonusAmount = 0;
    }

}
