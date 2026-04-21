using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MatchUI : MonoBehaviour
{
    private Label textoCuentaAtras;
    private VisualElement winScreenPanel;
    private Label winnerText;
    private Label vidasLabel;
    private Label duracionLabel;
    private Button lobbyButton;

    void OnEnable()
    {
        UIDocument uiDoc = GetComponent<UIDocument>();
        
        if (uiDoc != null)
        {
            var root = uiDoc.rootVisualElement;
            textoCuentaAtras = root.Q<Label>("TextoCuentaAtras");
            winScreenPanel   = root.Q<VisualElement>("WinScreenPanel");
            winnerText       = root.Q<Label>("WinnerText");
            vidasLabel       = root.Q<Label>("VidasLabel");
            duracionLabel    = root.Q<Label>("DuracionLabel");
            lobbyButton      = root.Q<Button>("LobbyButton");

            if (winScreenPanel != null)
                winScreenPanel.style.display = DisplayStyle.None;

            if (lobbyButton != null)
                lobbyButton.clicked += OnLobbyButtonClicked;
        }
    }

    void OnDisable()
    {
        if (lobbyButton != null)
            lobbyButton.clicked -= OnLobbyButtonClicked;
    }

    public void ActualizarTexto(string nuevoTexto)
    {
        if (textoCuentaAtras != null)
            textoCuentaAtras.text = nuevoTexto;
    }

    /// <summary>
    /// Muestra el panel de victoria con el username del ganador,
    /// sus vidas restantes y la duración de la partida.
    /// </summary>
    public void MostrarPantallaVictoria(string usernameGanador, int vidasRestantes, float duracionSegundos)
    {
        if (winScreenPanel == null) return;

        // Nombre del ganador
        if (winnerText != null)
            winnerText.text = usernameGanador;

        // Vidas restantes del ganador
        if (vidasLabel != null)
            vidasLabel.text = vidasRestantes.ToString();

        // Duración formateada m:ss
        if (duracionLabel != null)
        {
            int minutos = Mathf.FloorToInt(duracionSegundos / 60f);
            int segundos = Mathf.FloorToInt(duracionSegundos % 60f);
            duracionLabel.text = $"{minutos}:{segundos:D2}";
        }

        winScreenPanel.style.display = DisplayStyle.Flex;
    }

    /// <summary>
    /// Sobrecarga de compatibilidad — muestra solo el número de jugador (sin datos detallados).
    /// </summary>
    public void MostrarPantallaVictoria(int jugadorGanador)
    {
        string nombre = "Jugador " + jugadorGanador;
        MostrarPantallaVictoria(nombre, 0, Time.timeSinceLevelLoad);
    }

    private void OnLobbyButtonClicked()
    {
        // Restablecer tiempo (estaba en 0 por Time.timeScale = 0)
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}