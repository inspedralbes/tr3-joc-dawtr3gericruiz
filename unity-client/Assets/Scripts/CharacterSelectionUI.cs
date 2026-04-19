using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Linq;

public class CharacterSelectionUI : MonoBehaviour
{
    [Header("Base de Datos de Personajes")]
    public CharacterData[] todosLosPersonajes;

    private UIDocument uiDocument;
    
    private VisualElement contenedorRoster;
    private Label p1NombreText;
    private VisualElement p1Retrato;
    private Label p2NombreText;
    private VisualElement p2Retrato;
    private Button btnSeleccionar;

    private Label localNombreText;
    private VisualElement localRetrato;
    private Label rivalNombreText;
    private VisualElement rivalRetrato;

    private bool isLocalReady = false;
    private bool isRivalReady = false;

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        contenedorRoster = root.Q<VisualElement>("ContenedorRoster");
        p1NombreText = root.Q<Label>("P1NombreText");
        p1Retrato = root.Q<VisualElement>("P1Retrato");
        p2NombreText = root.Q<Label>("P2NombreText");
        p2Retrato = root.Q<VisualElement>("P2Retrato");

        if (MatchManager.Instance != null && MatchManager.Instance.isHost)
        {
            localNombreText = p1NombreText;
            localRetrato = p1Retrato;
            rivalNombreText = p2NombreText;
            rivalRetrato = p2Retrato;
        }
        else
        {
            localNombreText = p2NombreText;
            localRetrato = p2Retrato;
            rivalNombreText = p1NombreText;
            rivalRetrato = p1Retrato;
        }

        btnSeleccionar = root.Q<Button>("BtnSeleccionar");
        if (btnSeleccionar != null)
        {
            btnSeleccionar.clicked += OnSeleccionarClicked;
        }

        
        VisualElement pantalla = root.Q<VisualElement>("PantallaPrincipal");
        if (pantalla != null && ApiManager.Instance != null)
        {
            Label roomLabel = new Label();
            roomLabel.text = "SALA ID: " + ApiManager.Instance.CurrentGameId;
            roomLabel.style.fontSize = 40;
            roomLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            roomLabel.style.color = new StyleColor(Color.black);
            roomLabel.style.marginBottom = 20;
            pantalla.Insert(1, roomLabel);
        }

        
        if (NetworkManager.Instancia != null)
        {
            NetworkManager.Instancia.OnMapSelected += HandleMapSelected;
            NetworkManager.Instancia.OnCharacterSelected += HandleCharacterSelected;
            NetworkManager.Instancia.OnRivalReady += HandleRivalReady;
            NetworkManager.Instancia.OnStartMatch += HandleStartMatch;
        }

        
        
        if (MatchManager.Instance != null && MatchManager.Instance.isHost && !string.IsNullOrEmpty(MatchManager.Instance.sceneNameToLoad))
        {
            if (NetworkManager.Instancia != null)
            {
                
                string jsonMap = $"{{\"tipo\":\"map_selected\",\"mapName\":\"{MatchManager.Instance.sceneNameToLoad}\"}}";
                NetworkManager.Instancia.EnviarMensaje(jsonMap);
            }
        }

