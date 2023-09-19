using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    [SerializeField] private string Owner;
    [SerializeField] private Text t_number;
    public int score;
    [SerializeField] private GameObject smallchessPrebfs;
    private void Start(){
        score=gameObject.name=="Big Slot"?10:5;
        t_number=GetComponentInChildren<Text>();
    }
    private void Update()
    {
        t_number.text=score.ToString();
        ControlChessImage();
    }
    public void ControlChessImage()
    {
        if(score>0 && transform.childCount==1)
        {
            GameObject newchess=Instantiate(smallchessPrebfs,transform.position, Quaternion.identity);
            newchess.transform.SetParent(transform);
            newchess.transform.localScale=Vector3.one;
        }
    }
    public void AddScore()=>score++;
    public void CollectChess()
    {
        if(GameManager.Instance.state.ToString()!=Owner.ToUpper()|| score<=0 || HoldChess.Instance.score>0)
            return;
        for (int i = 0; i < GameManager.Instance.Slots.Length; i++)
        {
            if(GameManager.Instance.Slots[i].name.ToString()==gameObject.name)
            {
                HoldChess.Instance.i=i;
                break;
            }
        }
        HoldChess.Instance.gameObject.transform.position = Vector3.Lerp(HoldChess.Instance.gameObject.transform.position,transform.position,1f);
        HoldChess.Instance.AddScore(score);
        StartCoroutine(HandAnimation.Instance.Animated());
        score=0;
        Destroy(transform.GetChild(1).gameObject);
        if(GameManager.Instance.state.ToString()=="PLAYER")
            CanvasEvent.Instance.ButtonOn();
    }
}
