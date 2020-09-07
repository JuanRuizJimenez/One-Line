using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase encargada de gestionar el funcionamiento del renderizado
/// que muestra donde se ha pulsado en pantalla
/// </summary>
/// 
public class Touch : MonoBehaviour
{
    [Tooltip("Sprite que marcara donde se esta realizando la pulsacion")]
    public SpriteRenderer _touchSprite;

    // Update is called once per frame
    void Update()
    {
        // Si hay pulsacion en pantalla
        if (Input.GetMouseButton(0))
        {
            _touchSprite.enabled = true;
            // Leemos las coordenadas del raton en pixeles de pantalla
            Vector3 pos = Input.mousePosition;
            // Convertimos estas coordenadas a coordenadas de la escena
            pos = Camera.main.ScreenToWorldPoint(pos);
            pos.z = 0;
            // Actualizamos posicion
            gameObject.transform.position = pos;
        }

        else
        {
            // Si no hay pulsacion desactivamos el sprite
            _touchSprite.enabled = false;
        }
    }
}
