using UnityEngine;

[CreateAssetMenu(fileName = "NuevoAtaque", menuName = "Ataque/Datos de Ataque")]
public class AttackData : ScriptableObject
{
    public string nombreAtaque;
    public float daño = 10f;
    [Header("Físicas de Smash")]
    public float fuerzaEmpujeBase = 15f;
    public float escaladoEmpuje = 1.5f;
}