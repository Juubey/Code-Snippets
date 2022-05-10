/*
 * AnalyticsTracker.cs 
 * Author(s): Albert Njubi
 * Date Created: 10/1/16
 */


using UnityEngine;
using System.Collections.Generic;
using System;

public class AnalyticsTracker : Singleton<AnalyticsTracker>,
    ECBattleInput.IBattleInput,
    ECBattleStage.IBattleStage,
    ECGamePhase.IGamePhase,
    ECBattle.IBattle,
    ECTutorial.ITutorial
{
    public enum DataAction
    {
        Max,
        Min,
        Total,
        TotalAll,
        Ave,
        AveAll,
        Last,
        Count,
        Current,
        Prev,
    }

    /// <summary>
    /// The Data structure that manages the tracking of individual points of data.
    /// </summary>
    public class DataPoint
    { 
        // This is the function that will return the data we want to track.
        public delegate object GetData();
        public GetData dataToGet;       

        private string mixpanelKey;
        private MixpanelKeyType type;

        private Data data;
        private List<Data> dataList; // For sets of data

        private class Data
        {
            public float ave;
            public float max;
            public float min;
            public float total;
            public float prevValue;
            public float curValue;
            public int count;

            public void Reset()
            {
                ave = 0;
                max = float.MinValue;
                min = 0;// float.MaxValue;
                total = 0;
                count = 0;
                curValue = 0;
                prevValue = 0;
            }

            public void Replace(float newData)
            {
                curValue = newData;
                prevValue = newData;
                ave = newData;
                max = newData;
                min = newData;
                total = newData;
                count = 1;
            }


            public void Update(float newPoint)
            {
                prevValue = curValue;
                curValue = newPoint;
                count++;
                max = Mathf.Max(max, newPoint);
                min = Mathf.Min(min, newPoint);
                total += newPoint;
                ave = newPoint / count;
            }

        }


        public DataPoint(MixpanelKeyType type, string mixpanelKey, DataAction action)
        {
            dataList = new List<Data>();
            data = null;

            this.mixpanelKey = mixpanelKey;
            this.type = type;
            Reset();
            dataToGet = Max;
            switch (action)
            {
                case DataAction.Max:
                    dataToGet = Max;
                    break;
                case DataAction.Min:
                    dataToGet = Min;
                    break;
                case DataAction.Total:
                    dataToGet = Total;
                    break;
                case DataAction.TotalAll:
                    dataToGet = TotalAll;
                    break;
                case DataAction.Ave:
                    dataToGet = Ave;
                    break;
                case DataAction.AveAll:
                    dataToGet = AveAll;
                    break;
                case DataAction.Count:
                    dataToGet = Count;
                    break;
                case DataAction.Current:
                    dataToGet = Cur;
                    break;
                case DataAction.Prev:
                    dataToGet = Prev;
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// Clears all data
        /// </summary>
        public void Clear()
        {
            dataList.Clear();
            data = null;
        }


        /// <summary>
        /// Resets the data 
        /// </summary>
        public void Reset()
        {
            MyData.Reset();
        }

        /// <summary>
        /// Adds new data to be tracked to the list
        /// </summary>
        public DataPoint AddNewData()
        {
            if (data == null || data.count > 0)
            {
                dataList.Add(new Data());
                data = dataList[dataList.Count - 1];
                data.Reset();
            }

            //Returns itself
            return this;
        }


        /// <summary>
        /// Replaces exising data with one new poing
        /// </summary>
        /// <param name="newPoint"></param>
        public void Replace(float newPoint = 1)
        {
            MyData.Replace(newPoint);
        }


        /// <summary>
        /// Updates the current data
        /// </summary>
        /// <param name="newPoint"></param>
        public void Update(float newPoint = 1)
        {
            MyData.Update(newPoint);
        }


        /// <summary>
        /// Returns the max from the dataset
        /// </summary>
        /// <returns></returns>
        private object Max()
        {
            return MyData.max;
        }

        //Returns the min from the dataset
        private object Min()
        {
            return MyData.min;
        }


        //Returns the data accumulated data
        public object Total()
        {
            float result = 0;
            foreach (Data dp in dataList)
            {
                result += dp.total;
            }
            return result;
        }

        //Returns the data accumulated data
        private object TotalAll()
        {
            return MyData.total;
        }


        //Returns the ave of the current data
        private object Ave()
        {
            return data.ave;
        }

        //Returns the ave of all the data in the list
        private object AveAll()
        {
            float result = 0;
            foreach (Data dp in dataList)
            {
                result += dp.total;
            }
            return result / dataList.Count;
        }

        //Returns the last added data point
        private object Cur()
        {
            return data.curValue;
        }

        //Returns the previously added data point
        private object Prev()
        {
            return data.prevValue;
        }

        //Returns the number of data points in the current set
        private object Count()
        {
            return MyData.count;
        }

        /// <summary>
        /// Returns the mixpanel string for this data
        /// </summary>
        public string MixPanelKey
        {
            get { return mixpanelKey; }
        }

        /// <summary>
        /// Returns the mixpanel key for this data
        /// </summary>
        public MixpanelKeyType EntryType
        {
            get { return type; }
        }

        /// <summary>
        /// A getter, used in null checking
        /// </summary>
        private Data MyData
        {
            get
            {
                if (data == null)
                {
                    AddNewData();
                }
                return data;
            }
        }
    }


    Dictionary<MixpanelKeyType, DataPoint> analyticsBook;
    void Start()
    {

        analyticsBook = new Dictionary<MixpanelKeyType, DataPoint>();


        string[] levelNames =
        {
        "Any",
        "Training",
        "Desert",
        "Mountain",
        "Forest",
        "Space",
        "Beach",
        "Cave",
        "Colosseum"
        };

        DataAction turnAction = DataAction.AveAll;
        DataAction deathsAction = DataAction.Count;
        DataAction anyAction = DataAction.Count;

        for (int i = 0; i < levelNames.Length; i++)
        {                                              
            Add((MixpanelKeyType)((int)MixpanelKeyType.TURNS_MAP_ANY + i),   "Turns_" + levelNames[i], turnAction);
            Add((MixpanelKeyType)((int)MixpanelKeyType.DEATHS_MAP_ANY + i),  "Deaths_" + levelNames[i], deathsAction);

            Add((MixpanelKeyType)((int)MixpanelKeyType.BEGIN_ANY + i),       "Begin_" + levelNames[i], anyAction);
            Add((MixpanelKeyType)((int)MixpanelKeyType.END_ANY + i),         "End_" + levelNames[i], anyAction);
            Add((MixpanelKeyType)((int)MixpanelKeyType.BEGIN_SAME_ANY + i),  "BeginSame_" + levelNames[i], anyAction);
            Add((MixpanelKeyType)((int)MixpanelKeyType.END_SAME_ANY + i),    "EndSame_" + levelNames[i], anyAction);
            Add((MixpanelKeyType)((int)MixpanelKeyType.BEGIN_GREATER_ANY + i),  "BeginGreater_" + levelNames[i], anyAction);
            Add((MixpanelKeyType)((int)MixpanelKeyType.END_GREATER_ANY + i),    "EndGreater_" + levelNames[i], anyAction);
        }
        
        
        Add(MixpanelKeyType.BATTLES_PER_SESSION,  "RoundsPerSession",  DataAction.Count);
        Add(MixpanelKeyType.HIGHEST_MAP_LEVEL,    "MapLevel", DataAction.Max);
        Add(MixpanelKeyType.TUTORIAL_PHASE_MAX,   "Tutorial_Phase", DataAction.Current);

        Add(MixpanelKeyType.TEAM_AVERAGE_LEVEL,    "TeamAve_Level", DataAction.Ave);
        Add(MixpanelKeyType.TEAM_AVERAGE_HP,       "TeamAve_HP", DataAction.Ave);
        Add(MixpanelKeyType.TEAM_AVERAGE_DEFENCE,  "TeamAve_Defence", DataAction.Ave);
        Add(MixpanelKeyType.TEAM_AVERAGE_CHARISMA, "TeamAve_Charisma", DataAction.Ave);
        Add(MixpanelKeyType.TEAM_AVERAGE_ATTACK,   "TeamAve_Attack", DataAction.Ave);

        Add(MixpanelKeyType.MONETIZATION_REVIVE, "Count_Revives", DataAction.Count);
        Add(MixpanelKeyType.MONETIZATION_DELETE, "Count_Deletes", DataAction.Count);

        Add(MixpanelKeyType.MONETIZATION_REVIVE_ADS_WATCHED, "AdAward_Revive", DataAction.Count);
        Add(MixpanelKeyType.MONETIZATION_DELETE_ADS_WATCHED, "AdAward_Delete", DataAction.Count);
        Add(MixpanelKeyType.MONETIZATION_REVIVE_ADS_REWARDED, "AdWatch_Revive", DataAction.Count);
        Add(MixpanelKeyType.MONETIZATION_DELETE_ADS_REWARDED, "AdWatch_Delete", DataAction.Count);

        Add(MixpanelKeyType.BATTLE_ESCAPE_USED, "Battle_Escapes", DataAction.AveAll);
        Add(MixpanelKeyType.BATTLE_DEFEND_USED, "Battle_Defends", DataAction.AveAll);
        Add(MixpanelKeyType.BATTLE_SPELL_USED, "Battle_Spells", DataAction.AveAll);
        Add(MixpanelKeyType.BATTTLE_ATTACKS_USED, "Battle_Attacks", DataAction.AveAll);

        Add(MixpanelKeyType.IAP_PURCHSE_1DOLLAR, "IAP_1_Dollar", DataAction.Count);
        Add(MixpanelKeyType.IAP_PURCHSE_2_50DOLLAR, "IAP_2.5_Dollar", DataAction.Count);
        Add(MixpanelKeyType.IAP_PURCHSE_5DOLLAR, "IAP_5_Dollar", DataAction.Count);
        Add(MixpanelKeyType.IAP_PURCHSE_10DOLLAR, "IAP_10_Dollar", DataAction.Count);
        Add(MixpanelKeyType.IAP_PURCHSE_20DOLLAR, "IAP_20_Dollar", DataAction.Count);
        Add(MixpanelKeyType.IAP_PURCHSE_FAIL_COUNT, "IAP_Fails", DataAction.Count); ;
        Add(MixpanelKeyType.IAP_PURCHSE_USER_CANCEL_COUNT, "IAP_Cancels", DataAction.Count);



        Add(MixpanelKeyType.IAP_PURCHSE_COINS_IN_ACCOUNT, "IAP_CoinsB4Buy", DataAction.Ave);

        Add(MixpanelKeyType.SHOP_PURCHASES, "Shop_Purchases", DataAction.Count);
        Add(MixpanelKeyType.SHOP_PURCHASED_COINS_SPENT, "Shop_Spending", DataAction.Total); 
        Add(MixpanelKeyType.SHOP_PURCHASED_MOST_EXPENSIVE, "Shop_Expensive", DataAction.Max);
        Add(MixpanelKeyType.SHOP_PURCHASED_SPECIAL, "Shop_DailyDeals", DataAction.Count);
        Add(MixpanelKeyType.SHOP_PURCHASED_BASICITEMS,   "Shop_Basics", DataAction.Count);
        Add(MixpanelKeyType.SHOP_PURCHASED_TODAYSITEMS, "Shop_Todays", DataAction.Count);

        Add(MixpanelKeyType.AVE_TIME_IAP,       "TimeAve_IAP", DataAction.Ave);
        Add(MixpanelKeyType.AVE_TIME_SHOP,      "TimeAve_Shop", DataAction.Ave);
        Add(MixpanelKeyType.AVE_TIME_ARMORY,    "TimeAve_Armory", DataAction.Ave);
        Add(MixpanelKeyType.AVE_TIME_DRAFT,     "TimeAve_Draft", DataAction.Ave);
        Add(MixpanelKeyType.AVE_TIME_MAP,       "TimeAve_Map", DataAction.Ave);
        Add(MixpanelKeyType.AVE_TIME_MATCHUP,   "TimeAve_Matchup", DataAction.Ave);
        Add(MixpanelKeyType.AVE_TIME_SUMMARY,   "TimeAve_Summary", DataAction.Ave);

        Add(MixpanelKeyType.VISITED_IAP,        "Visits_IAP", DataAction.Count);
        Add(MixpanelKeyType.VISITED_SHOP,       "Visits_Shop", DataAction.Count);
        Add(MixpanelKeyType.VISITED_ARMORY,     "Visits_Armoy", DataAction.Count);
        Add(MixpanelKeyType.VISITED_DRAFT,      "Visits_Draft", DataAction.Count);
        Add(MixpanelKeyType.VISITED_MAP,        "Visits_Map", DataAction.Count);
        Add(MixpanelKeyType.VISITED_MATCHUP,    "Visits_Matchup", DataAction.Count);
        Add(MixpanelKeyType.VISITED_SUMMARY,    "Visits_Summary", DataAction.Count);

        Add(MixpanelKeyType.GOSSIP_ASKED_ABOUT, "Gossip_AskedFor", DataAction.Count);

        Add(MixpanelKeyType.DAILY_REWARD_RECIEVED, "DailyReward", DataAction.Total);

        Add(MixpanelKeyType.PROMO_CODES_ENTERED, "PromoCodes", DataAction.Total);




        EventManager.AddSubscriber(this, ECBattleInput.Method.OnPressAttack);
        EventManager.AddSubscriber(this, ECBattleInput.Method.OnPressDefend);
        EventManager.AddSubscriber(this, ECBattleStage.Method.OnPrepareBattle);
        EventManager.AddSubscriber(this, ECBattleStage.Method.OnBeginBattle);
        EventManager.AddSubscriber(this, ECBattleStage.Method.OnDeath);
        EventManager.AddSubscriber(this, ECBattleStage.Method.OnBattleFinished);

        EventManager.AddSubscriber(this, ECGamePhase.Method.MapPhase);
        EventManager.AddSubscriber(this, ECGamePhase.Method.MatchUpPhase);

        EventManager.AddSubscriber(this, ECTutorial.Method.OnStart);
        EventManager.AddSubscriber(this, ECTutorial.Method.OnComplete);
        
        EventManager.AddSubscriber(this, ECBattle.Method.SwapTurns);
    }
    /// <summary>
    /// Adds a new data point to monitor
    /// </summary>
    private void Add(MixpanelKeyType type, string mixPanelKey, DataAction action)
    {
        DataPoint point = new DataPoint(type, mixPanelKey, action);
        analyticsBook.Add(type, point);
    }

    /// <summary>
    /// For functions to update analytics data from outside of this class
    /// </summary>
    public void UpdateBook(MixpanelKeyType type, float data = 1)
    {
        if (!analyticsBook.ContainsKey(type))
        {
            return;
        }
        if (type == MixpanelKeyType.COUNT)
        {
            return; //not data
        }

        if (type == MixpanelKeyType.PROMO_CODES_ENTERED)
        {
            analyticsBook[type].Replace(data);
        }
        else
        {
            analyticsBook[type].Update(data);
        }
    }


    /// <summary>
    /// Updates the Mixpanel Dictionary will all the data we need to access
    /// 
    /// Called by the MixpanelController::OnApplicationFocus
    /// </summary>
    /// <param name="reportBook"></param>
    public void Report(ref Dictionary<string, object> reportBook)
    {
        foreach (KeyValuePair<MixpanelKeyType, DataPoint> pair in analyticsBook)
        {
            object data = pair.Value.dataToGet();

            MixpanelKeyType entryType = pair.Value.EntryType;

            //Do send these if their is no data
            if (string.IsNullOrEmpty(pair.Value.Total().ToString()) ||
                float.Parse(pair.Value.Total().ToString()) == 0f)
            {
                continue;
            }

            int value = 0;
            //Take Care of Special Cases
            switch (entryType)
            {
                case MixpanelKeyType.HIGHEST_MAP_LEVEL:
                    int mapIndex = (int)(float.Parse(pair.Value.dataToGet().ToString()));
                    data = VenueController.Instance.Venues[mapIndex].name;
                    break;
                case MixpanelKeyType.TUTORIAL_PHASE_MAX:
                    Tutorial tut = Tutorial.Instance;
                    if (tut.completed || !tut.showTutorial)
                    {
                        data = "Completed";
                    }
                    else
                    {
                        //Get the name of the Tutorial Command
                        string s = pair.Value.dataToGet().ToString();
                        value = (int)(float.Parse(s));
                        data = ((TutorialCommand)(value)).ToString();
                    }
                    break;
                case MixpanelKeyType.PROMO_CODES_ENTERED:
                    //Turn int into the correct string
                    value = (int)(float.Parse(pair.Value.dataToGet().ToString()));
                    data = EquipmentManager.instance.IntToPromoCode(value);
                    break;
                default:
                    break;
            }


            reportBook.Add(pair.Value.MixPanelKey, data);
        }
        return;
    }


    /// <summary>
    /// This is the TeamAnalytics data which will be passed into mixpanel
    /// </summary>
    public class TeamAnalyticsData
    {
        public float attackAve = 0f;
        public float defenseAve = 0f;
        public float healthAve = 0f;
        public float charismAve = 0f;
        public float levelAve = 0f;

        /// <summary>
        /// Calculates the team averages
        /// for attack, defense, and health.
        /// </summary>
        public static TeamAnalyticsData GetTeamAveInformation<T>(List<T> data) where T : Fighter
        {
            int numOfFighters = 0;

            int totalAttack = 0;
            int totalDefense = 0;
            int totalHealth = 0;
            int totalCharisma = 0;
            int totalLevel = 0;

            foreach (T fighter in data)
            {
                if (fighter != null)
                {
                    StatsData stats = fighter.MyInfo.Stats;

                    totalAttack += stats.attack.Damage;
                    totalDefense += stats.defend.DefenseValue;
                    totalHealth += stats.health.MaxHealth;
                    totalLevel += stats.level.Level;
                    if (fighter.MyFighterType == FighterType.Gladiator)
                    {
                        totalCharisma += (fighter as Gladiator).MyInfo.Verbal.Charisma;
                    }

                    numOfFighters++;
                }
            }

            TeamAnalyticsData resultData = new TeamAnalyticsData();
            resultData.attackAve = totalAttack / numOfFighters;
            resultData.defenseAve = totalDefense / numOfFighters;
            resultData.healthAve = totalHealth / numOfFighters;
            resultData.charismAve = totalCharisma / numOfFighters;
            resultData.levelAve = totalLevel / numOfFighters;
            return resultData;
        }



    }

    #region IBattleInput Functions


    /// <summary>
    /// Increases the number of times
    /// the player used attack.
    /// </summary>
    public void OnPressAttack()
    {
        analyticsBook[MixpanelKeyType.BATTTLE_ATTACKS_USED].Update();
    }


    /// <summary>
    /// Increases the number of times
    /// the player used defend.
    /// </summary>
    public void OnPressDefend()
    {
        analyticsBook[MixpanelKeyType.BATTLE_DEFEND_USED].Update();
    }


    /// <summary>
    /// Increases the number of times
    /// the player used spell.
    /// </summary>
    public void OnPressSpell()
    {
        analyticsBook[MixpanelKeyType.BATTLE_SPELL_USED].Update();
    }


    /// <summary>
    /// Fires when a gladiator defends
    /// </summary>
    public void OnDefendActivity(Gladiator activeGladiator)
    {
        //Nothing to do here
    }

    public void OnPressEscape()
    {
        analyticsBook[MixpanelKeyType.BATTLE_ESCAPE_USED].Update();
    }

    public void OnDeath()
    {
        MixpanelKeyType key = (MixpanelKeyType)
            (VenueController.Instance.CurrentVenueID +
            (int)MixpanelKeyType.DEATHS_MAP_TRAINING);

        UpdateBook(key, 1);
    }

    #endregion

    #region IBattleStage Functions


    /// <summary>
    /// Resets variables and then
    /// sets team numbers.
    /// </summary>
    public void OnPrepareBattle()
    {
        analyticsBook[MixpanelKeyType.BATTTLE_ATTACKS_USED].AddNewData();
        analyticsBook[MixpanelKeyType.BATTLE_DEFEND_USED].AddNewData();
        analyticsBook[MixpanelKeyType.BATTLE_SPELL_USED].AddNewData();
        analyticsBook[MixpanelKeyType.BATTLE_ESCAPE_USED].AddNewData();
    }


    /// <summary>
    /// Increases the battle number
    /// once the battle starts.
    /// </summary>
    public void OnBeginBattle()
    {
        UpdateBook(MixpanelKeyType.BATTLES_PER_SESSION, 1);
    }


    /// <summary>
    /// Sends info to Mixpanel
    /// when the battle is finished.
    /// </summary>
    public void OnBattleFinished()
    {
        //Nothing to do here
        
    }

    public void OnSwitchGladiators(VerbalAttackType verbalAttack)
    {
        //Nothing to do here
    }

    #endregion

    #region ECGamePhase Events

    public void StartPhase()
    {
        //Nothing to do here
    }

    public void DraftPhase()
    {
        //Nothing to do here
    }

    public void MapPhase()
    {
    }

    public void MatchUpPhase()
    {
        analyticsBook[MixpanelKeyType.HIGHEST_MAP_LEVEL].Update(VenueController.Instance.CurrentVenueID);

        //Just incase the player changes his or her mind, and wants to change gladiators before going to battle
        TeamAnalyticsData playerData = TeamAnalyticsData.GetTeamAveInformation(Player.Instance.MyRoster.Gladiators);

        analyticsBook[MixpanelKeyType.TEAM_AVERAGE_ATTACK].AddNewData().Update(playerData.attackAve);
        analyticsBook[MixpanelKeyType.TEAM_AVERAGE_CHARISMA].AddNewData().Update(playerData.charismAve);
        analyticsBook[MixpanelKeyType.TEAM_AVERAGE_DEFENCE].AddNewData().Update(playerData.defenseAve);
        analyticsBook[MixpanelKeyType.TEAM_AVERAGE_HP].AddNewData().Update(playerData.healthAve);
        analyticsBook[MixpanelKeyType.TEAM_AVERAGE_LEVEL].AddNewData().Update(playerData.levelAve);

        MixpanelKeyType key = (MixpanelKeyType)
          (VenueController.Instance.CurrentVenueID +
          (int)MixpanelKeyType.TURNS_MAP_TRAINING);

    }

    public void FightingPhase()
    {
        //Nothing to do here
    }

    public void SummaryPhase()
    {
        //Nothing to do here
    }

    public void UpgradePhase()
    {
        //Nothing to do here
    }

    #endregion

    #region ECTutorial Events

    public void OnComplete(TutorialCommand tutorialCommand)
    {
        analyticsBook[MixpanelKeyType.TUTORIAL_PHASE_MAX].Update((int)tutorialCommand);
    }

    public void OnStart(TutorialCommand tutorialCommand)
    {
        analyticsBook[MixpanelKeyType.TUTORIAL_PHASE_MAX].Update((int)tutorialCommand);
    }
    #endregion

    #region ECBattle Events

    public void AttackBegin(ECBattle.BattleObjectAttackerDefender param)
    {
        //Nothing to do here
    }

    public void AttackBattle(ECBattle.BattleObjectAttackerDefender param)
    {
        //Nothing to do here
    }

    public void AttackEnd(Fighter attacker)
    {
        //Nothing to do here
    }

    public void Defend(Fighter defender)
    {
        //Nothing to do here
    }

    public void Spell(ECBattle.BattleObjectAttackerDefender param)
    {
        //Nothing to do here
    }

    public void Escape(Fighter escapee)
    {
        //Nothing to do here
    }

    public void Victory(List<Gladiator> winnerList)
    {
        //Nothing to do here
    }

    public void Defeat(List<Gladiator> winnerList)
    {
        //Nothing to do here
    }

    public void SwapFighter(ECBattle.BattleObjectSwappers swappers)
    {
        //Nothing to do here
    }

    public void SwapTurns()
    {
        //Update Turn
        MixpanelKeyType key = (MixpanelKeyType)
            (VenueController.Instance.CurrentVenueID + (int)MixpanelKeyType.TURNS_MAP_TRAINING);
        UpdateBook(key, 0.5f); //half a turn

    }

    public void Log(string methodName)
    {
        //Nothing to do here
    }

    public void ShopPhase()
    {
        //Nothing to do here
    }

    public void ArmoryPhase()
    {
        //Nothing to do here
    }

    public void InfirmaryPhase()
    {
        //Nothing to do here
    }



    #endregion

    private static int battlePrevStartIndex = -1;
    private static int battlePrevEndIndex = -1;

    /// <summary>
    /// Tracks Events that correspond to the beginning and end of a battle
    /// </summary>
    /// <param name="isStarting"></param>
    public void TrackBattleEvent(bool isStarting = true)
    {
        int index = VenueController.Instance.CurrentVenue.id;
        index++; //add 1 to account for the "any" key type
        int indexToUse = isStarting ? battlePrevStartIndex : battlePrevEndIndex;
        if (indexToUse != -1)
        {
            if (index == indexToUse)
            {
                UpdateBattleEventBook(MixpanelKeyType.BEGIN_SAME_ANY,         MixpanelKeyType.END_SAME_ANY, isStarting);
                UpdateBattleEventBook(MixpanelKeyType.BEGIN_SAME_ANY + index, MixpanelKeyType.END_SAME_ANY + index, isStarting);
            }
            else if (index > indexToUse)
            {
                //Doing more challenging level
                UpdateBattleEventBook(MixpanelKeyType.BEGIN_GREATER_ANY,         MixpanelKeyType.END_GREATER_ANY, isStarting);
                UpdateBattleEventBook(MixpanelKeyType.BEGIN_GREATER_ANY + index, MixpanelKeyType.END_GREATER_ANY + index, isStarting);
            }
            else
            {
                //Doing less challenging level
            }
        }
        
        if(isStarting)
        {
            battlePrevStartIndex = index;
        }
        else
        {
            battlePrevEndIndex = index;
        }

        UpdateBattleEventBook(MixpanelKeyType.BEGIN_ANY,         MixpanelKeyType.END_ANY, isStarting);
        UpdateBattleEventBook(MixpanelKeyType.BEGIN_ANY + index, MixpanelKeyType.END_ANY + index, isStarting);
    }

    private void UpdateBattleEventBook(MixpanelKeyType start, MixpanelKeyType end, bool isStarting)
    {
        MixpanelKeyType use = end;
        if (isStarting)
        {
            use = start;
        }
        UpdateBook(use);
    }
}


