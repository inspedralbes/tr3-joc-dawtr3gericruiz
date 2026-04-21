using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private PlayerController p1Controller;
    private PlayerController p2Controller;
    private bool partidaTerminada = false;
    private float tiempoInicioPartida = 0f;

    [Header("UI de Partida")]
    public MatchUI uiPartida;

    [Header("Configuración de la Escena")]
    public PlayerUI[] hudsJugadores;
    public Transform[] puntosDeSpawn;

    [Header("Personajes Disponibles")]
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
            if (puntosDeSpawn.Length > indiceJugador) controlador.puntoDeRespawn = puntosDeSpawn[indiceJugador];
            if (hudsJugadores.Length > indiceJugador && hudsJugadores[indiceJugador] != null)
            {
                controlador.miInterfaz = hudsJugadores[indiceJugador];
                controlador.miInterfaz.EstablecerRetrato(controlador.miFotoDePerfil);
                controlador.miInterfaz.ActualizarVidas(controlador.vidasMaximas);
                controlador.miInterfaz.ActualizarPorcentaje(0f);
            }
        }
    }

    private void ConfigurarCerebroBot(GameObject jugadorInstanciado, bool esBot)
    {
        SimpleBotAgent botAgent = jugadorInstanciado.GetComponent<SimpleBotAgent>();
        Unity.MLAgents.DecisionRequester dr = jugadorInstanciado.GetComponent<Unity.MLAgents.DecisionRequester>();

        if (!esBot)
        {
            if (dr != null) Destroy(dr);
            if (botAgent != null) Destroy(botAgent);
        }
    }

    public void IniciarPelea(int eleccionP1, int eleccionP2)
    {
        bool isHost = MatchManager.Instance == null || MatchManager.Instance.isHost;
        bool esVsCpu = MatchManager.Instance != null && MatchManager.Instance.esModoVsCpu;

        GameObject prefabP1 = ObtenerPrefabPorID(eleccionP1);
        if (prefabP1 != null && puntosDeSpawn.Length > 0)
        {
            GameObject jugador1 = Instantiate(prefabP1, puntosDeSpawn[0].position, Quaternion.identity);
            ConfigurarJugador(jugador1, 0);
            
            p1Controller = jugador1.GetComponent<PlayerController>(); 
            p1Controller.esJugadorLocal = isHost; 
            p1Controller.esBot = false;
            ConfigurarCerebroBot(jugador1, false);

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
            jugador2.transform.localScale = new Vector3(-1, 1, 1);
            ConfigurarJugador(jugador2, 1);
            
            p2Controller = jugador2.GetComponent<PlayerController>();

            if (esVsCpu)
            {
                p2Controller.esJugadorLocal = false; 
                p2Controller.esBot = true;
                ConfigurarCerebroBot(jugador2, true);
                
                SimpleBotAgent botScript = jugador2.GetComponent<SimpleBotAgent>();
                if (botScript != null) botScript.isPlayerOne = false;
            }
            else
            {
                p2Controller.esJugadorLocal = !isHost; 
                p2Controller.esBot = false;
                ConfigurarCerebroBot(jugador2, false);
            }

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
            default: return prefabGojo;
        }
    }

    private System.Collections.IEnumerator RutinaCuentaAtras()
    {
        if (uiPartida == null) yield break;
        uiPartida.ActualizarTexto("3");
        yield return new WaitForSeconds(1f);
        uiPartida.ActualizarTexto("2");
        yield return new WaitForSeconds(1f);
        uiPartida.ActualizarTexto("1");
        yield return new WaitForSeconds(1f);
        uiPartida.ActualizarTexto("JA!");
        yield return new WaitForSeconds(1f);
        uiPartida.ActualizarTexto("");
        tiempoInicioPartida = Time.timeSinceLevelLoad;
    }
    
    public void ComprobarVictoria()
    {
        if (partidaTerminada) return;
        if (p1Controller != null && p1Controller.vidasActuales <= 0) TerminarPartida(2); 
        else if (p2Controller != null && p2Controller.vidasActuales <= 0) TerminarPartida(1); 
    }

    private void TerminarPartida(int ganador)
    {
        partidaTerminada = true;
        float duracion = Time.timeSinceLevelLoad - tiempoInicioPartida;
        PlayerController ganadorController = (ganador == 1) ? p1Controller : p2Controller;
        int vidasRestantes = (ganadorController != null) ? ganadorController.vidasActuales : 0;
        bool localEsGanador = (MatchManager.Instance == null) || (ganador == 1 && MatchManager.Instance.isHost) || (ganador == 2 && !MatchManager.Instance.isHost);
        string usernameGanador;
        if (localEsGanador) usernameGanador = (ApiManager.Instance != null && !string.IsNullOrEmpty(ApiManager.Instance.CurrentUsername)) ? ApiManager.Instance.CurrentUsername : "Jugador " + ganador;
        else usernameGanador = (!string.IsNullOrEmpty(MatchManager.Instance.rivalUsername)) ? MatchManager.Instance.rivalUsername : "Rival";
        if (uiPartida != null) uiPartida.MostrarPantallaVictoria(usernameGanador, vidasRestantes, duracion);
        if (ApiManager.Instance != null && !string.IsNullOrEmpty(ApiManager.Instance.CurrentGameId))
        {
            string winnerId = localEsGanador ? ApiManager.Instance.CurrentUserId : "rival_id";
            ApiManager.Instance.SaveResult(winnerId, duracion, (success) => {});
        }
        Time.timeScale = 0f;
    }
}