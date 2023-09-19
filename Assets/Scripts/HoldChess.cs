using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoldChess : MonoBehaviour
{
    private static HoldChess instance;
    public static HoldChess Instance{get=>instance;}
    public int score;
    public int i;
    private Text t_holding;
    private void Awake() {
        if(instance)
            Debug.LogError("More than 1 Holder");
        instance=this;
    }
    private void Start() =>t_holding=GetComponentInChildren<Text>();
    private void Update() =>UpdateScore();
    public void AddScore(int amout)=>score+=amout;
    private void UpdateScore()
    {
        score=score<0?0:score;
        t_holding.text=score.ToString();
    }
}
