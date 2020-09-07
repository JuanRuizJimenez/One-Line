using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Clase que implemente la funcionalidad de los botones de las distintas dificultades
/// del menu principal (cambio de escena, asignacion del sprite apropiado etc
/// Implementa PointerDown y PointerUp para cambiar su sprite de acuerdo a pulsaciones
/// Su ordenacion (y del boton challenge) la hace unity con los componentes layoutMember y gridLayoutGroup
/// </summary>
public class MainMenuButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Tooltip("SpriteSheetText que muestra niveles disponibles/totales")]
    public SpriteSheetText _levelsText;
    [Tooltip("Componente button")]
    public Button _button;
    [Tooltip("Imagen que muestra el nombre de la dificultad")]
    public Image _diffTextImage;
    [Tooltip("Dificultad asociada al boton")]
    public int _difficulty = 0; 

    private GameManager _gameManager; // GameManager
    private Sprite _buttonSprite; // Sprite del boton en reposo
    private Sprite _pressedButtonSprite; // Sprite al ser pulsado

    private string _buttonImagePath = "Sprites/Title/bottom0"; // Ruta para la imagen del boton en reposo
    private string _pressedImagePath = "Sprites/Title/bottom0_press"; // Ruta para la imagen del boton pulsado
    private string _diffImagePath = "Sprites/Texts/title_0_en"; // Ruta para la imagen con el texto dificultad


    public void Start()
    {
        _gameManager = GameManager.Instance();
        // Asignamos el texto de los niveles disponibles/totales
        _levelsText.text = _gameManager.getLevelIndex(_difficulty).ToString().PadLeft(1, '0')
                  + "S" + _gameManager.getLevelsPerDifficulty(_difficulty);
        _levelsText.createSprites();

        // Cargamos el sprite del texto de la dificultad
        _diffTextImage.sprite = Resources.Load<Sprite>(_diffImagePath.Replace("0", _difficulty.ToString()));

        // Cargamos el sprite del boton en reposo
        _buttonSprite = Resources.Load<Sprite>(_buttonImagePath.Replace("0", _difficulty.ToString()));
        // Si no se encuentra se deja la de dificultad 0
        if(_buttonSprite == null) _buttonSprite = Resources.Load<Sprite>(_buttonImagePath);
        _button.image.sprite = _buttonSprite;

        // Cargamos el sprite de boton puldaso
        _pressedButtonSprite = Resources.Load<Sprite>(_pressedImagePath.Replace("0", _difficulty.ToString()));
        // Si no se encuentra se deja la de dificultad 0
        if (_pressedButtonSprite == null) _pressedButtonSprite = Resources.Load<Sprite>(_pressedImagePath);
    }

    /// <summary>
    /// Lleva a la escena de seleccion de niveles con la dificultad seleccionada
    /// </summary>
    public void ToLevelSelector()
    {
        GameManager.Instance().setDifficulty(_difficulty);
        SceneManager.LoadScene(1);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Asignamos sprite de pulsacion
        if (!_button.interactable) return;
        _button.image.sprite = _pressedButtonSprite;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Asignamos sprite default
        if (!_button.interactable) return;
        _button.image.sprite = _buttonSprite;
    }
}
