using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

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
    private Button btnAtras;

    private Label localNombreText;
    private VisualElement localRetrato;
    private Label rivalNombreText;
    private VisualElement rivalRetrato;

    private bool isLocalReady = false;
    private bool isRivalReady = false;

    // Propiedad rápida para saber si estamos en modo CPU
    private bool esModoVsCpu => MatchManager.Instance != null && MatchManager.Instance.esModoVsCpu;

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

        // Resetear selecciones anteriores por si venimos de otra partida
        if (MatchManager.Instance != null)
        {
            MatchManager.Instance.localPlayerChoice = -1;
            MatchManager.Instance.rivalPlayerChoice = -1;
        }

        // Mostrar el username del jugador local
        if (ApiManager.Instance != null && !string.IsNullOrEmpty(ApiManager.Instance.CurrentUsername))
        {
            localNombreText.text = ApiManager.Instance.CurrentUsername;
        }

        // Si es Vs CPU, preparamos los textos
        if (esModoVsCpu)
        {
            rivalNombreText.text = "CPU\n(Tria personatge)";
        }

        btnSeleccionar = root.Q<Button>("BtnSeleccionar");
        if (btnSeleccionar != null)
        {
            if (esModoVsCpu) btnSeleccionar.text = "Confirmar Jugador";
            btnSeleccionar.clicked += OnSeleccionarClicked;
        }

        btnAtras = root.Q<Button>("BtnAtras");
        if (btnAtras != null)
        {
            btnAtras.clicked += OnBtnAtrasClicked;
        }

        // Solo mostrar la ID de la sala si NO estamos contra la CPU
        VisualElement pantalla = root.Q<VisualElement>("PantallaPrincipal");
        if (!esModoVsCpu && pantalla != null && ApiManager.Instance != null)
        {
            Label roomLabel = new Label();
            roomLabel.text = "SALA ID: " + ApiManager.Instance.CurrentGameId;
            roomLabel.style.fontSize = 40;
            roomLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            roomLabel.style.color = new StyleColor(Color.black);
            roomLabel.style.marginBottom = 20;
            pantalla.Insert(1, roomLabel);
        }

        // Solo suscribir a eventos de red si NO estamos contra la CPU
        if (!esModoVsCpu && NetworkManager.Instancia != null)
        {
            NetworkManager.Instancia.OnMapSelected += HandleMapSelected;
            NetworkManager.Instancia.OnCharacterSelected += HandleCharacterSelected;
            NetworkManager.Instancia.OnRivalReady += HandleRivalReady;
            NetworkManager.Instancia.OnStartMatch += HandleStartMatch;
        }

        // Solo enviar el mapa por red si somos host y NO estamos contra la CPU
        if (!esModoVsCpu && MatchManager.Instance != null && MatchManager.Instance.isHost && !string.IsNullOrEmpty(MatchManager.Instance.sceneNameToLoad))
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
        if (btnSeleccionar != null) btnSeleccionar.clicked -= OnSeleccionarClicked;
        if (btnAtras != null) btnAtras.clicked -= OnBtnAtrasClicked;
        
        if (!esModoVsCpu && NetworkManager.Instancia != null)
        {
            NetworkManager.Instancia.OnMapSelected -= HandleMapSelected;
            NetworkManager.Instancia.OnCharacterSelected -= HandleCharacterSelected;
            NetworkManager.Instancia.OnRivalReady -= HandleRivalReady;
            NetworkManager.Instancia.OnStartMatch -= HandleStartMatch;
        }
    }

    private void OnBtnAtrasClicked()
    {
        if (!esModoVsCpu && NetworkManager.Instancia != null)
        {
            NetworkManager.Instancia.Desconectar();
        }
        SceneManager.LoadScene("MainMenu");
    }

    private void HandleMapSelected(string mapName)
    {
        if (MatchManager.Instance != null)
            MatchManager.Instance.sceneNameToLoad = mapName;
    }

    private void HandleCharacterSelected(int index, string rivalUsername)
    {
        if (index >= 0 && index < todosLosPersonajes.Length)
        {
            CharacterData personajeElegido = todosLosPersonajes[index];
            string displayName = string.IsNullOrEmpty(rivalUsername)
                ? personajeElegido.characterName
                : rivalUsername + "\n" + personajeElegido.characterName;
            rivalNombreText.text = displayName;

            rivalRetrato.style.backgroundImage = new StyleBackground(personajeElegido.characterPortrait);
            rivalRetrato.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;

            if (MatchManager.Instance != null)
            {
                MatchManager.Instance.rivalPlayerChoice = index;
                if (!string.IsNullOrEmpty(rivalUsername))
                    MatchManager.Instance.rivalUsername = rivalUsername;
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
        if (esModoVsCpu)
        {
            // PASO 1: Confirmar jugador local
            if (!isLocalReady)
            {
                if (MatchManager.Instance == null || MatchManager.Instance.localPlayerChoice == -1) return;
                
                isLocalReady = true;
                localNombreText.text = "LLEST!\n" + localNombreText.text;
                localNombreText.style.color = new StyleColor(Color.green);
                
                // Cambiar el botón para guiar al jugador al siguiente paso
                btnSeleccionar.text = "Confirmar CPU"; 
            }
            // PASO 2: Confirmar CPU
            else if (!isRivalReady)
            {
                if (MatchManager.Instance == null || MatchManager.Instance.rivalPlayerChoice == -1) return;

                isRivalReady = true;
                rivalNombreText.text = "LLEST!\n" + rivalNombreText.text;
                rivalNombreText.style.color = new StyleColor(Color.green);
                
                btnSeleccionar.text = "Iniciant...";
                btnSeleccionar.style.backgroundColor = new StyleColor(Color.gray);
                
                CheckAmbosListos();
            }
        }
        else
        {
            // MODO ONLINE NORMAL
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
    }

    private void CheckAmbosListos()
    {
        if (isLocalReady && isRivalReady)
        {
            if (esModoVsCpu)
            {
                HandleStartMatch();
            }
            else if (MatchManager.Instance != null && MatchManager.Instance.isHost)
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
            btnPersonaje.RegisterCallback<ClickEvent>(ev => SeleccionarPersonaje(index));
            contenedorRoster.Add(btnPersonaje);
        }
    }

    // He cambiado el nombre a SeleccionarPersonaje (ya que sirve para ambos ahora)
    void SeleccionarPersonaje(int index)
    {
        // Si estamos en online y ya estamos listos, no hacer nada.
        if (!esModoVsCpu && isLocalReady) return;
        
        // Si estamos en Vs CPU y ya hemos elegido ambos, no hacer nada.
        if (esModoVsCpu && isLocalReady && isRivalReady) return;

        CharacterData personajeElegido = todosLosPersonajes[index];

        if (!isLocalReady)
        {
            // SELECCIÓN DEL JUGADOR LOCAL
            string username = (ApiManager.Instance != null) ? ApiManager.Instance.CurrentUsername : "Jugador";
            localNombreText.text = username + "\n" + personajeElegido.characterName;

            localRetrato.style.backgroundImage = new StyleBackground(personajeElegido.characterPortrait);
            localRetrato.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;

            if(MatchManager.Instance != null)
            {
                MatchManager.Instance.localPlayerChoice = index;
                MatchManager.Instance.player1Choice = personajeElegido;
            }

            // Solo enviar por red si NO estamos contra la CPU
            if (!esModoVsCpu && NetworkManager.Instancia != null)
            {
                string json = $"{{\"tipo\":\"character_selected\",\"characterId\":{index},\"username\":\"{username}\"}}";
                NetworkManager.Instancia.EnviarMensaje(json);
            }
        }
        else if (esModoVsCpu && !isRivalReady)
        {
            // SELECCIÓN DE LA CPU (Si el jugador ya está listo)
            rivalNombreText.text = "CPU\n" + personajeElegido.characterName;

            rivalRetrato.style.backgroundImage = new StyleBackground(personajeElegido.characterPortrait);
            rivalRetrato.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;

            if (MatchManager.Instance != null)
            {
                MatchManager.Instance.rivalPlayerChoice = index;
                MatchManager.Instance.player2Choice = personajeElegido;
                MatchManager.Instance.rivalUsername = "CPU";
            }
        }
    }
}