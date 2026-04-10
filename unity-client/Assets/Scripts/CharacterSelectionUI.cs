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

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        contenedorRoster = root.Q<VisualElement>("ContenedorRoster");
        p1NombreText = root.Q<Label>("P1NombreText");
        p1Retrato = root.Q<VisualElement>("P1Retrato");

        GenerarRoster();
    }

    void GenerarRoster()
    {
        foreach (CharacterData personaje in todosLosPersonajes)
        {
            Button btnPersonaje = new Button();
            btnPersonaje.style.backgroundImage = new StyleBackground(personaje.characterIcon);
            btnPersonaje.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            btnPersonaje.AddToClassList("character-button");
            btnPersonaje.RegisterCallback<ClickEvent>(ev => SeleccionarPersonajeP1(personaje));
            contenedorRoster.Add(btnPersonaje);
        }
    }

    void SeleccionarPersonajeP1(CharacterData personajeElegido)
    {
        p1NombreText.text = personajeElegido.characterName;
        p1Retrato.style.backgroundImage = new StyleBackground(personajeElegido.characterPortrait);
        p1Retrato.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;

        if(MatchManager.Instance != null)
        {
            MatchManager.Instance.player1Choice = personajeElegido;
        }
    }
}