using UnityEngine;

public class MatchManager : MonoBehaviour
{
    private static MatchManager _instance;
    public static MatchManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MatchManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("MatchManager");
                    _instance = go.AddComponent<MatchManager>();
                }
            }
            return _instance;
        }
    }

    public CharacterData player1Choice;
    public CharacterData player2Choice;
    public MapData mapaElegido;

    public bool isHost = true;
    public int localPlayerChoice = -1;
    public int rivalPlayerChoice = -1;
    public string sceneNameToLoad;
    public string rivalUsername = "";
    public bool isVsCpu = false;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
}