using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance;

    public CharacterData player1Choice;
    public CharacterData player2Choice;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}