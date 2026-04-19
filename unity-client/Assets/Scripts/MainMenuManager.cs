using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public UIDocument document;

    
    private VisualElement loginPanel;
    private TextField usernameInput;
    private TextField passwordInput;
    private Button loginButton;
    private Label statusText;

    
    private VisualElement lobbyPanel;
    private TextField gameIdInput;
    private Button createGameButton;
    private Button joinGameButton;
    private Label lobbyStatusText;

    private void OnEnable()
    {
        if (document == null) document = GetComponent<UIDocument>();
        var root = document.rootVisualElement;

        
        loginPanel = root.Q<VisualElement>("LoginPanel");
        usernameInput = root.Q<TextField>("UsernameInput");
        passwordInput = root.Q<TextField>("PasswordInput");
        loginButton = root.Q<Button>("LoginButton");
        statusText = root.Q<Label>("StatusText");

        lobbyPanel = root.Q<VisualElement>("LobbyPanel");
        gameIdInput = root.Q<TextField>("GameIdInput");
        createGameButton = root.Q<Button>("CreateGameButton");
        joinGameButton = root.Q<Button>("JoinGameButton");
        lobbyStatusText = root.Q<Label>("LobbyStatusText");

        
        loginPanel.style.display = DisplayStyle.Flex;
        lobbyPanel.style.display = DisplayStyle.None;

        
        loginButton.clicked += OnLoginClicked;
        createGameButton.clicked += OnCreateGameClicked;
        joinGameButton.clicked += OnJoinGameClicked;
    }

    private void OnDisable()
    {
        
        if (loginButton != null) loginButton.clicked -= OnLoginClicked;
        if (createGameButton != null) createGameButton.clicked -= OnCreateGameClicked;
        if (joinGameButton != null) joinGameButton.clicked -= OnJoinGameClicked;
    }

    private void OnLoginClicked()
    {
        string username = usernameInput.value;
        string password = passwordInput.value;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Error: Usuario y contraseña requeridos.";
            return;
        }

        statusText.text = "Conectando...";
        loginButton.SetEnabled(false);

        ApiManager.Instance.Login(username, password, (success, message) =>
        {
            loginButton.SetEnabled(true);
            if (success)
            {
                statusText.text = "Login exitoso!";
                loginPanel.style.display = DisplayStyle.None;
                lobbyPanel.style.display = DisplayStyle.Flex;
            }
            else
            {
                statusText.text = "Error: " + message;
                
            }
        });
    }

    private void InitNetworkManager(string gameId)
    {
        if (NetworkManager.Instancia == null)
        {
            GameObject netObj = new GameObject("NetworkManager");
            NetworkManager.Instancia = netObj.AddComponent<NetworkManager>();
        }
        NetworkManager.Instancia.Conectar(gameId);
    }

    private void OnCreateGameClicked()
    {
        createGameButton.SetEnabled(false);
        joinGameButton.SetEnabled(false);
        lobbyStatusText.text = "Creando partida...";

        ApiManager.Instance.CreateGame((success, gameIdOrError) =>
        {
            createGameButton.SetEnabled(true);
            joinGameButton.SetEnabled(true);

            if (success)
            {
                lobbyStatusText.text = "Partida creada! ID: " + gameIdOrError;
                Debug.Log("ID Partida Creada: " + gameIdOrError);
                if (MatchManager.Instance != null) MatchManager.Instance.isHost = true;
                
                InitNetworkManager(gameIdOrError);
                SceneManager.LoadScene("MapSelection"); 
            }
            else
            {
                lobbyStatusText.text = "Error: " + gameIdOrError;
            }
        });
    }

    private void OnJoinGameClicked()
    {
        string gameId = gameIdInput.value;
        if (string.IsNullOrEmpty(gameId))
        {
            lobbyStatusText.text = "Introduce un ID de partida";
            return;
        }

        createGameButton.SetEnabled(false);
        joinGameButton.SetEnabled(false);
        lobbyStatusText.text = "Uniéndose...";

        ApiManager.Instance.JoinGame(gameId, (success, message) =>
        {
            createGameButton.SetEnabled(true);
            joinGameButton.SetEnabled(true);

            if (success)
            {
                lobbyStatusText.text = "Unido a la partida!";
                if (MatchManager.Instance != null) MatchManager.Instance.isHost = false;
                
                InitNetworkManager(gameId);
                SceneManager.LoadScene("CharacterSelection");
            }
            else
            {
                lobbyStatusText.text = "Error: " + message;
            }
        });
    }
}
