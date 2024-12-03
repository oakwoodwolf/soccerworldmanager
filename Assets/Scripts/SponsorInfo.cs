using UnityEngine;
using UnityEngine.Serialization;

public class SponsorInfo : MonoBehaviour
{
    [FormerlySerializedAs("_textureId")]
    public Texture2D textureId;
    public uint flags;
}
