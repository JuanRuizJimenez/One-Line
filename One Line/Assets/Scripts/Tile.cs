using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase encargada de gestionar el funcionamiento individual de cada tile del tablero
/// Proporciona la funcionalidad de cambiar su apariencia en función de si forma parte
/// del camino o no y el control de esta
/// </summary>

public class Tile : MonoBehaviour
{
    // Control de posicion del tile
    private int _x = 0;
    private int _y = 0;

    // Controla si el tile forma parte del camino o no
    private bool _pressed = false;

    // Controla si es un tile que servira de inicio en el tablero
    private bool _initial = false;

    // Control de sprites en función de si está en el camino o no
    [Tooltip("Cuando el bloque no forma parte del camino")]
    public SpriteRenderer _defaultSprite;

    [Tooltip("Cuando el bloque forma parte del camino")]
    public SpriteRenderer _pathSprite;

    // Control de sprites para generar el camino de manera visual
    [Tooltip("Camino hacia arriba")]
    public SpriteRenderer _upPathSprite;

    [Tooltip("Camino hacia abajo")]
    public SpriteRenderer _downPathSprite;

    [Tooltip("Camino hacia la derecha")]
    public SpriteRenderer _rightPathSprite;

    [Tooltip("Camino hacia left")]
    public SpriteRenderer _leftPathSprite;

    // Control de sprites para generar la pista de manera visual
    [Tooltip("Pista hacia arriba")]
    public SpriteRenderer _upHintsSprite;

    [Tooltip("Pista hacia abajo")]
    public SpriteRenderer _downHintsSprite;

    [Tooltip("Pista hacia la derecha")]
    public SpriteRenderer _rightHintsSprite;

    [Tooltip("Pista hacia left")]
    public SpriteRenderer _leftHintsSprite;

    /// <summary>
    /// Actualiza la posición X del tile
    /// </summary>
    /// 
    /// <param name="x">
    /// Nueva posicion en el eje X
    /// </param>
    public void setX(int x)
    {
        _x = x;
    }

    /// <summary>
    /// Actualiza la posición Y del tile
    /// </summary>
    /// 
    /// <param name="y">
    /// Nueva posicion en el eje Y
    /// </param>
    public void setY(int y)
    {
        _y = y;
    }

    /// <summary>
    ///  Actualiza la posición X e Y del tile
    /// </summary>
    /// 
    /// <param name="x">
    /// Nueva posicion en el eje X
    /// </param>
    /// <param name="y">
    /// Nueva posicion en el eje Y
    /// </param>
    public void setPosition(int x, int y)
    {
        _x = x;
        _y = y;
    }

    /// <summary>
    /// Devuelve la posicion X del tile
    /// </summary>
    /// 
    /// <returns>
    /// Posicion X del tile
    /// </returns>
    public int getX()
    {
        return _x;
    }

    /// <summary>
    /// Devuelve la posicion Y del tile
    /// </summary>
    /// 
    /// <returns>
    /// Posicion Y del tile
    /// </returns>
    public int getY()
    {
        return _y;
    }

    /// <summary>
    /// Asigna el tile como inicial o no segun un valor dado
    /// </summary>
    /// 
    /// <param name="value"></param>
    public void SetInitial(bool value)
    {
        _initial = value;
    }

    /// <summary>
    /// Indica si es o no un tile inicial de tablero
    /// </summary>
    /// 
    /// <returns></returns>
    public bool IsInitial()
    {
        return _initial;
    }

    /// <summary>
    /// Cambia de estado el tile y su apariencia en función de un valor dado
    /// </summary>
    /// 
    /// <param name="value"> 
    /// Indica si el tile pasa a formar parte del camino o no 
    /// </param>
    public void SetPressed(bool value)
    {
        // Si ha sido pulsado y pasa a formar parte del camino
        if (value)
        {
            _defaultSprite.gameObject.SetActive(false);
            _pathSprite.gameObject.SetActive(true);
        }
        // En caso contrario
        else
        {
            _defaultSprite.gameObject.SetActive(true);
            _pathSprite.gameObject.SetActive(false);
            SetPath(PathType.NOT_DEFINED);
        }

        _pressed = value;
    }

