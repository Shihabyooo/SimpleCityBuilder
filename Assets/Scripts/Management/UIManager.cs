﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    Text dateField;
    Text hourField;
    Text cityNameField;
    Text treasuryField;
    Text populationField;
    GraphGenerator tsGraph;

    void Awake()
    {
        Transform canvas = GameObject.Find("Canvas").transform;
        Transform header = canvas.Find("Header");
        dateField = header.Find("Date").GetComponent<Text>();
        hourField = header.Find("Hour").GetComponent<Text>();
        cityNameField = header.Find("CityName").GetComponent<Text>();
        treasuryField = header.Find("Treasury").GetComponent<Text>();
        populationField = header.Find("Population").GetComponent<Text>();


        tsGraph= canvas.Find("GraphsContainer").Find("TimeSeriesGraph").GetComponent<GraphGenerator>();

        AddButtonListeners();
    }
    
    public void UpdateTime(System.DateTime date)
    {
        //string dateString = date.Day.ToString() + "-" + GetMonthAbbr(date.Month) + "-" + date.Year;
        string hourString = (date.Hour < 10? "0" : "") +  date.Hour.ToString() + ":" + (date.Minute < 10? "0" : "" ) + date.Minute.ToString();

        dateField.text = DateToString(date);
        hourField.text = hourString;
    }

    static public string DateToString(System.DateTime date, bool shortFormat = false)
    {
        string dateString;

        if (shortFormat)
            dateString = date.Day.ToString() + "/" + GetMonthAbbr(date.Month) + "/" + (date.Year %100).ToString("D2");
        else
            dateString = date.Day.ToString() + "-" + GetMonthAbbr(date.Month) + "-" + date.Year.ToString();

        return dateString;
    }

    public void UpdateCityName(string name)
    {
        cityNameField.text = name;
    }

    public void UpdateTreasury(long funds)
    {
        treasuryField.text = "$" + funds.ToString("N0");
    }

    public void UpdatePopulation (long population)
    {
        populationField.text = population.ToString("N0");
    }

    static public string GetMonthAbbr(int month)
    {
        switch(month)
        {
            case 1:
                return "Jan";
            case 2:
                return "Feb";
            case 3:
                return "Mar";
            case 4:
                return "Apr";
            case 5:
                return "May";
            case 6:
                return "Jun";
            case 7:
                return "Jul";
            case 8:
                return "Aug";
            case 9:
                return "Sep";
            case 10:
                return "Oct";
            case 11:
                return "Nov";
            case 12:
                return "Dec";
            default:
                return "NUL";
        }
    }


    void AddButtonListeners() //Some UI elements (including text) can have actions when clicked on. e.g. clicking on treasury would open a historical graph of treasury.
    {
        treasuryField.gameObject.GetComponent<Button>().onClick.AddListener(ShowTreasuryGraph);
        populationField.gameObject.GetComponent<Button>().onClick.AddListener(ShowPopulationGraph);
    }

    //Graph showing
    //General building graph calls
    public void ShowGraph(Building building, string dataTitle)
    {
        tsGraph.ShowGraph(building, dataTitle);
    }

    //Specialized graph calls
    public void ShowTreasuryGraph()
    {
        print ("Attempting to show graph for Treasury");
        //TimeSeries<float> treasuryTS = GameManager.resourceMan.GetTimeSeries(ResourcesHistory.DataType.treasury);
        //tsGraph.ShowGraph(treasuryTS, "Treasury", "Days", "Funds - $");
        tsGraph.ShowGraph(ResourcesHistory.DataType.treasury);
    }

    public void ShowPopulationGraph()
    {
        print ("Attempting to show graph for Population");
        tsGraph.ShowGraph(ResourcesHistory.DataType.population);
    }

}
