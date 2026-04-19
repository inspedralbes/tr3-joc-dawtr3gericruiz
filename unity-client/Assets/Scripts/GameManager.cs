using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private PlayerController p1Controller;
    private PlayerController p2Controller;
    private bool partidaTerminada = false;

    [Header("UI de Partida")]
    public MatchUI uiPartida;

    [Header("Configuración de la Escena (Arrastrar de la jerarquía)")]
    public PlayerUI[] hudsJugadores;
    public Transform[] puntosDeSpawn;

    [Header("Personajes Disponibles (Arrastrar de la carpeta Prefabs)")]
    public GameObject prefabGojo;
    public GameObject prefabSukuna;

    void Start()
    {
        if (MatchManager.Instance != null)
        {
            int p1Choice = MatchManager.Instance.isHost ? MatchManager.Instance.localPlayerChoice : MatchManager.Instance.rivalPlayerChoice;
            int p2Choice = MatchManager.Instance.isHost ? MatchManager.Instance.rivalPlayerChoice : MatchManager.Instance.localPlayerChoice;
            
            IniciarPelea(p1Choice, p2Choice);
        }
    }

    private void ConfigurarJugador(GameObject jugadorInstanciado, int indiceJugador)
    {
        PlayerController controlador = jugadorInstanciado.GetComponent<PlayerController>();

        if (controlador != null)
        {
            if (puntosDeSpawn.Length > indiceJugador)
            {
                controlador.puntoDeRespawn = puntosDeSpawn[indiceJugador];
            }

            if (hudsJugadores.Length > indiceJugador && hudsJugadores[indiceJugador] != null)
            {
                controlador.miInterfaz = hudsJugadores[indiceJugador];
                controlador.miInterfaz.EstablecerRetrato(controlador.miFotoDePerfil);
                controlador.miInterfaz.ActualizarVidas(controlador.vidasMaximas);
                controlador.miInterfaz.ActualizarPorcentaje(0f);
            }
            else
            {
                Debug.LogError($"Falta assignar el HUD pel {indiceJugador + 1} en el GameManager.");
            }
        }
    }

    public void IniciarPelea(int eleccionP1, int eleccionP2)
    {
        bool isHost = MatchManager.Instance == null || MatchManager.Instance.isHost;

        GameObject prefabP1 = ObtenerPrefabPorID(eleccionP1);
        if (prefabP1 != null && puntosDeSpawn.Length > 0)
        {
            GameObject jugador1 = Instantiate(prefabP1, puntosDeSpawn[0].position, Quaternion.identity);
            ConfigurarJugador(jugador1, 0);
            
            p1Controller = jugador1.GetComponent<PlayerController>(); 
            p1Controller.esJugadorLocal = isHost; 
            if (NetworkManager.Instancia != null) {
                if (isHost) NetworkManager.Instancia.localController = p1Controller;
                else NetworkManager.Instancia.rivalController = p1Controller;
            }
            p1Controller.IniciarSecuenciaIntro(3f);
        }

        GameObject prefabP2 = ObtenerPrefabPorID(eleccionP2);
        if (prefabP2 != null && puntosDeSpawn.Length > 1)
        {
            GameObject jugador2 = Instantiate(prefabP2, puntosDeSpawn[1].position, Quaternion.identity);
            ConfigurarJugador(jugador2, 1);
            
            p2Controller = jugador2.GetComponent<PlayerController>();
            p2Controller.esJugadorLocal = !isHost; 

            if (NetworkManager.Instancia != null) {
                if (!isHost) NetworkManager.Instancia.localController = p2Controller;
                else NetworkManager.Instancia.rivalController = p2Controller;
            }

            p2Controller.IniciarSecuenciaIntro(3f);
        }
        
        StartCoroutine(RutinaCuentaAtras());
    }

    private GameObject ObtenerPrefabPorID(int idPersonaje)
    {
        switch (idPersonaje)
        {
            case 0: return prefabGojo;
            case 1: return prefabSukuna;
            default:
                Debug.LogWarning("ID de personatge no vàlida. Per defecte s'assigna Gojo.");
                return prefabGojo;
        }
    }

    private System.Collections.IEnumerator RutinaCuentaAtras()
    {
        if (uiPartida == null)
        {
            Debug.LogWarning("Falta assignar el MatchUI en el GameManager.");
            yield break;
        }

        uiPartida.ActualizarTexto("3");
        yield return new WaitForSeconds(1f);

        uiPartida.ActualizarTexto("2");
        yield return new WaitForSeconds(1f);

        uiPartida.ActualizarTexto("1");
        yield return new WaitForSeconds(1f);

        uiPartida.ActualizarTexto("JA!");
        yield return new WaitForSeconds(1f);

        uiPartida.ActualizarTexto("");
    }
    
    public void ComprobarVictoria()
    {
        if (partidaTerminada) return;

        if (p1Controller != null && p1Controller.vidasActuales <= 0)
        {
            TerminarPartida(2); 
        }
        else if (p2Controller != null && p2Controller.vidasActuales <= 0)
        {
            TerminarPartida(1); 
        }
    }

    private void TerminarPartida(int ganador)
    {
        partidaTerminada = true;
        
        if (uiPartida != null)
        {
            uiPartida.MostrarPantallaVictoria(ganador);
        }

        
        if (ApiManager.Instance != null && !string.IsNullOrEmpty(ApiManager.Instance.CurrentGameId))
        {
            string winnerId = (ganador == 1) ? ApiManager.Instance.CurrentUserId : "rival_id";
            ApiManager.Instance.SaveResult(winnerId, Time.timeSinceLevelLoad, (success) => {
                if(success) Debug.Log("Resultado guardado en el servidor.");
                else Debug.LogError("Error al guardar el resultado.");
            });
        }

        Time.timeScale = 0f;
    }
}