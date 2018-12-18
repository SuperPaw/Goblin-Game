using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpriteSelecter : MonoBehaviour
{
    public Sprite[] sprites;
    public SpriteRenderer SpriteRenderer;
    
	void Start ()
	{
	    if (SpriteRenderer)
	        SpriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
	}
	
}
