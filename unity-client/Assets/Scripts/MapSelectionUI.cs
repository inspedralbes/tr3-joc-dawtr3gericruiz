using UnityEngine;
using UnityEngine.UIElements;

public class MapSelectionUI : MonoBehaviour
{
    [Header("Base de Datos de Mapas")]
    public MapData[] todosLosMapas; 

    private UIDocument uiDocument;
    
    private VisualElement contenedorMapas;
    private Label nombreMapaText;
    private VisualElement imagenMapa;

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        contenedorMapas = root.Q<VisualElement>("ContenedorMapas");
        nombreMapaText = root.Q<Label>("NombreMapaText");
        imagenMapa = root.Q<VisualElement>("ImagenMapa");

        GenerarListaMapas();
    }

    void GenerarListaMapas()
    {
        foreach (MapData mapa in todosLosMapas)
        {
            Button btnMapa = new Button();
            
            btnMapa.style.backgroundImage = new StyleBackground(mapa.mapPortrait); 
            
            btnMapa.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            btnMapa.AddToClassList("map-button"); 
            
            btnMapa.RegisterCallback<ClickEvent>(ev => SeleccionarMapa(mapa));
            contenedorMapas.Add(btnMapa);
        }
    }

    void SeleccionarMapa(MapData mapaElegido)
    {
        nombreMapaText.text = mapaElegido.mapName;
        
        imagenMapa.style.backgroundImage = new StyleBackground(mapaElegido.mapPortrait);
        
        imagenMapa.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;

        if(MatchManager.Instance != null)
        {
            MatchManager.Instance.mapaElegido = mapaElegido; 
        }
    }
}