    /// <summary>
    /// Devuelve un booleano indicando si forma parte del camino o no
    /// </summary>
    /// 
    /// <returns>
    /// Booleano indicando si forma parte del camino o no
    /// </returns>
    public bool IsPressed()
    {
        return _pressed;
    }

    /// <summary>
    /// Activa el camino de puntos en funcion de un tipo dado
    /// </summary>
    /// 
    /// <param name="type">
    /// Indica el tipo del camino
    /// </param>
    public void SetPath(PathType type)
    {
        switch(type)
        {
            // Camino hacia arriba
            case PathType.UP:
                _downPathSprite.gameObject.SetActive(false);
                _upPathSprite.gameObject.SetActive(true);
                _rightPathSprite.gameObject.SetActive(false);
                _leftPathSprite.gameObject.SetActive(false);
                break;

            // Camino hacia abajo
            case PathType.DOWN:
                _downPathSprite.gameObject.SetActive(true);
                _upPathSprite.gameObject.SetActive(false);
                _rightPathSprite.gameObject.SetActive(false);
                _leftPathSprite.gameObject.SetActive(false);
                break;

            // Camino hacia la derecha
            case PathType.RIGHT:
                _downPathSprite.gameObject.SetActive(false);
                _upPathSprite.gameObject.SetActive(false);
                _rightPathSprite.gameObject.SetActive(true);
                _leftPathSprite.gameObject.SetActive(false);
                break;

            // Camino hacia la izquierda
            case PathType.LEFT:
                _downPathSprite.gameObject.SetActive(false);
                _upPathSprite.gameObject.SetActive(false);
                _rightPathSprite.gameObject.SetActive(false);
                _leftPathSprite.gameObject.SetActive(true);
                break;

            // No tiene camino definido, no forma parte del camino
            case PathType.NOT_DEFINED:
                _downPathSprite.gameObject.SetActive(false);
                _upPathSprite.gameObject.SetActive(false);
                _rightPathSprite.gameObject.SetActive(false);
                _leftPathSprite.gameObject.SetActive(false);
                break;

            default:
                _downPathSprite.gameObject.SetActive(false);
                _upPathSprite.gameObject.SetActive(false);
                _rightPathSprite.gameObject.SetActive(false);
                _leftPathSprite.gameObject.SetActive(false);
                break;
        }
    }

    /// <summary>
    /// Activa la pista de puntos en funcion de un tipo dado
    /// </summary>
    /// 
    /// <param name="type">
    /// Indica el tipo de la pista
    /// </param>
    public void SetHint(PathType type)
    {
        switch (type)
        {
            // Camino hacia arriba
            case PathType.UP:
                _downHintsSprite.gameObject.SetActive(false);
                _upHintsSprite.gameObject.SetActive(true);
                _rightHintsSprite.gameObject.SetActive(false);
                _leftHintsSprite.gameObject.SetActive(false);
                break;

            // Camino hacia abajo
            case PathType.DOWN:
                _downHintsSprite.gameObject.SetActive(true);
                _upHintsSprite.gameObject.SetActive(false);
                _rightHintsSprite.gameObject.SetActive(false);
                _leftHintsSprite.gameObject.SetActive(false);
                break;

            // Camino hacia la derecha
            case PathType.RIGHT:
                _downHintsSprite.gameObject.SetActive(false);
                _upHintsSprite.gameObject.SetActive(false);
                _rightHintsSprite.gameObject.SetActive(true);
                _leftHintsSprite.gameObject.SetActive(false);
                break;

            // Camino hacia la izquierda
            case PathType.LEFT:
                _downHintsSprite.gameObject.SetActive(false);
                _upHintsSprite.gameObject.SetActive(false);
                _rightHintsSprite.gameObject.SetActive(false);
                _leftHintsSprite.gameObject.SetActive(true);
                break;

            // No tiene camino definido, no forma parte del camino
            case PathType.NOT_DEFINED:
                _downHintsSprite.gameObject.SetActive(false);
                _upHintsSprite.gameObject.SetActive(false);
                _rightHintsSprite.gameObject.SetActive(false);
                _leftHintsSprite.gameObject.SetActive(false);
                break;

            default:
                _downHintsSprite.gameObject.SetActive(false);
                _upHintsSprite.gameObject.SetActive(false);
                _rightHintsSprite.gameObject.SetActive(false);
                _leftHintsSprite.gameObject.SetActive(false);
                break;
        }
    }
}
