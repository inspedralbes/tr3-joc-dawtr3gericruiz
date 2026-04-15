using UnityEngine;
using UnityEngine.UIElements;

public class PlayerUI : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement fotoPersonaje;
    private Label textoPorcentaje;
    private Label textoVidas;

    [Header("Configuración Visual")]
    public Color colorCeroDaño = Color.white;
    public Color colorPeligro = Color.red;
    public float dañoParaPeligroMaximo = 150f;

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;
        fotoPersonaje = root.Q<VisualElement>("FotoPersonaje");
        textoPorcentaje = root.Q<Label>("TextoPorcentaje");
        textoVidas = root.Q<Label>("TextoVidas");
        ActualizarPorcentaje(0f);
    }

    public void ActualizarPorcentaje(float dañoActual)
    {
        if (textoPorcentaje == null) return;

        textoPorcentaje.text = dañoActual.ToString("F0") + "%";
        float proporcionDaño = Mathf.Clamp01(dañoActual / dañoParaPeligroMaximo);
        Color colorActual = Color.Lerp(colorCeroDaño, colorPeligro, proporcionDaño);
        
        textoPorcentaje.style.color = new StyleColor(colorActual);
    }

    public void ActualizarVidas(int vidasRestantes)
    {
        if (textoVidas == null) return;
        
        textoVidas.text = "Vidas: " + vidasRestantes;
    }

    public void EstablecerRetrato(Sprite nuevaFoto)
    {
        if (fotoPersonaje != null && nuevaFoto != null)
        {
            fotoPersonaje.style.backgroundImage = new StyleBackground(nuevaFoto);
        }
    }
}