using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public UIDocument document;

    // ── Login ──────────────────────────────────────────────────────
    private VisualElement loginPanel;
    private TextField usernameInput;
    private TextField passwordInput;
    private Button loginButton;
    private Button goToRegisterButton;
    private Label statusText;

    // ── Register ───────────────────────────────────────────────────
    private VisualElement registerPanel;
    private TextField regUsernameInput;
    private TextField regPasswordInput;
    private TextField regConfirmPasswordInput;
    private Button registerButton;
    private Button goToLoginButton;
    private Label regStatusText;

    // ── Lobby ──────────────────────────────────────────────────────
    private VisualElement lobbyPanel;
    private TextField gameIdInput;
    private Button createGameButton;
    private Button joinGameButton;
    private Button vsCpuButton;
    private Label lobbyStatusText;

    private void OnEnable()
    {
        if (document == null) document = GetComponent<UIDocument>();
        var root = document.rootVisualElement;

        // Login
        loginPanel         = root.Q<VisualElement>("LoginPanel");
        usernameInput      = root.Q<TextField>("UsernameInput");
        passwordInput      = root.Q<TextField>("PasswordInput");
        loginButton        = root.Q<Button>("LoginButton");
        goToRegisterButton = root.Q<Button>("GoToRegisterButton");
        statusText         = root.Q<Label>("StatusText");

        // Register
        registerPanel           = root.Q<VisualElement>("RegisterPanel");
        regUsernameInput        = root.Q<TextField>("RegUsernameInput");
        regPasswordInput        = root.Q<TextField>("RegPasswordInput");
        regConfirmPasswordInput = root.Q<TextField>("RegConfirmPasswordInput");
        registerButton          = root.Q<Button>("RegisterButton");
        goToLoginButton         = root.Q<Button>("GoToLoginButton");
        regStatusText           = root.Q<Label>("RegStatusText");

        // Lobby
        lobbyPanel       = root.Q<VisualElement>("LobbyPanel");
        gameIdInput      = root.Q<TextField>("GameIdInput");
        createGameButton = root.Q<Button>("CreateGameButton");
        joinGameButton   = root.Q<Button>("JoinGameButton");
        vsCpuButton      = root.Q<Button>("VsCpuButton");
        lobbyStatusText  = root.Q<Label>("LobbyStatusText");

        // Forçar codi de sala en majúscules
        if (gameIdInput != null)
        {
            gameIdInput.RegisterValueChangedCallback(evt =>
            {
                string upper = evt.newValue.ToUpper();
                if (evt.newValue != upper)
                    gameIdInput.SetValueWithoutNotify(upper);
            });
        }

        // ── Subscriure TOTS els events aquí ────────────────────────────
        // UI Toolkit registra perfectament els events d'elements ocults. 
        if (loginButton != null) loginButton.clicked               += OnLoginClicked;
        if (goToRegisterButton != null) goToRegisterButton.clicked += OnGoToRegisterClicked;
        if (registerButton != null) registerButton.clicked         += OnRegisterClicked;
        if (goToLoginButton != null) goToLoginButton.clicked       += OnGoToLoginClicked;
        
        if (createGameButton != null) createGameButton.clicked     += OnCreateGameClicked;
        if (joinGameButton != null) joinGameButton.clicked         += OnJoinGameClicked;
        if (vsCpuButton != null) vsCpuButton.clicked               += OnVsCpuClicked;

        // Decidir quin panell mostrar
        if (ApiManager.Instance != null && !string.IsNullOrEmpty(ApiManager.Instance.AuthToken))
            MostrarLobby();
        else
        {
            loginPanel.style.display    = DisplayStyle.Flex;
            registerPanel.style.display = DisplayStyle.None;
            lobbyPanel.style.display    = DisplayStyle.None;
        }
    }

    private void OnDisable()
    {
        if (loginButton != null) loginButton.clicked               -= OnLoginClicked;
        if (goToRegisterButton != null) goToRegisterButton.clicked -= OnGoToRegisterClicked;
        if (registerButton != null) registerButton.clicked         -= OnRegisterClicked;
        if (goToLoginButton != null) goToLoginButton.clicked       -= OnGoToLoginClicked;
        if (createGameButton != null) createGameButton.clicked     -= OnCreateGameClicked;
        if (joinGameButton != null) joinGameButton.clicked         -= OnJoinGameClicked;
        if (vsCpuButton != null) vsCpuButton.clicked               -= OnVsCpuClicked;
    }

    private void MostrarLobby()
    {
        loginPanel.style.display    = DisplayStyle.None;
        registerPanel.style.display = DisplayStyle.None;
        lobbyPanel.style.display    = DisplayStyle.Flex;
        
        // Com hem traslladat els estils al UXML i les subscripcions al OnEnable,
        // aquesta funció ara només s'encarrega d'intercanviar els panells.
    }

    private void OnVsCpuClicked()
    {
        Debug.Log("[MainMenuManager] Jugar contra CPU!");
        if (MatchManager.Instance != null)
        {
            MatchManager.Instance.isHost      = true;
            MatchManager.Instance.esModoVsCpu = true;
        }
        SceneManager.LoadScene("MapSelection");
    }

    // ── Navegació panells ──────────────────────────────────────────
    private void OnGoToRegisterClicked()
    {
        loginPanel.style.display    = DisplayStyle.None;
        registerPanel.style.display = DisplayStyle.Flex;
        statusText.text    = "";
        regStatusText.text = "";
    }

    private void OnGoToLoginClicked()
    {
        registerPanel.style.display = DisplayStyle.None;
        loginPanel.style.display    = DisplayStyle.Flex;
        statusText.text    = "";
        regStatusText.text = "";
    }

    // ── Register ───────────────────────────────────────────────────
    private void OnRegisterClicked()
    {
        string username        = regUsernameInput.value;
        string password        = regPasswordInput.value;
        string confirmPassword = regConfirmPasswordInput.value;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            regStatusText.text = "Error: Usuari i contrasenya requerits.";
            return;
        }
        if (password != confirmPassword)
        {
            regStatusText.text = "Error: Les contrasenyes no coincideixen.";
            return;
        }

        regStatusText.text = "Creant compte...";
        registerButton.SetEnabled(false);

        ApiManager.Instance.Register(username, password, (success, message) =>
        {
            registerButton.SetEnabled(true);
            if (success)
            {
                regStatusText.text = "Compte creat! Pots iniciar sessió.";
                regStatusText.style.color = new StyleColor(new Color(0.2f, 0.8f, 0.2f));
                regUsernameInput.value        = "";
                regPasswordInput.value        = "";
                regConfirmPasswordInput.value = "";
                Invoke("OnGoToLoginClicked", 1.5f);
            }
            else
            {
                regStatusText.text = "Error: " + message;
                regStatusText.style.color = new StyleColor(new Color(0.8f, 0.2f, 0.2f));
            }
        });
    }

    // ── Login ──────────────────────────────────────────────────────
    private void OnLoginClicked()
    {
        string username = usernameInput.value;
        string password = passwordInput.value;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Error: Usuari i contrasenya requerits.";
            return;
        }

        statusText.text = "Connectant...";
        loginButton.SetEnabled(false);

        ApiManager.Instance.Login(username, password, (success, message) =>
        {
            loginButton.SetEnabled(true);
            if (success)
            {
                statusText.text = "Inici de sessió exitós!";
                MostrarLobby();
            }
            else
            {
                statusText.text = "Error: " + message;
            }
        });
    }

    // ── Xarxa ──────────────────────────────────────────────────────
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
        lobbyStatusText.text = "Creant partida...";

        ApiManager.Instance.CreateGame((success, gameIdOrError) =>
        {
            createGameButton.SetEnabled(true);
            joinGameButton.SetEnabled(true);

            if (success)
            {
                lobbyStatusText.text = "Partida creada! ID: " + gameIdOrError;
                if (MatchManager.Instance != null)
                {
                    MatchManager.Instance.isHost      = true;
                    MatchManager.Instance.esModoVsCpu = false;
                }
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
            lobbyStatusText.text = "Introdueix un ID de partida";
            return;
        }

        createGameButton.SetEnabled(false);
        joinGameButton.SetEnabled(false);
        lobbyStatusText.text = "Unint-se...";

        ApiManager.Instance.JoinGame(gameId, (success, message) =>
        {
            createGameButton.SetEnabled(true);
            joinGameButton.SetEnabled(true);

            if (success)
            {
                lobbyStatusText.text = "Unit a la partida!";
                if (MatchManager.Instance != null)
                {
                    MatchManager.Instance.isHost      = false;
                    MatchManager.Instance.esModoVsCpu = false;
                }
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