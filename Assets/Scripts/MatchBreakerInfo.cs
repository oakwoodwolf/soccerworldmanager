using System;
using UnityEngine;
[CreateAssetMenu]
public class MatchBreakerInfo : ScriptableObject
{
    public Sprite texture;
    public int cost;
    public Enums.MatchBreakerFlags flags;
    public string textBig;
    public string textSmall;
}
