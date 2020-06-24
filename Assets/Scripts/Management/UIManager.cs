using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    Text dateField;
    Text hourField;
    Text cityNameField;
    Text treasuryField;

    void Awake()
    {
        Transform canvas = GameObject.Find("Canvas").transform;
        Transform header = canvas.Find("Header");
        dateField = header.Find("Date").GetComponent<Text>();
        hourField = header.Find("Hour").GetComponent<Text>();
        cityNameField = header.Find("CityName").GetComponent<Text>();
        treasuryField = header.Find("Treasury").GetComponent<Text>();
    }
    
    public void UpdateTime(System.DateTime date)
    {
        string dateString = date.Day.ToString() + "-" + GetMonthAbbr(date.Month) + "-" + date.Year;
        string hourString = (date.Hour < 10? "0" : "") +  date.Hour.ToString() + ":" + (date.Minute < 10? "0" : "" ) + date.Minute.ToString();

        dateField.text = dateString;
        hourField.text = hourString;
    }

    public void UpdateCityName(string name)
    {
        cityNameField.text = name;
    }

    public void UpdateTreasury(long funds)
    {
        treasuryField.text = "$" + funds.ToString("N0");
    }


    public string GetMonthAbbr(int month)
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

}
