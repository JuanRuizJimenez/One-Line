using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Clase para los botones de seleccion de nivel
/// Facilita el cambio de escena al nivel asociado al boton
/// </summary>
public class LevelButton : MonoBehaviour
{
    [Tooltip("SpriteSheetText con el numero del nivel")]
    public SpriteSheetText _levelSpriteSheetText;

    /// <summary>
    /// Carga el nivel asociado a este boton
    /// </summary>
    public void loadLevel()
    {
        // Obtenemos el nivel del componente SriteSheetText
        int level = int.Parse(_levelSpriteSheetText.text);
        // Asignamos el nivel en el gamemanager y cargamos la escena de juego
        GameManager.Instance().setLevel(level);
        SceneManager.LoadScene(2);
    }
}
