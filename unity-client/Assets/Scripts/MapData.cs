using UnityEngine;

[CreateAssetMenu(fileName = "NewMap", menuName = "SmashClone/Map Data")]
public class MapData : ScriptableObject
{
    public string mapName;
    public Sprite mapPortrait;
    [Tooltip("Escribe el nombre exacto de la escena (ej: 'Battlefield')")]
    public string sceneName; 
}