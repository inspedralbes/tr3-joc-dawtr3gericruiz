using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instancia;
    
    private ClientWebSocket ws;
    private string urlServidor = "ws://157.180.36.176:3000";
    public PlayerController rivalController;
    public PlayerController localController;

    public event Action<string> OnMapSelected;
    public event Action<int> OnCharacterSelected;
    public event Action OnRivalReady;
    public event Action<string> OnStartMatch;

    private void Awake()
    {
        if (Instancia == null) { Instancia = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        
    }

    public async void Conectar(string gameId)
    {
        if (ws != null && ws.State == WebSocketState.Open) return;

        ws = new ClientWebSocket();
        try
        {
            await ws.ConnectAsync(new Uri(urlServidor), CancellationToken.None);
            Debug.Log("Connectat al servidor WebSocket.");

            
            string joinMsg = $"{{\"tipo\":\"join_room\",\"gameId\":\"{gameId}\"}}";
            EnviarMensaje(joinMsg);

            EscucharMensajes();
        }
        catch (Exception e)
        {
            Debug.LogError("Error de connexio: " + e.Message);
        }
    }

    private async void EscucharMensajes()
    {
        byte[] buffer = new byte[1024];
        while (ws.State == WebSocketState.Open)
        {
            WebSocketReceiveResult resultado = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            string mensaje = Encoding.UTF8.GetString(buffer, 0, resultado.Count);
            
            ProcesarMensaje(mensaje);
        }
    }

    private void ProcesarMensaje(string json)
    {
        try 
        {
            DatosRed datos = JsonUtility.FromJson<DatosRed>(json);

            if (datos.tipo == "movimiento" && rivalController != null)
            {
                rivalController.transform.position = new Vector3(datos.posX, datos.posY, 0);
                rivalController.transform.localScale = new Vector3(datos.scaleX, 1, 1);
                rivalController.inputX = (datos.scaleX > 0) ? 1 : -1; 
            }
            else if (datos.tipo == "accion" && rivalController != null)
            {
                if (datos.accion == "jab") 
                {
                    rivalController.EjecutarJab();
                }
                else if (datos.accion == "proyectil")
                {
                    rivalController.EjecutarProyectil();
                }
            }

            else if (datos.tipo == "golpe" && localController != null)
            {
                Vector2 direccion = new Vector2(datos.dirX, datos.dirY);
                localController.RecibirDaño(datos.daño, direccion, datos.empujeBase, datos.escalado);
                Debug.Log("Hem rebut un cop del rival!");
            }
            else if (datos.tipo == "map_selected")
            {
                OnMapSelected?.Invoke(datos.mapName);
            }
            else if (datos.tipo == "character_selected")
            {
                OnCharacterSelected?.Invoke(datos.characterId);
            }
            else if (datos.tipo == "player_ready")
            {
                OnRivalReady?.Invoke();
            }
            else if (datos.tipo == "start_match")
            {
                OnStartMatch?.Invoke(datos.mapName);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error processant JSON: " + e.Message);
        }
    }

    public void EnviarMovimiento(float x, float y, float scaleX)
    {
        if (ws != null && ws.State == WebSocketState.Open)
        {
            DatosRed datos = new DatosRed { tipo = "movimiento", posX = x, posY = y, scaleX = scaleX };
            string json = JsonUtility.ToJson(datos);
            
            EnviarMensaje(json);
        }
    }

    public void EnviarAccion(string nombreAccion)
    {
        if (ws != null && ws.State == WebSocketState.Open)
        {
            DatosRed datos = new DatosRed { tipo = "accion", accion = nombreAccion };
            string json = JsonUtility.ToJson(datos);
            
            Debug.Log("Enviant accio al servidor: " + nombreAccion);
            
            EnviarMensaje(json);
        }
    }

    public void EnviarGolpe(float daño, float dirX, float dirY, float empujeBase, float escalado)
    {
        if (ws != null && ws.State == WebSocketState.Open)
        {
            DatosRed datos = new DatosRed 
            { 
                tipo = "golpe", 
                daño = daño,
                dirX = dirX,
                dirY = dirY,
                empujeBase = empujeBase,
                escalado = escalado
            };
            string json = JsonUtility.ToJson(datos);
            EnviarMensaje(json);
        }
    }

    public async void EnviarMensaje(string mensaje)
    {
        if (ws != null && ws.State == WebSocketState.Open)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(mensaje);
            await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    private async void OnApplicationQuit()
    {
        if (ws != null && ws.State == WebSocketState.Open)
        {
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Tancant el joc", CancellationToken.None);
        }
    }
}

[System.Serializable]
public class DatosRed
{
    public string tipo; 
    public float posX;
    public float posY;
    public float scaleX;
    public string accion; 
    
    public float daño;
    public float dirX;
    public float dirY;
    public float empujeBase;
    public float escalado;

    
    public string mapName;
    public int characterId;
}