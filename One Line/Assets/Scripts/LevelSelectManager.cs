using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Clase que se encarga de la funcionalidad general de la escena de seleccion de nivel
/// Asigna los spriteSheetText de la dificultad e instancia tantos botones como niveles
/// hay en la dificultad (bloqueados o desbloqueados)
/// 
/// El GameObject que contiene este componente además actua como padre de los botones,
/// con el componente gridLayoutGroup, unity se encarga de su colocacion.
/// Tambien tiene el componente scrollRect de unity que proporciona el comportamiento 
/// de scroll (en nuestro caso vertical)
/// </summary>
public class LevelSelectManager : MonoBehaviour
{
    [Tooltip("Prefab del boton para un nivel desbloqueado")]
    public GameObject _levelButton;

    [Tooltip("Prefab de una imagen de nivel bloqueado")]
    public GameObject _levelLocked;

    [Tooltip("Imagen que muestra la dificultad actual")]
    public Image _diffImage;

    // Dificultad actual
    private int _difficulty;

    // Gamemanager
    private GameManager _gameManager; 

    private string textPath = "Sprites/Texts/title_0_en";

    // Start is called before the first frame update
    private void Start()
    {
        _gameManager = GameManager.Instance();
        _difficulty = (int)_gameManager.getDifficulty();
        Sprite textSprite = Resources.Load<Sprite>(textPath.Replace("0", _difficulty.ToString()));
        _diffImage.sprite = textSprite;

        generateButtons();
    }

    // Metodo que crea los botones para nLevels niveles
    private void generateButtons()
    {
        // Obtenemos nlevels para la dificultad desde el gamemanager
        int nLevels = _gameManager.getLevelsPerDifficulty(_difficulty);
        // Obtenemos el numero de niveles desbloqueados en esta dificultad desde el gamemanager
        int levelIndex = _gameManager.getLevelIndex(_difficulty);

        GameObject aux;
        // Creamos nLevel Objetos
        for (int i = 0; i < nLevels; i++)
        {
            // Desbloqueado, creamos boton
            if (i < levelIndex)
            {
                aux = Instantiate(_levelButton, this.transform);
                // Asignamos el texto del SpriteSheetText que contiene
                aux.GetComponentInChildren<SpriteSheetText>().text = (i + 1).ToString().PadLeft(3, '0');
            }
            // Bloqueado, creamos imagen
            else
            {
                aux = Instantiate(_levelLocked, this.transform);
            }
        }
    }
}
