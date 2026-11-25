using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 2
/// Actividad 2.3. Programación de botones
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: FontTextController.cs
/// Responsabilidad: Controlar tipografías, tamaños, estilos y contraste
/// de los elementos de texto en la aplicación interactiva de escritorio.
/// </summary>
public class FontTextController : MonoBehaviour
{
    [Header("Elementos de Texto a Controlar")]
    [Tooltip("Lista de componentes Text de la interfaz que responderán a los botones.")]
    [SerializeField] private List<Text> targetTextElements = new List<Text>();

    [Header("Configuración de Tamaño de Fuente")]
    [SerializeField] private int minFontSize = 14;
    [SerializeField] private int maxFontSize = 36;
    [SerializeField] private int fontStepSize = 2;
    [SerializeField] private int currentFontSize = 18;

    [Header("Colección de Fuentes Disponibles")]
    [Tooltip("Diferentes familias tipográficas cargadas en Assets (ej. Arial, Calibri, Comic, etc.).")]
    [SerializeField] private List<Font> availableFonts = new List<Font>();
    private int currentFontIndex = 0;

    [Header("Paletas de Color para Accesibilidad y Lectura")]
    [SerializeField] private Color lightModeTextColor = new Color(0.12f, 0.12f, 0.12f, 1f); // Gris oscuro
    [SerializeField] private Color darkModeTextColor = new Color(0.95f, 0.95f, 0.95f, 1f);  // Blanco suave
    [SerializeField] private Color sepiaModeTextColor = new Color(0.35f, 0.25f, 0.15f, 1f); // Marrón lectura
    private int currentColorModeIndex = 0;

    private void Start()
    {
        ApplyCurrentSettings();
    }

    /// <summary>
    /// Incrementa el tamaño tipográfico de los textos objetivo.
    /// Vinculable al botón (+) en la UI.
    /// </summary>
    public void IncreaseFontSize()
    {
        if (currentFontSize + fontStepSize <= maxFontSize)
        {
            currentFontSize += fontStepSize;
            ApplyFontSize();
        }
    }

    /// <summary>
    /// Reduce el tamaño tipográfico de los textos objetivo.
    /// Vinculable al botón (-) en la UI.
    /// </summary>
    public void DecreaseFontSize()
    {
        if (currentFontSize - fontStepSize >= minFontSize)
        {
            currentFontSize -= fontStepSize;
            ApplyFontSize();
        }
    }

    /// <summary>
    /// Alterna cíclicamente entre las fuentes tipográficas configuradas en availableFonts.
    /// Vinculable al botón [Cambiar Fuente] en la UI.
    /// </summary>
    public void CycleFontFamily()
    {
        if (availableFonts.Count == 0) return;

        currentFontIndex = (currentFontIndex + 1) % availableFonts.Count;
        Font selectedFont = availableFonts[currentFontIndex];

        foreach (Text t in targetTextElements)
        {
            if (t != null && selectedFont != null)
            {
                t.font = selectedFont;
            }
        }
    }

    /// <summary>
    /// Alterna el estilo de fuente entre Normal y Negrita (Bold).
    /// </summary>
    public void ToggleBoldStyle()
    {
        foreach (Text t in targetTextElements)
        {
            if (t != null)
            {
                t.fontStyle = (t.fontStyle == FontStyle.Bold) ? FontStyle.Normal : FontStyle.Bold;
            }
        }
    }

    /// <summary>
    /// Alterna los modos de color del texto (Claro, Oscuro, Sepia).
    /// Vinculable al botón [Modo de Lectura] en la UI.
    /// </summary>
    public void CycleTextColorMode()
    {
        currentColorModeIndex = (currentColorModeIndex + 1) % 3;
        Color chosenColor;

        switch (currentColorModeIndex)
        {
            case 0:
                chosenColor = lightModeTextColor;
                break;
            case 1:
                chosenColor = darkModeTextColor;
                break;
            case 2:
                chosenColor = sepiaModeTextColor;
                break;
            default:
                chosenColor = lightModeTextColor;
                break;
        }

        foreach (Text t in targetTextElements)
        {
            if (t != null)
            {
                t.color = chosenColor;
            }
        }
    }

    /// <summary>
    /// Aplica el tamaño de fuente actual a todos los textos registrados.
    /// </summary>
    private void ApplyFontSize()
    {
        foreach (Text t in targetTextElements)
        {
            if (t != null)
            {
                t.fontSize = currentFontSize;
            }
        }
    }

    /// <summary>
    /// Aplica todas las configuraciones iniciales.
    /// </summary>
    private void ApplyCurrentSettings()
    {
        ApplyFontSize();

        if (availableFonts.Count > 0 && availableFonts[0] != null)
        {
            foreach (Text t in targetTextElements)
            {
                if (t != null)
                {
                    t.font = availableFonts[0];
                }
            }
        }
    }
}
