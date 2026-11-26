using UnityEngine;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 3: Desarrollo de videojuegos 2D
/// Actividad 3.9. Pruebas de resultados y recorrido ampliado
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: AbyssFallTrigger2D.cs
/// Responsabilidad: Detectar si el jugador cae a un abismo entre plataformas y reaparecerlo
/// de forma segura en el punto de control sin reiniciar el juego ni perder monedas.
/// </summary>
public class AbyssFallTrigger2D : MonoBehaviour
{
    [Header("Punto de Reaparición")]
    [Tooltip("Punto seguro donde reaparece el jugador al caer al abismo.")]
    [SerializeField] private Vector3 respawnPoint = new Vector3(0f, -2.0f, 0f);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.name.Contains("Player"))
        {
            Debug.Log("[AbyssFallTrigger2D] ¡El jugador cayó al abismo! Reapareciendo en punto seguro...");
            
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            other.transform.position = respawnPoint;
        }
    }

    public void SetRespawnPoint(Vector3 newPoint)
    {
        respawnPoint = newPoint;
    }
}
