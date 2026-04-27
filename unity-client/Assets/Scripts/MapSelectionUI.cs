using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MapSelectionUI : MonoBehaviour
{
    [Header("Base de Datos de Mapas")]
    public MapData[] todosLosMapas; 

    private UIDocument uiDocument;
    
    private VisualElement contenedorMapas;
    private Label nombreMapaText;
    private VisualElement imagenMapa;
    private Button btnSeleccionar;
    private Button btnAtras;

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        contenedorMapas = root.Q<VisualElement>("ContenedorMapas");
        nombreMapaText = root.Q<Label>("NombreMapaText");
        imagenMapa = root.Q<VisualElement>("ImagenMapa");
        
        btnSeleccionar = root.Q<Button>("BtnSeleccionar");
        if (btnSeleccionar != null)
        {
            btnSeleccionar.clicked += OnSeleccionarClicked;
        }

        btnAtras = root.Q<Button>("BtnAtras");
        if (btnAtras != null)
        {
            btnAtras.clicked += OnBtnAtrasClicked;
        }

        GenerarListaMapas();
    }

    private void OnDisable()
    {
        if (btnSeleccionar != null)
        {
            btnSeleccionar.clicked -= OnSeleccionarClicked;
        }
        if (btnAtras != null)
        {
            btnAtras.clicked -= OnBtnAtrasClicked;
        }
    }

    private void OnBtnAtrasClicked()
    {
        if (NetworkManager.Instancia != null)
        {
            NetworkManager.Instancia.Desconectar();
        }
        SceneManager.LoadScene("MainMenu");
    }

    private void OnSeleccionarClicked()
    {
        if (MatchManager.Instance != null && MatchManager.Instance.mapaElegido != null)
        {
            MatchManager.Instance.sceneNameToLoad = MatchManager.Instance.mapaElegido.sceneName;
            if (NetworkManager.Instancia != null)
            {
                string jsonMap = $"{{\"tipo\":\"map_selected\",\"mapName\":\"{MatchManager.Instance.mapaElegido.sceneName}\"}}";
                NetworkManager.Instancia.EnviarMensaje(jsonMap);
            }
            SceneManager.LoadScene("CharacterSelection");
        }
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