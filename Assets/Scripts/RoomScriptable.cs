using UnityEngine;

[CreateAssetMenu(fileName = "Room", menuName = "Scriptable Objects/Room")]
public class RoomScriptable : ScriptableObject
{
    public RoomType roomType;
    public int[] occupiedTiles;
    public Sprite[] roomVariations;
}
