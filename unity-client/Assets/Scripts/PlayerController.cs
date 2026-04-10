using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento y Salto")]
    public float velocidadCorrer = 8f;
    public float fuerzaSalto = 25f;
    public int saltosMaximos = 2;
    private int saltosRestantes;
    private float inputX;

    [Header("Caída Rápida")]
    public float multiplicadorCaidaRapida = 3f;
    public float impulsoInstantaneo = 5f;
    private bool esCaidaRapida = false;
    private float gravedadBase;

    [Header("Detección de Suelo")]
    public Transform groundCheck;
    public float radioSuelo = 0.2f;
    public LayerMask capaSuelo;
    public bool enSuelo;

    private Rigidbody2D rb;
    private Animator anim;

    private bool estaCargandoSmash = false;
    private bool estaHaciendoAtaque = false;

    [Header("Combate General")]
    public Hitbox hitboxAtaque;
    public AttackData[] arsenalDeAtaques;
    private float multiplicadorCalculado = 1f;

    [Header("Combo Jab (J)")]
    public float comboWindowTime = 0.6f; 
    private float currentComboTimer = 0f;
    private int comboStateJab = 0;

    [Header("Smash Attack (K)")]
    public float tiempoMaximoCarga = 2f;
    public float multiplicadorMaximo = 2f;
    private float tiempoCargaActual = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        gravedadBase = rb.gravityScale;
        saltosRestantes = saltosMaximos;
    }

    void Update()
    {
        enSuelo = Physics2D.OverlapCircle(groundCheck.position, radioSuelo, capaSuelo);
        anim.SetBool("isGrounded", enSuelo);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);

        if (enSuelo)
        {
            saltosRestantes = saltosMaximos;
            if (esCaidaRapida)
            {
                esCaidaRapida = false;
                rb.gravityScale = gravedadBase;
            }
        }

        if (estaCargandoSmash)
        {
            ManejarEstadoCargaSmash();
        }
        else if (estaHaciendoAtaque)
        {
            HandleJabComboState();
        }
        else
        {
            ManejarEstadoLibre();
        }
    }

    private void ManejarEstadoCargaSmash()
    {
        inputX = 0;
        
        if (Input.GetKey(KeyCode.K) && tiempoCargaActual < tiempoMaximoCarga)
        {
            tiempoCargaActual += Time.deltaTime;
        }
        else if (Input.GetKeyUp(KeyCode.K) || tiempoCargaActual >= tiempoMaximoCarga)
        {
            float porcentajeCarga = tiempoCargaActual / tiempoMaximoCarga;
            multiplicadorCalculado = Mathf.Lerp(1f, multiplicadorMaximo, porcentajeCarga);
            anim.SetTrigger("LanzarSmash");

            estaCargandoSmash = false;
            estaHaciendoAtaque = true;
            anim.SetBool("CargandoSmash", false);
        }
    }

    private void HandleJabComboState()
    {
        inputX = 0;

        if (comboStateJab == 2)
        {
            currentComboTimer += Time.deltaTime;
            
            if (Input.GetKeyDown(KeyCode.J) || Input.GetKey(KeyCode.J))
            {
                anim.SetBool("HacerComboJab", true);
                comboStateJab = 3;
            }
            else if (currentComboTimer >= comboWindowTime)
            {
                comboStateJab = 0;
            }
        }
    }

    private void ManejarEstadoLibre()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        anim.SetBool("isRunning", inputX != 0);

        if (inputX > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (inputX < 0) transform.localScale = new Vector3(-1, 1, 1);

        ManejarMovimientoVertical();

        if (Input.GetKeyDown(KeyCode.J) && enSuelo)
        {
            multiplicadorCalculado = 1f;
            estaHaciendoAtaque = true;
            comboStateJab = 1;
            anim.SetTrigger("JabStart");
        }
        else if (Input.GetKeyDown(KeyCode.K) && enSuelo)
        {
            estaCargandoSmash = true;
            tiempoCargaActual = 0f;
            anim.SetBool("CargandoSmash", true);
        }
    }

    void ManejarMovimientoVertical()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (enSuelo) EjecutarSalto();
            else if (saltosRestantes > 1) { EjecutarSalto(); saltosRestantes--; }
        }
        
        if (!enSuelo && rb.linearVelocity.y <= 0.1f && !esCaidaRapida)
        {
            if (Input.GetAxisRaw("Vertical") < 0)
            {
                esCaidaRapida = true;
                rb.gravityScale = gravedadBase * multiplicadorCaidaRapida;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y - impulsoInstantaneo);
            }
        }
    }

    void EjecutarSalto()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.linearVelocity += Vector2.up * fuerzaSalto;
        if (esCaidaRapida) { esCaidaRapida = false; rb.gravityScale = gravedadBase; }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(inputX * velocidadCorrer, rb.linearVelocity.y);
    }

    public void ActivarHitboxJab(int indiceArsenal)
    {
        hitboxAtaque.ataqueActual = arsenalDeAtaques[indiceArsenal];
        hitboxAtaque.multiplicadorActual = multiplicadorCalculado;
        hitboxAtaque.gameObject.SetActive(true);
    }

    public void DesactivarHitbox()
    {
        hitboxAtaque.gameObject.SetActive(false);
        hitboxAtaque.ataqueActual = null;
        hitboxAtaque.multiplicadorActual = 1f;
    }

    public void EntrarEnComboGap()
    {
        comboStateJab = 2;
        currentComboTimer = 0f;
    }

    public void TerminarSecuenciaJab()
    {
        estaHaciendoAtaque = false;
        comboStateJab = 0;
        anim.SetBool("HacerComboJab", false);
    }
}