        GenerarRoster();
    }

    private void OnDisable()
    {
        if (btnSeleccionar != null)
        {
            btnSeleccionar.clicked -= OnSeleccionarClicked;
        }
        
        if (NetworkManager.Instancia != null)
        {
            NetworkManager.Instancia.OnMapSelected -= HandleMapSelected;
            NetworkManager.Instancia.OnCharacterSelected -= HandleCharacterSelected;
            NetworkManager.Instancia.OnRivalReady -= HandleRivalReady;
            NetworkManager.Instancia.OnStartMatch -= HandleStartMatch;
        }
    }

    private void HandleMapSelected(string mapName)
    {
        if (MatchManager.Instance != null)
        {
            MatchManager.Instance.sceneNameToLoad = mapName;
        }
    }

    private void HandleCharacterSelected(int index)
    {
        if (index >= 0 && index < todosLosPersonajes.Length)
        {
            CharacterData personajeElegido = todosLosPersonajes[index];
            rivalNombreText.text = personajeElegido.characterName;
            rivalRetrato.style.backgroundImage = new StyleBackground(personajeElegido.characterPortrait);
            rivalRetrato.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;

            if (MatchManager.Instance != null)
            {
                MatchManager.Instance.rivalPlayerChoice = index;
            }
        }
    }

    private void HandleRivalReady()
    {
        isRivalReady = true;
        rivalNombreText.text = "LLEST!\n" + rivalNombreText.text;
        rivalNombreText.style.color = new StyleColor(Color.green);
        CheckAmbosListos();
    }

    private void HandleStartMatch()
    {
        HandleStartMatch(null);
    }

    private void HandleStartMatch(string mapName)
    {
        // If a mapName was received in the message, store it
        if (!string.IsNullOrEmpty(mapName) && MatchManager.Instance != null)
        {
            MatchManager.Instance.sceneNameToLoad = mapName;
        }

        if (MatchManager.Instance != null && !string.IsNullOrEmpty(MatchManager.Instance.sceneNameToLoad))
        {
            SceneManager.LoadScene(MatchManager.Instance.sceneNameToLoad);
        }
        else
        {
            Debug.LogError("No s'ha pogut carregar l'escena: sceneNameToLoad esta buit!");
        }
    }

    private void OnSeleccionarClicked()
    {
        if (isLocalReady) return;
        if (MatchManager.Instance == null || MatchManager.Instance.localPlayerChoice == -1) return;

        isLocalReady = true;
        btnSeleccionar.text = "Esperant...";
        btnSeleccionar.style.backgroundColor = new StyleColor(Color.gray);
        localNombreText.text = "LLEST!\n" + localNombreText.text;
        localNombreText.style.color = new StyleColor(Color.green);

        if (NetworkManager.Instancia != null)
        {
            NetworkManager.Instancia.EnviarMensaje("{\"tipo\":\"player_ready\"}");
        }

        CheckAmbosListos();
    }

    private void CheckAmbosListos()
    {
        if (isLocalReady && isRivalReady && MatchManager.Instance != null && MatchManager.Instance.isHost)
        {
            string mapName = MatchManager.Instance.sceneNameToLoad;
            if (NetworkManager.Instancia != null)
            {
                string json = $"{{\"tipo\":\"start_match\",\"mapName\":\"{mapName}\"}}";
                NetworkManager.Instancia.EnviarMensaje(json);
            }
            HandleStartMatch(); 
        }
    }

    void GenerarRoster()
    {
        for (int i = 0; i < todosLosPersonajes.Length; i++)
        {
            int index = i;
            CharacterData personaje = todosLosPersonajes[index];
            Button btnPersonaje = new Button();
            btnPersonaje.style.backgroundImage = new StyleBackground(personaje.characterIcon);
            btnPersonaje.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            btnPersonaje.AddToClassList("character-button");
            btnPersonaje.RegisterCallback<ClickEvent>(ev => SeleccionarPersonajeLocal(index));
            contenedorRoster.Add(btnPersonaje);
        }
    }

    void SeleccionarPersonajeLocal(int index)
    {
        if (isLocalReady) return; 

        CharacterData personajeElegido = todosLosPersonajes[index];
        localNombreText.text = personajeElegido.characterName;
        localRetrato.style.backgroundImage = new StyleBackground(personajeElegido.characterPortrait);
        localRetrato.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;

        if(MatchManager.Instance != null)
        {
            MatchManager.Instance.localPlayerChoice = index;
            
            MatchManager.Instance.player1Choice = personajeElegido; 
        }

        if (NetworkManager.Instancia != null)
        {
            string json = $"{{\"tipo\":\"character_selected\",\"characterId\":{index}}}";
            NetworkManager.Instancia.EnviarMensaje(json);
        }
    }
}