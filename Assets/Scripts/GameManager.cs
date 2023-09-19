using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public enum GameState { START, PLAYER, RIVAL, DONE }
public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance{get=>instance;}
    public Slot[] Slots;
    public Slot[] BigSlots;
    public Slot[] PlayerSlots;
    public Slot[] RivalSlots;
    [SerializeField] private GameObject bigChessPrebfs;
    [SerializeField] private GameObject smallChessPrebfs;
    public GameState state;
    public CharacterStatus[] characterStatuses;
    [SerializeField] private Text[] t_score;
    [SerializeField] private Text Labels;
    [SerializeField] private Text showTimer;
    [SerializeField] private AudioClip mainMusic;
    [SerializeField] private float playTime;
    float duration;
    float timer;
    bool loot;
    bool acting;
    private void Awake() {
        if(instance)
            Debug.LogError("More than 1 Game Manager");
        instance=this;
    }
    private void Setup()
    {
        foreach (Slot item in Slots)
        {
            GameObject newchess=Instantiate(item.name=="Big Slot"?bigChessPrebfs:smallChessPrebfs,item.gameObject.transform.position, Quaternion.identity);
            newchess.transform.SetParent(item.gameObject.transform);
            newchess.transform.localScale= Vector3.one;
        }
    }
    private void Start() {
        GetComponent<AudioSource>().PlayOneShot(mainMusic);
        duration=mainMusic.length+8f;
        timer=Time.time;
        loot=false;
        acting=false;
        Setup();
        state=GameState.START;
        StartCoroutine(PlayerTurn());
        // Time.timeScale=5f;
    }
    private void Update() {
        if(state==GameState.PLAYER && !loot && !acting)
        {
            showTimer.text="Timer: "+ Mathf.Round(playTime).ToString();
            playTime=playTime<=0?0:playTime-Time.deltaTime;
            if(playTime==0)
                StartCoroutine(EndPlayerTurn());
        }
        else
            showTimer.text="Timer: ";
        if(duration<Time.time-timer)
        {
            GetComponent<AudioSource>().PlayOneShot(mainMusic);
            timer=Time.time;
        }
        CheckWin();
    }
    private IEnumerator PlayerTurn()
    {
        state=state!=GameState.DONE?GameState.PLAYER:state;
        yield return null;
    }
    public IEnumerator EndPlayerTurn()
    {
        playTime=0;
        state=state!=GameState.DONE?GameState.RIVAL:state;
        yield return new WaitForSeconds(1f);
        StartCoroutine(RivalTurn());
        playTime=40;
    }
    private IEnumerator RivalTurn()
    {
        int x=0;
        if(state==GameState.RIVAL)
        {
            while (state==GameState.RIVAL)
            {
                x=UnityEngine.Random.Range(0,5);
                if(Slots[x].score>0)
                    break;
            }
            Slots[x].CollectChess();
            yield return new WaitForSeconds(2f);
            int y=UnityEngine.Random.Range(1,3);
            if(y%2==0)
                StartCoroutine(PayRight());
            else        
                StartCoroutine(PayLeft());
            yield return new WaitUntil(()=>acting==false && loot == false);
            state=GameState.PLAYER;
            yield return new WaitForSeconds(2f);
            StartCoroutine(PlayerTurn());
        }
    }
    private void CheckWin()
    {
        for (int i = 0; i < characterStatuses.Length; i++)
        {
            t_score[i].text=characterStatuses[i].score.ToString();
        }
        if(BigSlots.Length>0)
        {
            for (int i = 0; i < BigSlots.Length; i++)
            {
                if(!BigSlots[i].gameObject.transform.Find("BigChess(Clone)"))
                    RemoveAt(ref BigSlots,i);
            }
        }
        CheckChess(PlayerSlots,GameState.PLAYER,characterStatuses[0],characterStatuses[1]);
        CheckChess(RivalSlots,GameState.RIVAL,characterStatuses[1],characterStatuses[0]);
        
    }
    private void CheckChess(Slot[] slots,GameState status, CharacterStatus first, CharacterStatus second)
    {
        bool overchess=false;
        string label=null;
        for (int i = 0; i < slots.Length; i++)
        {
            if(slots[i].score==0 && HoldChess.Instance.score==0)
            {
                if(i==slots.Length-1)
                    overchess=true;
                else
                    continue;
            }   
            else
                break;
        }
        if(overchess && !loot && !acting && state==status)
        {
            if(BigSlots.Length!=0)
            {
                if(first.score>0)
                {
                    for (int j = 0; j < slots.Length; j++)
                    {
                        slots[j].AddScore();
                        first.score--;
                    }
                }
                else
                {
                    label=CalculateScore(first,second)?first.name+" Win":"";
                    if(label!=null)
                        Labels.text=label;
                    Debug.Log(CalculateScore(first,second)?first.name+" Win":"");
                    state=GameState.DONE;
                }
                    
            }
            else
            {
                label=CalculateScore(first,second)?first.name+" Win":"";
                if(label!=null)
                    Labels.text=label;
                Debug.Log(CalculateScore(first,second)?first.name+" Win":"");
                state=GameState.DONE;
            }
        }
    }
    private bool CalculateScore(CharacterStatus a, CharacterStatus b) {return a.score>b.score;}
    public IEnumerator PayLeft()
    {
        acting=true;
        int i=HoldChess.Instance.i;
        if(HoldChess.Instance.score>0)
        {
            do
            {
                if(i==0)
                    i=Slots.Length-1;
                else
                    i--;
                StartCoroutine(HandAnimation.Instance.Animated());
                Slots[i].AddScore();
                HoldChess.Instance.score--;
                HoldChess.Instance.gameObject.transform.position = Vector3.Lerp(HoldChess.Instance.gameObject.transform.position,Slots[i].gameObject.transform.position,1f);
                yield return new WaitForSeconds(1f);
            } while (HoldChess.Instance.score>0);
        }
        if(i==0)
            i=Slots.Length-1;
        else
            i--;
        if(Slots[i].score>0 && !loot && Slots[i].gameObject.name!="Big Slot")
        {
            HoldChess.Instance.gameObject.transform.position = Vector3.Lerp(HoldChess.Instance.gameObject.transform.position,Slots[i].gameObject.transform.position,1f);
            HoldChess.Instance.AddScore(Slots[i].score);
            StartCoroutine(HandAnimation.Instance.Animated());
            HoldChess.Instance.i=i;
            Slots[i].score=0;
            if(Slots[i].gameObject.transform.childCount>1)
                Destroy(Slots[i].gameObject.transform.GetChild(1).gameObject);
            yield return new WaitForSeconds(1f);
            StartCoroutine(PayLeft());
        }
        else if(Slots[i].score==0 && Slots[i].gameObject.name!="Big Slot")
        {
            HoldChess.Instance.gameObject.transform.position = Vector3.Lerp(HoldChess.Instance.gameObject.transform.position,Slots[i].gameObject.transform.position,1f);
            loot=true;
            acting=false;
            if(i==0)
                i=Slots.Length-1;
            else
                i--;
            if (Slots[i].score>0)
            {
                yield return new WaitForSeconds(1f);
                HoldChess.Instance.gameObject.transform.position = Vector3.Lerp(HoldChess.Instance.gameObject.transform.position,Slots[i].gameObject.transform.position,1f);
                HoldChess.Instance.i=i;
                StartCoroutine(HandAnimation.Instance.Animated());
                if(state==GameState.PLAYER)
                    characterStatuses[0].score+=Slots[i].score;
                else if(state==GameState.RIVAL)
                    characterStatuses[1].score+=Slots[i].score;
                Slots[i].score=0;
                if(Slots[i].gameObject.transform.childCount>1)
                    Destroy(Slots[i].gameObject.transform.GetChild(1).gameObject);
                yield return new WaitForSeconds(1f);
                StartCoroutine(PayLeft());
            }
            else
            {
                loot=false;
                acting=false;
                if(state==GameState.PLAYER)
                    StartCoroutine(EndPlayerTurn());
                HoldChess.Instance.gameObject.transform.position = Vector3.Lerp(HoldChess.Instance.gameObject.transform.position,Vector3.zero,1f);
            }
        }
        else
        {
            loot=false;
            acting=false;
            if(state==GameState.PLAYER)
                StartCoroutine(EndPlayerTurn());
            HoldChess.Instance.gameObject.transform.position = Vector3.Lerp(HoldChess.Instance.gameObject.transform.position,Vector3.zero,1f);
        }
    }
    public IEnumerator PayRight()
    {
        acting=true;
        int i=HoldChess.Instance.i;
        if(HoldChess.Instance.score>0)
        {
            do
            {
                if(i==Slots.Length-1)
                    i=0;
                else
                    i++;
                StartCoroutine(HandAnimation.Instance.Animated());
                Slots[i].AddScore();
                HoldChess.Instance.score--;
                HoldChess.Instance.gameObject.transform.position = Vector3.Lerp(HoldChess.Instance.gameObject.transform.position,Slots[i].gameObject.transform.position,1f);
                yield return new WaitForSeconds(1f);
            } while (HoldChess.Instance.score>0);
        }
        if(i==Slots.Length-1)
            i=0;
        else
            i++;
        if(Slots[i].score>0 && !loot && Slots[i].gameObject.name!="Big Slot")
        {
            HoldChess.Instance.gameObject.transform.position = Vector3.Lerp(HoldChess.Instance.gameObject.transform.position,Slots[i].gameObject.transform.position,1f);
            HoldChess.Instance.AddScore(Slots[i].score);
            HoldChess.Instance.i=i;
            StartCoroutine(HandAnimation.Instance.Animated());
            Slots[i].score=0;
            if(Slots[i].gameObject.transform.childCount>1)
                Destroy(Slots[i].gameObject.transform.GetChild(1).gameObject);
            yield return new WaitForSeconds(1f);
            StartCoroutine(PayRight());
        }
        else if(Slots[i].score==0 && Slots[i].gameObject.name!="Big Slot")
        {
            HoldChess.Instance.gameObject.transform.position = Vector3.Lerp(HoldChess.Instance.gameObject.transform.position,Slots[i].gameObject.transform.position,1f);
            loot=true;
            if(i==Slots.Length-1)
                i=0;
            else
                i++;
            if (Slots[i].score>0)
            {
                yield return new WaitForSeconds(1f);
                HoldChess.Instance.gameObject.transform.position = Vector3.Lerp(HoldChess.Instance.gameObject.transform.position,Slots[i].gameObject.transform.position,1f);
                HoldChess.Instance.i=i;
                StartCoroutine(HandAnimation.Instance.Animated());
                if(state==GameState.PLAYER)
                    characterStatuses[0].score+=Slots[i].score;
                else if(state==GameState.RIVAL)
                    characterStatuses[1].score+=Slots[i].score;
                Slots[i].score=0;
                if(Slots[i].gameObject.transform.childCount>1)
                    Destroy(Slots[i].gameObject.transform.GetChild(1).gameObject);
                yield return new WaitForSeconds(1f);
                StartCoroutine(PayRight());
            }
            else
            {
                loot=false;
                acting=false;
                if(state==GameState.PLAYER)
                    StartCoroutine(EndPlayerTurn());
                HoldChess.Instance.gameObject.transform.position = Vector3.Lerp(HoldChess.Instance.gameObject.transform.position,Vector3.zero,1f);
            }
        }
        else
        {
            loot=false;
            acting=false;
            if(state==GameState.PLAYER)
                StartCoroutine(EndPlayerTurn());
            HoldChess.Instance.gameObject.transform.position = Vector3.Lerp(HoldChess.Instance.gameObject.transform.position,Vector3.zero,1f);
        }
    }
    public static void RemoveAt<T>(ref T[] arr, int index)
    {
        for (int a = index; a < arr.Length - 1; a++)
        {
            arr[a] = arr[a + 1];
        }
        Array.Resize(ref arr, arr.Length - 1);
    }
}
