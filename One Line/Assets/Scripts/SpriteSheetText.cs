using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Clase que implementa la creacion de textos en base a una spritesheet 
/// Los sprite de la spritesheet deben tener como nombre el caracter que representan
/// Esto conlleva el problema de que algunos caracteres no pueden usarse como nombre por si mismos,
/// como / y :, en estos casos usamos una letra como codificacion
/// El ordenamiento de las letras queda a manos de Unity, ya que los hijos tienen el componente
/// layoutElement y el padre gridLayoutGroup. 
/// El formato se configura desde el inspector con esos componentes
///</summary>

public class SpriteSheetText : MonoBehaviour
{
    [Tooltip("Texto a mostrar")]
    public string text; // Texto a mostrar
    [Tooltip("Texture2D que contiene el spriteSheet")]
    public Texture2D spriteSheet; // Texture2D que contiene el spriteSheet
    [Tooltip("GameObject que usamos como modelo para instanciar las letras")]
    public GameObject defaultLetter; // GameObject que usamos como modelo para instanciar las letras
    [Tooltip("Crear sprites en el start")]
    public bool createOnStart = true; // Bool que indica si las letras se crean automaticamente al activarse el componente
    private Sprite[] _sprites; // Array con los sprites sacados de la spriteSheet
    private string _fontsPath = "Fonts/"; // Ruta a la carpeta con las fuentes, desde resources
    private List<GameObject> _letters; // Lista de objetos que conforman el texto, cada uno una letra

    // Cargamos los sprites e instanciamos la lista de letras
    // para que estén disponibles en el start
    private void Awake()
    {
        // Cargamos los sprites e inicializamos letters 
        _sprites = Resources.LoadAll<Sprite>(_fontsPath + spriteSheet.name);
        _letters = new List<GameObject>();
    }

    // Creamos los sprites si createOnStart es true
    void Start()
    {
        if (createOnStart) createSprites();
    }

    /// <summary>
    /// Metodo que crea GameObjects que formen text, si ya hay, se limpian los antiguos
    /// </summary>
    public void createSprites()
    {
        // Si ya hay sprites, los limpiamos
        if (_letters.Count > 0) {
            clearLetters(); }

        // Creamos defaultLetters con el sprite asociado a cada caracter del texto
        for (int i = 0; i < text.Length; i++)
        {
            GameObject aux = Instantiate(defaultLetter, transform);
            aux.GetComponent<Image>().sprite = findSprite(text[i]);
            _letters.Add(aux);
        }
    }

    // Busca un sprite con nombre c en el spritesheet
    private Sprite findSprite(char c)
    {
        bool found = false;
        Sprite s = null;
        int i = 0;
        // Busca el sprite hasta encontrarlo en _sprites,
        // si no lo encuentra, devuelve null
        while (!found && i < _sprites.Length)
        {
            if (_sprites[i].name == c.ToString()) {
                s = _sprites[i];
                found = true;
            }
            i++;
        }
        return s;
    }

    // Limpia la lista de objetos letra
    private void clearLetters()
    {
        foreach(GameObject g in _letters)
        {
            Destroy(g);
        }
        _letters.Clear();
    }
}
