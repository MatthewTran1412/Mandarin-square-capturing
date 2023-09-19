using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasEvent : MonoBehaviour
{
    private static CanvasEvent instance;
    public static CanvasEvent Instance{get=>instance;}
    [SerializeField] private Button Left;
    [SerializeField] private Button Right;
    private void Awake() {
        if(instance)
            Debug.LogError("More than 1 Canvas Event");
        instance=this;
        Left.onClick.AddListener(()=>{
            StartCoroutine(GameManager.Instance.PayLeft());
            ButtonOff();
        });
        Right.onClick.AddListener(()=>{
            StartCoroutine(GameManager.Instance.PayRight());
            ButtonOff();
        });
    }
    private void Start() =>ButtonOff();
    public void ButtonOn()
    {
        Left.gameObject.SetActive(true);
        Right.gameObject.SetActive(true);
    }
    public void ButtonOff()
    {
        Left.gameObject.SetActive(false);
        Right.gameObject.SetActive(false);
    }
}
