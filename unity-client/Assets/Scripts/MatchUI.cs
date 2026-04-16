using UnityEngine;
using UnityEngine.UIElements;

public class MatchUI : MonoBehaviour
{
    private Label textoCuentaAtras;

    void OnEnable()
    {
        UIDocument uiDoc = GetComponent<UIDocument>();
        
        if (uiDoc != null)
        {
            var root = uiDoc.rootVisualElement;
            textoCuentaAtras = root.Q<Label>("TextoCuentaAtras");
        }
    }

    public void ActualizarTexto(string nuevoTexto)
    {
        if (textoCuentaAtras != null)
        {
            textoCuentaAtras.text = nuevoTexto;
        }
    }
}