using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtendedDataViewer : MonoBehaviour
{

    protected virtual void Awake()
    {

    }

    public virtual void SetExtendedData(Building building)
    {

    }

    protected string GetEducationLevelString(EducationLevel level)
    {
        switch (level)
        {
            case EducationLevel.illetarte:
                return "Illiterate";
            case EducationLevel.primary:
                return "Primary";
            case EducationLevel.secondery:
                return "Secondery";
            case EducationLevel.tertiary:
                return "Tertiary";
            default:
                return "N/A";
        }
    }

    protected string GetClassString(CitizenClass _class)
    {
        switch (_class)
        {
            case CitizenClass.low:
                return "Lower Class";
            case CitizenClass.middle:
                return "Middle Class";
            case CitizenClass.high:
                return "Higher Class";
            default:
                return "N/A";
        }
    }
}
