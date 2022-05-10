/*
 * CurrencyManager
 * Author: Albert N
 * Date Created: 9/1/16
 */
 
using UnityEngine;

/// <summary>
/// Manages the currency mechanics for our game
/// </summary>
public class CurrencyManager : Singleton<CurrencyManager>
{

    public const string CURRENCY_NAME = "Coins";
    private const string PREF_CURRENCY = "coins";

    /// <summary>
    /// The number of coins the player starts with. 
    /// </summary>
    [SerializeField]
    private int coinsToStart = 0;

    [SerializeField]
    private CurrencyItemManager currencyItemManager;


    private void Start()
    {
        CurrencyDisplay[] displays = Resources.FindObjectsOfTypeAll<CurrencyDisplay>();
        foreach (CurrencyDisplay display in displays)
        {
            if (!display.gameObject.activeSelf)
            {
                Debug.Log("[Currency Manager] CurrencyDisplay as not active when game started, setting it active now");
            }
            display.gameObject.SetActive(true);
        }
    }


    /// <summary>
    /// Delegate used for CurrencyChanged event. Takes the amount before and after the change.
    /// </summary>
    public delegate void CurrencyChangedDelegate(int prev, int current);


    /// <summary>
    /// Event called every time the amount of currency is changed.
    /// </summary>
    public event CurrencyChangedDelegate CurrencyChanged;


    /// <summary>
    /// Gets or sets the amount of currency the player has.
    /// </summary>
    public int Amount
    {
        get
        {
            if (PlayerPrefs.HasKey(PREF_CURRENCY))
            {
                return PlayerPrefs.GetInt(PREF_CURRENCY);
            }
            else
            {
                PlayerPrefs.SetInt(PREF_CURRENCY, coinsToStart);
                return PlayerPrefs.GetInt(PREF_CURRENCY);
            }
        }

        set
        {
            if (value < 0)
            {
                Debug.LogWarning(string.Format("Player's currency set to a negative number ({0})!", value));
            }

            int previousAmount = Amount;
            PlayerPrefs.SetInt(PREF_CURRENCY, value);

            // send event if the new value isn't equal to the old value
            if (previousAmount != value)
            {
                // call the event
                // we do this after we change the value so that all calls to CurrencyManager.Amount
                // will return the amount of money the player really has
                CurrencyChanged(previousAmount, value);
            }

        }
    }


    /// <summary>
    /// Returns whether or not the player has the provided amount of currency.
    /// </summary>
    public bool HasAmount(int amountNeeded)
    {
        return Amount >= amountNeeded;
    }


    /// <summary>
    /// Gives the player the specified amount of currency.
    /// </summary>
    public void GiveCurrency(int givenAmount)
    {
        Amount += givenAmount;
    }


    /// <summary>
    /// Takes the specified amount of currency from the player.
    /// </summary>
    public void TakeCurrency(int takenAmount)
    {
        if (Amount < takenAmount)
        {
            Debug.LogWarning(string.Format("More currency taken from player than player has (has: {0}, taken: {1})!", Amount, takenAmount));
        }

        Amount -= takenAmount;
    }


    /// <summary>
    /// Returns the current item manager ui
    /// </summary>
    public CurrencyItemManager MyCurrencyItemManager
    {
        get { return currencyItemManager; }
    }
}
