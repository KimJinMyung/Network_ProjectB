using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_UI : MonoBehaviour
{
    [SerializeField]
    List<Image> Player_HP;

    [SerializeField] 
    List<Image> Player_DashCount;

    public void Changed_PlayerHP(int HP)
    {        
        ResetList(Player_HP);
        int index = 0;
        foreach (var item in Player_HP)
        {
            if (index >= HP) break;

            item.gameObject.SetActive(true);
            index++;
        }
    }

    private void ResetList(List<Image> images)
    {
        foreach (var item in images)
        {
            item.gameObject.SetActive(false);
        }
    }

    public void Changed_DashCount(int Count)
    {
        ResetList(Player_DashCount);
        int index = 0;
        foreach (var item in Player_DashCount)
        {
            if (index >= Count) break;

            item.gameObject.SetActive(true);
            index++;
        }
    }
}
