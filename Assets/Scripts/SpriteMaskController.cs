using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 2
/// Actividad 2.4. El bucle de juego y la animación
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: SpriteMaskController.cs
/// Responsabilidad: Gestionar máscaras de sprites (Sprite Masking) para delimitar,
/// revelar u ocultar áreas de animación en segundo plano o transiciones entre páginas.
/// </summary>
[RequireComponent(typeof(SpriteMask))]
public class SpriteMaskController : MonoBehaviour
{
    [Header("Configuración de Máscara de Sprites")]
    [Tooltip("Sprite utilizado como silueta o stencil de la máscara.")]
    [SerializeField] private Sprite maskSprite;

    [Tooltip("Interpolación de escala de la máscara para transiciones.")]
    [SerializeField] private float transitionSpeed = 3f;

    [Tooltip("Si es verdadero, la máscara se expande al activarse.")]
    [SerializeField] private bool expandOnStart = true;

    private SpriteMask spriteMask;
    private Vector3 targetScale;
    private Vector3 initialScale;

    private void Awake()
    {
        spriteMask = GetComponent<SpriteMask>();
        if (maskSprite != null && spriteMask != null)
        {
            spriteMask.sprite = maskSprite;
        }

        initialScale = transform.localScale;
        targetScale = initialScale;
    }

    private void Start()
    {
        if (expandOnStart)
        {
            transform.localScale = Vector3.zero;
            targetScale = initialScale;
        }
    }

    private void Update()
    {
        if (transform.localScale != targetScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * transitionSpeed);
        }
    }

    /// <summary>
    /// Expande la máscara para revelar la animación o escena subyacente.
    /// </summary>
    public void RevealMask()
    {
        targetScale = initialScale;
    }

    /// <summary>
    /// Contrae la máscara a cero para ocultar el contenido.
    /// </summary>
    public void HideMask()
    {
        targetScale = Vector3.zero;
    }

    /// <summary>
    /// Cambia dinámicamente la forma del sprite de máscara.
    /// </summary>
    public void SetMaskShape(Sprite newMaskShape)
    {
        if (spriteMask != null && newMaskShape != null)
        {
            maskSprite = newMaskShape;
            spriteMask.sprite = newMaskShape;
        }
    }
}
