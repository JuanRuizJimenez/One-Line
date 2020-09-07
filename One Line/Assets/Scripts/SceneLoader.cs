using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Componente que usamos para llamar a cargas de escena de forma genérica desde algunos botones
/// De este modo se evita que esos botones requieran un componente para ello
/// </summary>

public class SceneLoader : MonoBehaviour
{
    /// <summary>
    /// Carga la escena con un index scene dado
    /// </summary>
    public void loadScene(int scene)
    {
        SceneManager.LoadScene(scene);
    }
}
