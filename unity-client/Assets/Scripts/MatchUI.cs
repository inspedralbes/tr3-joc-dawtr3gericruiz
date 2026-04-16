using UnityEngine;
using UnityEngine.UIElements;

public class MatchUI : MonoBehaviour
{
    private Label textoCuentaAtras;
    private VisualElement winScreenPanel;
    private Label winnerText;

    void OnEnable()
    {
        UIDocument uiDoc = GetComponent<UIDocument>();
        
        if (uiDoc != null)
        {
            var root = uiDoc.rootVisualElement;
            textoCuentaAtras = root.Q<Label>("TextoCuentaAtras");
            winScreenPanel = root.Q<VisualElement>("WinScreenPanel");
            winnerText = root.Q<Label>("WinnerText");
            if (winScreenPanel != null)
            {
                winScreenPanel.style.display = DisplayStyle.None;
            }
        }
    }

    public void ActualizarTexto(string nuevoTexto)
    {
        if (textoCuentaAtras != null)
        {
            textoCuentaAtras.text = nuevoTexto;
        }
    }

    public void MostrarPantallaVictoria(int jugadorGanador)
    {
        if (winScreenPanel != null && winnerText != null)
        {
            winnerText.text = "JUGADOR " + jugadorGanador + " GANA!";
            winScreenPanel.style.display = DisplayStyle.Flex; 
        }
    }
}