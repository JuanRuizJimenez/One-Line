using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase que implementa una animacion de "flotar"
/// Desplaza maxDesp transform del objeto de forma ciclica hacia arriba y abajo
/// </summary>
public class FloatAnim : MonoBehaviour
{
    [Tooltip("Tamaño de la oscilacion")]
    public float maxDesp = 15;
    [Tooltip("Velocidad del desplazamiento")]
    public float vel = 1;

    // Y minima y maxima
    private float minY;
    private float maxY;

    private void Start()
    {
        // Calculamos los limites de Y respecto a la posicion inicial
        minY = transform.position.y;
        maxY = minY + maxDesp;
    }

    private void Update()
    {
        // Desplazamos 
        transform.position += new Vector3(0, vel * Time.deltaTime, 0);
        // Invertimos vel si llegamos a un limite
        if (transform.position.y <= minY || transform.position.y >= maxY) vel = -vel; 
    }
}