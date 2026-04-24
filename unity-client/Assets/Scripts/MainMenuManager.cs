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
    private Button goToRegisterButton;
    private Label statusText;

    private VisualElement registerPanel;
    private TextField regUsernameInput;
    private TextField regPasswordInput;
    private TextField regConfirmPasswordInput;
    private Button registerButton;
    private Button goToLoginButton;
    private Label regStatusText;

    private VisualElement lobbyPanel;
    private TextField gameIdInput;
    private Button createGameButton;
    private Button joinGameButton;
    private Label lobbyStatusText;

    private void OnEnable()
    {
        if (document == null) document = GetComponent<UIDocument>();
        var root = document.rootVisualElement;

        loginPanel         = root.Q<VisualElement>("LoginPanel");
        usernameInput      = root.Q<TextField>("UsernameInput");
        passwordInput      = root.Q<TextField>("PasswordInput");
        loginButton        = root.Q<Button>("LoginButton");
        goToRegisterButton = root.Q<Button>("GoToRegisterButton");
        statusText         = root.Q<Label>("StatusText");

        registerPanel           = root.Q<VisualElement>("RegisterPanel");
        regUsernameInput        = root.Q<TextField>("RegUsernameInput");
        regPasswordInput        = root.Q<TextField>("RegPasswordInput");
        regConfirmPasswordInput = root.Q<TextField>("RegConfirmPasswordInput");
        registerButton          = root.Q<Button>("RegisterButton");
        goToLoginButton         = root.Q<Button>("GoToLoginButton");
        regStatusText           = root.Q<Label>("RegStatusText");

        lobbyPanel       = root.Q<VisualElement>("LobbyPanel");
        gameIdInput      = root.Q<TextField>("GameIdInput");
        createGameButton = root.Q<Button>("CreateGameButton");
        joinGameButton   = root.Q<Button>("JoinGameButton");
        lobbyStatusText  = root.Q<Label>("LobbyStatusText");

        if (gameIdInput != null)
        {
            gameIdInput.RegisterValueChangedCallback(evt =>
            {
                string upper = evt.newValue.ToUpper();
                if (evt.newValue != upper)
                    gameIdInput.SetValueWithoutNotify(upper);
            });
        }

        if (loginButton != null) loginButton.clicked               += OnLoginClicked;
        if (goToRegisterButton != null) goToRegisterButton.clicked += OnGoToRegisterClicked;
        if (registerButton != null) registerButton.clicked         += OnRegisterClicked;
        if (goToLoginButton != null) goToLoginButton.clicked       += OnGoToLoginClicked;
        if (createGameButton != null) createGameButton.clicked     += OnCreateGameClicked;
        if (joinGameButton != null) joinGameButton.clicked         += OnJoinGameClicked;

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
    }

    private void MostrarLobby()
    {
        loginPanel.style.display    = DisplayStyle.None;
        registerPanel.style.display = DisplayStyle.None;
        lobbyPanel.style.display    = DisplayStyle.Flex;
    }

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
                Invoke("OnGoToLoginClicked", 1.5f);
            }
            else
            {
                regStatusText.text = "Error: " + message;
            }
        });
    }

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
                    MatchManager.Instance.isHost = true;
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
                    MatchManager.Instance.isHost = false;
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