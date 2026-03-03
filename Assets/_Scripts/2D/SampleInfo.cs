using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleInfo : MonoBehaviour
{
    [SerializeField] Color Abv0, Blw0;

    Color CurrentColor;

    private void Start()
    {
        CurrentColor = transform.position.y >= 0 ? Abv0 : Blw0;
        SpriteRenderer rend = GetComponent<SpriteRenderer>();
        rend.color = CurrentColor;
    }
}
