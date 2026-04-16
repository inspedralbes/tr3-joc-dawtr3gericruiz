using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Estado del Jugador (Vidas y UI)")]
    public PlayerUI miInterfaz;
    public int vidasMaximas = 3;
    private int vidasActuales;
    public Transform puntoDeRespawn;
    public Sprite miFotoDePerfil;

    [Header("Movimiento y Salto")]
    public float velocidadCorrer = 16f;
    public float fuerzaSalto = 22f;
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
    private bool estaHaciendoRecovery = false;

    [Header("Combate General")]
    public Hitbox hitboxAtaque;
    public AttackData[] arsenalDeAtaques;
    private float multiplicadorCalculado = 1f;

    [Header("Proyectil (Ataque a Distancia)")]
    public GameObject proyectilPrefab;
    public Transform puntoDeDisparo;

    [Header("Combo Jab (J)")]
    public float comboWindowTime = 0.6f;
    private float currentComboTimer = 0f;
    private int comboStateJab = 0;

    [Header("Smash Attack (K)")]
    public float tiempoMaximoCarga = 2f;
    public float multiplicadorMaximo = 2f;
    private float tiempoCargaActual = 0f;

    [Header("Recovery (Salto + L)")]
    public float velocidadDashRecovery = 20f;
    public float duracionDashRecovery = 0.2f;
    private float timerDashRecovery = 0f;
    private bool yaUsoRecovery = false;

    [Header("Sistema de Daño (Smash)")]
    public float porcentajeDaño = 0f;
    public float peso = 100f;
    public bool estaEnHitstun = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        gravedadBase = rb.gravityScale;
        saltosRestantes = saltosMaximos;
        vidasActuales = vidasMaximas;
        if (miInterfaz != null)
        {
            miInterfaz.ActualizarVidas(vidasActuales);
        }
    }

    void Update()
    {
        if (estaEnHitstun) return;
        enSuelo = Physics2D.OverlapCircle(groundCheck.position, radioSuelo, capaSuelo);
        anim.SetBool("isGrounded", enSuelo);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("isAttacking", estaHaciendoAtaque || estaCargandoSmash);

        if (enSuelo)
        {
            saltosRestantes = saltosMaximos;
            yaUsoRecovery = false;

            if (rb.linearVelocity.y <= 0.1f)
            {
                estaHaciendoRecovery = false;
                anim.SetBool("isRecovering", false);
            }

            if (esCaidaRapida)
            {
                esCaidaRapida = false;
                rb.gravityScale = gravedadBase;
            }
            else
        {
            if (rb.linearVelocity.y < 0f)
            {
                anim.SetBool("isRecovering", false);
            }
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
        else if (estaHaciendoRecovery)
        {
            inputX = Input.GetAxisRaw("Horizontal");
            saltosRestantes = 0;
            if (timerDashRecovery > 0)
            {
                timerDashRecovery -= Time.deltaTime;
            }
        }
        else
        {
            ManejarEstadoLibre();
        }

        if (!anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack") && hitboxAtaque != null && hitboxAtaque.gameObject.activeSelf)
        {
            DesactivarHitbox();
            estaHaciendoAtaque = false;
            estaHaciendoRecovery = false;
            anim.SetBool("isRecovering", false);
        }
    }

    public void RecibirDaño(float dañoRecibido, Vector2 direccion, float empujeBase, float escalado)
    {
        porcentajeDaño += dañoRecibido;
        if (miInterfaz != null) miInterfaz.ActualizarPorcentaje(porcentajeDaño);

        float knockbackTotal = empujeBase + ((porcentajeDaño * dañoRecibido * escalado) / (peso * 0.1f));

        rb.linearVelocity = Vector2.zero;
        
        rb.AddForce(direccion * knockbackTotal, ForceMode2D.Impulse);

        float tiempoAturdimiento = knockbackTotal * 0.025f;
        
        StartCoroutine(RutinaHitstun(tiempoAturdimiento));
    }

    public void PerderVida()
    {
        vidasActuales--;
        if (miInterfaz != null) miInterfaz.ActualizarVidas(vidasActuales);

        if (vidasActuales > 0)
        {
            transform.position = puntoDeRespawn.position;
            rb.linearVelocity = Vector2.zero;
            porcentajeDaño = 0f;
            
            if (miInterfaz != null) miInterfaz.ActualizarPorcentaje(porcentajeDaño);
        }
        else
        {
            // GAME OVER para este personaje
            Debug.Log("¡El jugador ha sigut derrotadt!");
            gameObject.SetActive(false);
        }
    }

    private System.Collections.IEnumerator RutinaHitstun(float tiempo)
    {
        estaEnHitstun = true;
        DesactivarHitbox();
        estaHaciendoAtaque = false;
        estaCargandoSmash = false;
        estaHaciendoRecovery = false;
        anim.SetBool("isRecovering", false);
        anim.SetBool("CargandoSmash", false);

        yield return new WaitForSeconds(tiempo);
        
        estaEnHitstun = false;
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
        inputX = Input.GetAxisRaw("Horizontal");
        if (inputX > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (inputX < 0) transform.localScale = new Vector3(-1, 1, 1);

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
                estaHaciendoAtaque = false;
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

        if (Input.GetButton("Jump") && Input.GetKeyDown(KeyCode.L) && !yaUsoRecovery)
        {
            EjecutarRecovery();
        }
        else if (Input.GetKeyDown(KeyCode.J))
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

        else if (Input.GetKeyDown(KeyCode.L))
        {
            if (proyectilPrefab != null && puntoDeDisparo != null)
            {
                estaHaciendoAtaque = true;
                anim.SetTrigger("LanzarProyectil");
            }
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

    void EjecutarRecovery()
    {
        yaUsoRecovery = true;
        estaHaciendoRecovery = true;

        timerDashRecovery = duracionDashRecovery;
        anim.SetBool("isRecovering", true);
        anim.SetTrigger("HacerRecovery");
    }

    public void TerminarRecovery()
    {
        estaHaciendoRecovery = false;
        DesactivarHitbox();
    }

    void FixedUpdate()
    {
        if (estaEnHitstun) return;
        if (estaHaciendoRecovery && timerDashRecovery > 0)
        {
            rb.linearVelocity = new Vector2(inputX * velocidadCorrer, velocidadDashRecovery);
        }
        else
        {
            rb.linearVelocity = new Vector2(inputX * velocidadCorrer, rb.linearVelocity.y);
        }
    }

    public void ActivarHitboxJab(int indiceArsenal)
    {
        hitboxAtaque.ataqueActual = arsenalDeAtaques[indiceArsenal];
        hitboxAtaque.multiplicadorActual = multiplicadorCalculado;
        hitboxAtaque.gameObject.SetActive(true);
    }

    public void DesactivarHitbox()
    {
        if (hitboxAtaque != null)
        {
            hitboxAtaque.gameObject.SetActive(false);
            hitboxAtaque.ataqueActual = null;
            hitboxAtaque.multiplicadorActual = 1f;
        }
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
        DesactivarHitbox();
    }

    public void Disparar()
    {
        if (proyectilPrefab != null && puntoDeDisparo != null)
        {
            Quaternion rotacionProyectil = puntoDeDisparo.rotation;
            if (transform.localScale.x < 0)
            {
                rotacionProyectil = Quaternion.Euler(0, 180, 0);
            }
            GameObject proy = Instantiate(proyectilPrefab, puntoDeDisparo.position, rotacionProyectil);
            Proyectil scriptProy = proy.GetComponent<Proyectil>();
            if (scriptProy != null)
            {
                scriptProy.jugadorQueDispara = this;
            }
        }
    }

    public void FinalizarAtaque()
{
    estaHaciendoAtaque = false;
}
}