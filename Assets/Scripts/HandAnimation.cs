using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandAnimation : MonoBehaviour
{
    private static HandAnimation instance;
    public static HandAnimation Instance{get=>instance;}
    public Sprite[] handAnimation;
    public Image image;
    private void Awake() {
        if(instance)
            Debug.LogError("More than one handAnimation");
        instance=this;
    }
    private void Start() {
        image=GetComponent<Image>();
        image.sprite=handAnimation[1];
    }
    public IEnumerator Animated()
    {
        for (int i = 0; i <handAnimation.Length; i++)
        {
            image.sprite=handAnimation[i];
            yield return new WaitForSeconds(.5f);
        }
    }
}
