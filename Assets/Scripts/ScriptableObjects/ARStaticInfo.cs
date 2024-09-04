using System.Collections;
using System.Collections.Generic;
using Messy.Definitions;
using UnityEngine;

[CreateAssetMenu(fileName = "ARStaticInfo", menuName = "ARStaticInfo")]
public class ARStaticInfo : ScriptableObject
{
    private static ARStaticInfo instance;

    public static ARStaticInfo Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<ARStaticInfo>("ARStaticInfo");
            }

            return instance;
        }
    }
    
    public List<ExhibitionSO> Exhibitions;
}