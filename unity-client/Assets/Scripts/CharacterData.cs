using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "SmashClone/Character Data")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public Sprite characterIcon;
    public Sprite characterPortrait;
    public GameObject gameplayPrefab;
}