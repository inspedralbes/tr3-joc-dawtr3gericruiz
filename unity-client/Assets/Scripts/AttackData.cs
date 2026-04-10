using UnityEngine;

[CreateAssetMenu(fileName = "NuevoAtaque", menuName = "Ataque/Datos de Ataque")]
public class AttackData : ScriptableObject
{
    public string nombreAtaque;
    public float daño = 10f;
    public float fuerzaEmpuje = 15f;
}