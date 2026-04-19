using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public UIDocument document;

    // Login UI Elements
    private VisualElement loginPanel;
    private TextField usernameInput;
    private TextField passwordInput;
    private Button loginButton;
    private Label statusText;

    // Lobby UI Elements
    private VisualElement lobbyPanel;
    private TextField gameIdInput;
    private Button createGameButton;
    private Button joinGameButton;
    private Label lobbyStatusText;

    private void OnEnable()
    {
        if (document == null) document = GetComponent<UIDocument>();
        var root = document.rootVisualElement;

        // Búsqueda de elementos (Asegúrate de que los nombres coincidan en tu UI Builder)
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

        // Estado inicial
        loginPanel.style.display = DisplayStyle.Flex;
        lobbyPanel.style.display = DisplayStyle.None;

        // Asignar eventos
        loginButton.clicked += OnLoginClicked;
        createGameButton.clicked += OnCreateGameClicked;
        joinGameButton.clicked += OnJoinGameClicked;
    }

    private void OnDisable()
    {
        // Limpiar eventos para evitar memory leaks
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
                // Si falla por 401, el usuario probablemente no exista
            }
        });
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
                // Cargar la escena de selección de personajes (asume build index 1)
                SceneManager.LoadScene(1); 
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
                // Cargar la escena de selección de personajes
                SceneManager.LoadScene(1);
            }
            else
            {
                lobbyStatusText.text = "Error: " + message;
            }
        });
    }
}
