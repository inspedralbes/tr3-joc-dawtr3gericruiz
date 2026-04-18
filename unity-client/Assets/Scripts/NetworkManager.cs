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
    private string urlServidor = "ws://localhost:3000";

    public PlayerController rivalController;

    private void Awake()
    {
        if (Instancia == null) { Instancia = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private async void Start()
    {
        ws = new ClientWebSocket();
        try
        {
            await ws.ConnectAsync(new Uri(urlServidor), CancellationToken.None);
            Debug.Log("Connectat al servidor WebSocket.");
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
}