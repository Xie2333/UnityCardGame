using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card
{ 
    /// <summary>
    /// Ȩֵ
    /// </summary>
    public int Weight { get; set; }
    /// <summary>
    /// ��ɫ
    /// </summary>
    public int Color { get; set; }

    public Card(int weight, int color)
    {
        Weight = weight;
        Color = color;
    }

}
