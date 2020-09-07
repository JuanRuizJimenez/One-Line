using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Advertisements;

/// <summary>
/// Definicion util para controlar cada tipo de camino de puntos a formar
/// </summary>
public enum PathType { UP, DOWN, LEFT, RIGHT, NOT_DEFINED };

/// <summary>
/// En esta clase se implementa la funcionalidad encargada de crear el tablero, 
/// que gestionara que tiles estan o no en el camino y como se forma este así como
/// la entrada por input y su influencia en el gameplay.
/// Además ajustará el tamaño de los tiles para que siempre ocupen el tamaño máximo 
/// de la zona de juego ya que esta puede ser cambiante
/// </summary>
public class BoardManager : MonoBehaviour
{
    // Prefabs
    [Tooltip("Prefab de los tiles del tablero")]
    public Tile _tile;
    [Tooltip("Prefab de la marca del dedo ")]
    public Touch _touch;

    // Textos
    [Tooltip("SpriteSheetText del numero de nivel")]
    public SpriteSheetText _levelText;
    [Tooltip("SpriteSheetText del numero de nivel")]
    public SpriteSheetText _clearLevelText;
    [Tooltip("SpriteSheetText de las monedas actuales")]
    public SpriteSheetText _coinText;

    [Tooltip("Imagen con el nombre de la dificultad")]
    public Image _difficultyImage;
    [Tooltip("Imagen con el nombre de la dificultad")]
    public Image _clearDifficultyImage;

    // Paneles de final de nivel
    [Tooltip("Panel que se muestra al acabar un nivel")]
    public GameObject _clearPanel;
    [Tooltip("Panel que se muestra al ganar un challenge")]
    public GameObject _challengeClearPanel;
    [Tooltip("Panel que se muestra al perder un challenge")]
    public GameObject _challengeFailedPanel;

    [Tooltip("SpriteSheetText de la recompensa de un challenge")]
    public SpriteSheetText _challengeRewardText;

    // El fondo del UI cambia segun si es challenge o no
    [Tooltip("Fondo del UI en un nivel normal")]
    public GameObject _regularBottomUI;
    [Tooltip("Fondo del UI en un challenge")]
    public GameObject _challengeBottomUI;

    [Tooltip("Controlador de anuncios, para lanzar uno entre el paso de niveles")]
    public Advertisements _ads;

    // Matriz de tiles que simula el tablero
    private Tile[,] _board;

    // Tamaño de la matriz de tiles
    private int _rows = 0;
    private int _cols = 0;

    // Color de la skin de los tiles
    private int _color = 1;

    [Tooltip("Numero de skins distintas para los tiles")]
    public int _colorRange = 6;

    // Tamaño de la zona de juego
    private int _gamezoneWidth = 0;
    private int _gamezoneHeight = 0;

    // Escalado de los tiles
    private float _tileScale;

    // Tamaños "logicos" de los paneles y zona central en pixeles
    private const int PANEL_WIDTH = 720;
    private const int UP_PANEL_HEIGHT = 264;
    private const int DOWN_PANEL_HEIGHT = 222;
    private const int GAME_PANEL_HEIGHT = 794;

    // Por otra parte este es el tamaño estandar del tile, cuando tiene escala 1
    private const int TILE_ORIG_SIZE = 100;

    // Tamaños de margenes por limpieza
    private const int VERTICAL_MARGIN = 80;
    private const int HORIZONTAL_MARGIN = 80;

    // Nivel Completado
    private bool _solved = false;

    // Objeto padre de los tiles
    private GameObject _boardAnchor;

    // Pila en la cual iremos guardando los tiles del camino
    // que se van marcando
    private Stack<Tile> _path;

    // Matriz que contiene la unica solucion del nivel (leida desde fichero de nivel)
    // La primera dimension indica el numero de la casilla desde el principio, en la otra
    // en la segunda guardamos la coordenada x en 0 y la y en 1
    private int[,] _solPath; 
    private int _solLength = 0; // Longitud de la solucion (numero de tiles en el tablero)

    // Pistas
    private int _hintIndex = 1; // Hasta que posicion hemos revelado
    [Tooltip("Cuantas casillas revelamos con cada pista")]
    public int _hintsPerPay = 5;
    [Tooltip("Coste de las pistas")]
    public int _hintsCost = 25;

    // Niveles que van con resolucion 6x5
    private int _smallLevelsDifficulty = 2;

    // Challenge
    [Tooltip("Monedas dadas por completar un challenge")]
    public int _challengeRewardCoins = 50;
    [Tooltip("Minima dificultad de un challenge")]
    public int _minChallengeDif = 2;
    [Tooltip("Maxima dificultad de un challenge")]
    public int _maxChallengeDif = 3; 
    [Tooltip("Tiempo disponible para completar un challenge")]
    public float _challengeTime = 30;
    private bool _challenge = false; // Indica si estamos en un challenge
    private bool _lostChallenge = false; // Indica si hemos ganado o perdido el challenge

    // Referencia al game manager
    private GameManager _gameManager;

    // Rutas a diferentes sprites que cargamos como recursos
    private string _pathSpritePath = "Sprites/Game/block_00";
    private string _touchSpritePath = "Sprites/Game/block_00_touch";
    private string _hintSpritePath = "Sprites/Game/block_00_hint";
    private string _textPath = "Sprites/Texts/game_0_en";
    // Ruta del archivo con los niveles
    private string _levelsPath = "Levels/";

    // Start is called before the first frame update
    private void Start()
    {
        _gameManager = GameManager.Instance();
        initialSetUp();
        loadLevel();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!_solved)
        {
            // Si hay pulsacion en pantalla
            if (Input.GetMouseButton(0))
            {

                // Leemos las coordenadas del raton en pixeles de pantalla
                Vector3 pos = Input.mousePosition;
                // Convertimos estas coordenadas a coordenadas de la escena
                pos = Camera.main.ScreenToWorldPoint(pos);

                pos = pos / _boardAnchor.transform.localScale.x;

                // Correjimos coordenada Y (en mapas es arriba, en unity es abajo)
                pos.y = _rows - 1 - pos.y;

                // Si redondeamos, la posicion en coordenadas de la escena pasara
                // a ser la posicion correspondiente a cada tile
                int x = Mathf.RoundToInt(pos.x - (_boardAnchor.transform.position.x / _boardAnchor.transform.localScale.x));
                int y = Mathf.RoundToInt(pos.y + (_boardAnchor.transform.position.y / _boardAnchor.transform.localScale.y));

                // Si no nos salimos del tablero
                if (x < _board.GetLength(0) && x >= 0 && y < _board.GetLength(1) && y >= 0)
                {
                    if (_board[x, y] != null) // Aun dentro del tablero hay casillas vacias
                    {

                        // Si ese tile que estamos pulsando no esta marcado como camino y es adyacente al
                        // ultimo elemento añadido al camino, marcamos
                        if (!_board[x, y].IsPressed())
                        {
                            // Peek() == Top()
                            Tile aux = _path.Peek();

                            // Si es adyacente
                            bool ady = false;

                            if (aux.getX() == x && aux.getY() + 1 == y)
                            {
                                ady = true;
                                _board[x, y].SetPath(PathType.UP);
                            }

                            else if (aux.getX() == x && aux.getY() - 1 == y)
                            {
                                ady = true;
                                _board[x, y].SetPath(PathType.DOWN);
                            }

                            else if (aux.getY() == y && aux.getX() + 1 == x)
                            {
                                ady = true;
                                _board[x, y].SetPath(PathType.LEFT);
                            }

                            else if (aux.getY() == y && aux.getX() - 1 == x)
                            {
                                ady = true;
                                _board[x, y].SetPath(PathType.RIGHT);
                            }

                            if (ady)
                            {
                                _board[x, y].SetPressed(true);
                                _path.Push(_board[x, y]);
                            }
                        }

                        // Si ese tile que estamos pulsando si esta marcado como camino y es adyacente
                        // al ultimo elemento añadido al camino, desmarcamos
                        else if (_board[x, y].IsPressed())
                        {
                            bool found = false;

                            // > 1 por seguridad, el tile inicial nunca se desmarca
                            while (_path.Count > 1 && !found)
                            {
                                // Peek() == Top()
                                Tile aux = _path.Peek();

                                // Si no llegamos al pulsado desapilamos
                                if (!(aux.getX() == x && aux.getY() == y))
                                {
                                    aux.SetPressed(false);
                                    _path.Pop();
                                }

                                // En caso de llegar al tile pulsado salimos del bulce
                                else
                                {
                                    found = true;
                                }
                            }
                        }
                    }
                }
            }

            // Si no hay pulsacion miramos si el nivel se ha completado
            else
            {
                // Recorremos el tablero hasta que encontremos alguno sin marcar
                // si al salir hemos recorrido el tablero entero es que el nivel esta
                // completado

                bool found = false;
                int i;

                i = 0;
                while (!found && (i < _solPath.GetLength(0)))
                {
                    if (!_board[_solPath[i, 0], _solPath[i, 1]].IsPressed())
                        found = true;
                    i++;
                }

                // Nivel completado
                if (!found)
                {
                    levelCompleted();
                }
            }

            // Pulsando la H obtenemos una pista sin pagar
            // (Para hacer rpruebas en el editor facilmente)
            if (Input.GetKeyDown(KeyCode.H))
            {
                giveHint();
            }

            if (_challenge) challengeTimeUpdate();
        }
        setBoardAnchorTransform();
        calcScale();

    }

    // Metodo que gestiona el final de nivel una vez se detecta su final
    private void levelCompleted()
    {
        // Desactivamos el objeto touch
        _touch.enabled = false;
        _touch._touchSprite.enabled = false;
        // Indicamos el final para no entrar al bucle de update
        _solved = true;

        // Obtenemos nivel y dificultad
        int level =_gameManager.getLevel();
        int dif = _gameManager.getDifficulty();

        // En nivel normal enseñamos anuncio y llamamos a normalLevelCompleted
        if (!_challenge)
        {
            normalLevelCompleted(level, dif);
            _ads.showAd();
        }
        // En challenge llamamos a challengeLevelCompleted
        else
        {
            challengeLevelCompleted();
        }
    }

    // Aqui gestionamos una victoria en uno de los niveles normales
    private void normalLevelCompleted(int level, int dif)
    {
        _clearPanel.SetActive(true);
        _clearLevelText.createSprites();
        if (level + 1 > _gameManager.getLevelIndex(dif) && level <= 100)
            _gameManager.upgradeLevelIndex(dif);
    }

    // Aqui gestionamos un fin de partida en uno de los niveles challenge
    private void challengeLevelCompleted()
    {
        // Si perdimos la partida activamos el panel correspondiente
        if (_lostChallenge)
        {
            _challengeFailedPanel.SetActive(true);
        }
        // Si ganamos activamos el panel de victoria, obtenemos monedas 
        // y aumentamos el contador de victorias
        else
        {
            _challengeClearPanel.SetActive(true);
            addChallengeCoins();
            _gameManager.upgradeLevelIndex(_gameManager.getNDifficulties());
        }

        // Guardamos el momento actual para que pueda controlar los 
        // 30 minutos de espera desde el titulo
        _gameManager.saveDate();
    }

    // Gestion del tiempo en los challenges
    private void challengeTimeUpdate()
    {
        // Solo esperamos a que el anuncio acabe en android
#if UNITY_ANDROID
        if (!Advertisement.isShowing)
        {
#endif
        // Disminuimos el tiempo segun deltatime
        _challengeTime -= Time.deltaTime;
            int mins = (int)_challengeTime / 60;
            int secs = (int)_challengeTime % 60;
            string s = mins.ToString().PadLeft(2, '0') + "p" + secs.ToString().PadLeft(2, '0');
            SpriteSheetText aux = _challengeBottomUI.GetComponentInChildren<SpriteSheetText>();
            // comprobamos si nos hemos quedado sin tiempo
            if (_challengeTime <= 0)
            {
                _lostChallenge = true;
                levelCompleted();
            }
            // Si el tiempo ha cambiado, actualizamos el texto
            else if (s != aux.text)
            {
                aux.text = s;
                aux.createSprites();
            }
#if UNITY_ANDROID
        }
#endif
    }

    /// <summary>
    /// Compra pistas si tienes suficiente dinero y no has comprado ya todas
    /// </summary>
    public void buyHint()
    {
        // Si tenemos suficiente dinero lo restamos, actualizamos el texto de monedas
        // y damos la pista
        if(_hintIndex < _solLength && _gameManager.getCoins() >= _hintsCost)
        {
            _gameManager.addCoins(-_hintsCost);
            _coinText.text = _gameManager.getCoins().ToString();
            _coinText.createSprites();
            giveHint();
        }
    }

    /// <summary>
    /// Da pistas sobre el recorrido
    /// </summary>
    public void giveHint()
    {
        // Limpiamos el camino recorrido
        clearPath();
        int i = 0;
        // Recorremos las casilla que debemos revelar a no ser que lleguemos al final
        while (i < _hintsPerPay && _hintIndex < _solLength)
        {
            PathType aux;
            if (_solPath[_hintIndex - 1, 0] == _solPath[_hintIndex, 0]) // Movimiento en eje Y (X no cambia)
            {
                if (_solPath[_hintIndex - 1, 1] > _solPath[_hintIndex, 1]) aux = PathType.UP;
                else aux = PathType.DOWN;
            }
            else // Movimiento en eje X (Y no cambia)
            {
                if (_solPath[_hintIndex - 1, 0] > _solPath[_hintIndex, 0]) aux = PathType.LEFT;
                else aux = PathType.RIGHT;
            }

            // Asignamos la casilla como pista con la direccion correspondiente
            _board[_solPath[_hintIndex - 1, 0], _solPath[_hintIndex - 1, 1]].SetHint(aux);
            _hintIndex++;
            i++;
        }

    }

    // Limpia el camino recorrido (sin tocar casilla inicial)
    private void clearPath()
    {
        while (_path.Count > 1)
        {
            Tile aux = _path.Pop();
            aux.SetPressed(false);
        }
    }

    // Asigna el indexLevel y los textos e imagenes relativos al nivel
    // gestiona los niveles aleatorios en challenges
    private void initialSetUp()
    {
        int level;
        int difficulty = _gameManager.getDifficulty();

        // En ambos casos asignamos la imagen que muestra la dificultad y el sheet del dinero
      
        _coinText.text = _gameManager.getCoins().ToString();
        _coinText.createSprites();

        // Si estamos en un challenge asignamos el texto de la recompensa
        // y elegimos un nivel aleatorio dentro del rango
        if (difficulty == _gameManager.getNDifficulties())
        {
            Sprite textSprite = Resources.Load<Sprite>(_textPath.Replace("0", "challenge"));
            _difficultyImage.sprite = textSprite;
            _clearDifficultyImage.sprite = textSprite;
            _challenge = true;
            _gameManager.setDifficulty(Random.Range(_minChallengeDif, _maxChallengeDif));
            _gameManager.setLevel(Random.Range(0, _gameManager.getLevelsPerDifficulty(_gameManager.getDifficulty())));
            _challengeRewardText.text = "+" + _challengeRewardCoins;
            _challengeBottomUI.SetActive(true);
        }
        // En niveles normales asignamos textos e imagenes relativoa al nivel
        else
        {
            Sprite textSprite = Resources.Load<Sprite>(_textPath.Replace("0", difficulty.ToString().ToLower()));
            _difficultyImage.sprite = textSprite;
            _clearDifficultyImage.sprite = textSprite;
            level = _gameManager.getLevel();
            _levelText.text = level.ToString();
            _clearLevelText.text = level.ToString();
            _levelText.createSprites();
            _regularBottomUI.SetActive(true);
        }
    }

    // Carga de nivel a partir del archivo de texto
    private void loadLevel()
    {
        _boardAnchor = new GameObject();

        // Obtenemos el archivo como un TextAsset, para recorrerlo linea a linaa hacemos split
        // con caracter '\n' y recorremos lo strings en el array resultante
        string[] levelFileLines = Resources.Load<TextAsset>(_levelsPath + _gameManager.getDifficulty()
            + "levels").text.Split('\n');

        bool levelFound = false;
        int lineIndex = 0;
        int level =_gameManager.getLevel();
        // Buscamos el index del nivel
        while (!levelFound && lineIndex < levelFileLines.Length) // no hemos encontrado y no es el final
        {
            string[] aux = levelFileLines[lineIndex].Split(':');
            if (aux[0] == "index" && aux[1].Replace(",", "").Replace(" ", "").Replace('\r'.ToString(), "") == level.ToString())
            {
                levelFound = true;
            }
            lineIndex++;
        }

        if (!levelFound)
            Debug.Log("NIVEL NO ENCONTRADO, NUMERO NO VALIDO: " + _gameManager.getLevel());

        // Una vez encontrado empezamos leyendo la linea con el layout
        else
        {
            // Nos quedamos con [[ "row1", "row2", ... ]],
            string layoutLine = levelFileLines[lineIndex].Split(':')[1]; 

            // Eliminamos caracteres que no queremos, acabamos con row1,row2,...
            layoutLine = layoutLine.Replace("],", "").Replace("[", "").Replace("]", "").Replace("\"", "").Replace(" ", "").Replace('\r'.ToString(), "");

            // Separamos por comas, quedandonos las filas codificadas
            string[] layout = layoutLine.Split(',');
            // Calculamos dimensiones del nivel
            _rows = layout.Length; 
            _cols = layout[0].Length;

            // Creamos el layout del nivel
            createLayoutFromString(layout); 

            // Despues toca leer el path de la solucion
            lineIndex++;

            // Queda como [ [ x1, y1],  [x2, y2], ...]
            string pathLine = levelFileLines[lineIndex].Split(':')[1]; 
                                                                      
            // Eliminamos caracteres que estorban, queda como x1,y1,x2,y2,...
            pathLine = pathLine.Replace("[", "").Replace("]", "").Replace("\"", "").Replace(" ", "").Replace('\r'.ToString(), "");

            // Separamos por comas, quedando cada coordenada como un string
            var path = pathLine.Split(','); 

            // Calculamos longitud solucion
            _solLength = path.Length / 2;

            // Creamos y asignamos
            _solPath = new int[_solLength, 2];

            for (int i = 0; i < _solLength; i++)
            {
                // Coordenada x
                _solPath[i, 0] = int.Parse(path[2 * i + 1]);
                // Coordenada y
                _solPath[i, 1] = int.Parse(path[2 * i]); 
            }
        }
    }

    /// <summary>
    /// Carga del siguiente nivel tras una victoria
    /// </summary>
    public void loadNextLevel()
    {
        int level = _gameManager.getLevel();
        int dif = _gameManager.getDifficulty();
        level += 1;

        if (level > _gameManager.getLevelsPerDifficulty(dif))
        {
            // Decision de diseño, si llegas al final de la dificultad MASTER
            // Es el ultimo nivel del juego asi que no avanzamos mas
            // Challenge son niveles a parte asi que no llegamos a esa dificultad
            if (dif + 1 == _gameManager.getNDifficulties())
            {
                level = 100;
            }
            else
            {
                level = 1;
                dif += 1;
                _gameManager.setDifficulty(dif);
            }
        }
        _gameManager.setLevel(level);

        // Limpiamos variables y hacemos configuraciones iniciales 
        // para dejar lista la llegada del siguiente nivel
        Destroy(_boardAnchor);
        initialSetUp();
        loadLevel();
        _touch.enabled = true;
        _clearPanel.SetActive(false);
        _challengeClearPanel.SetActive(false);
        _touch._touchSprite.enabled = true;
        _solved = false;
        _hintIndex = 1;
    }

    // Crea el tablero a partir de una fuente de texto dada
    private void createLayoutFromString(string[] source)
    {
        _cols = source[0].Length;
        _rows = source.Length;

        // Color aleatorio en el rango, para las distintas skins
        _color = Random.Range(1, _colorRange);

        // Reserva memoria para guardar las referencias a los objetos tile
        _board = new Tile[_cols, _rows];

        // Reservamos memoria para la pila
        _path = new Stack<Tile>();
        string s = _color.ToString().PadLeft(2, '0'); //añadimos 0 a la izquierda si tiene un digito

        // Obtenemos los sprites asociados al color actual
        Sprite pathSprite = Resources.Load<Sprite>(_pathSpritePath.Replace("00", s));
        Sprite hintSprite = Resources.Load<Sprite>(_hintSpritePath.Replace("00", s));

        _touch._touchSprite.sprite = Resources.Load<Sprite>(_touchSpritePath.Replace("00", s));

        for (int i = 0; i < _rows; i++) // Coordenada Y
        {
            for (int j = 0; j < _cols; j++) // Coordenada X
            {
                char c = source[i][j];
                if (c != '0')
                {
                    // Instanciamos el tile
                    Tile aux = GameObject.Instantiate(_tile);
                    aux._pathSprite.sprite = pathSprite;
                    aux._upHintsSprite.sprite = hintSprite;
                    aux._downHintsSprite.sprite = hintSprite;
                    aux._rightHintsSprite.sprite = hintSprite;
                    aux._leftHintsSprite.sprite = hintSprite;

                    // Asignamos padre Anchor
                    aux.transform.SetParent(_boardAnchor.transform);

                    // Su posicion en el mundo fisico y logico coinciden
                    // Correjimos coordenada Y (en mapas es arriba, en unity es abajo)
                    aux.gameObject.transform.position = new Vector3(j, _rows - 1 - i, 0);
                    aux.setPosition(j, i);

                    // Comprobacion destino
                    if (c == '2')
                    {
                        aux.SetPressed(true);
                        aux.SetInitial(true);
                        _path.Push(aux);
                    }

                    // Guardamos la referencia del objeto creado
                    _board[j, i] = aux;
                }
            }
        }
        setBoardAnchorTransform();
    }

    // Coloca el ancla de la que son hijos todos los tiles en la posicion adecuada de la zona de juego
    private void setBoardAnchorTransform()
    {
        // Posicion
        Vector3 anchorPos = new Vector3(Screen.width / 2f, (Screen.height / 2f) - ((UP_PANEL_HEIGHT - DOWN_PANEL_HEIGHT) / 2f), 10);
        
        // Pasamos a coordenadas del mundo
        anchorPos = Camera.main.ScreenToWorldPoint(anchorPos);

        // Movemos en funcion de las filas, las columnas y la escala
        anchorPos.x -= (((_cols - 1) * _tileScale) / 2f);
        anchorPos.y -= (((_rows - 1) * _tileScale) / 2f);
        anchorPos.z = 10;

        _boardAnchor.transform.position = anchorPos;
    }

    // Calcula la escala en funcion de la disposicion de la resolucion
    // de tal forma que despues se ajusten los tiles para abarcar el mayor
    // espacio posible de la zona de juego
    private void calcScale()
    {
        // Tamaño de la pantalla
        float screenHeightInUnits = Camera.main.orthographicSize * 2;
        float screenWidthInUnits = screenHeightInUnits * Screen.width / Screen.height;
        Vector3 screenSize = new Vector3(screenWidthInUnits * 100, screenHeightInUnits * 100, 0);

        // El ancho de los paneles siempre es el de la pantalla
        int panelWidth = (int)screenSize.x;

        // Regla de 3 para calcular el alto de los paneles
        int upPanelHeight = panelWidth * UP_PANEL_HEIGHT / PANEL_WIDTH;
        int downPanelHeight = panelWidth * DOWN_PANEL_HEIGHT / PANEL_WIDTH;

        // Tamaño de la zona de juego, el espacio libre
        _gamezoneHeight = (int)screenSize.y - (upPanelHeight + downPanelHeight);
        _gamezoneWidth = panelWidth;

        // Regla de 3 para calcular los margenes de limpieza
        int verticalMargin = _gamezoneHeight * VERTICAL_MARGIN / GAME_PANEL_HEIGHT;
        int horizontalMargin = _gamezoneWidth * HORIZONTAL_MARGIN / PANEL_WIDTH;

        // Restamos los margenes de limpieza
        _gamezoneHeight -= verticalMargin;
        _gamezoneWidth -= horizontalMargin;

        // Calculamos la escala especifica que debe tener el tile
        _tileScale = calcTilesScale(_gamezoneWidth, _gamezoneHeight);

        // Aplicamos esa escala al padre de los tiles y a la sombra de tocado
        _boardAnchor.transform.localScale = new Vector3(_tileScale, _tileScale, 0);
        _touch.transform.localScale = new Vector3(_tileScale, _tileScale, 0);

    }

    // Calcula la escala que deben tener los tiles para abarcar el maximo espacio
    // posible de una zona de juego dada
    private float calcTilesScale(int gzWidth, int gzHeight)
    {
        int difficulty = _gameManager.getDifficulty();

        // Dimensiones del tablero en tiles, o 6x5 o 6x8
        int tabW, tabH;

        // Dependiendo de la dificultad la disposicion sera de 5x6 o de 8x6 tiles
        tabW = 6;
        tabH = (difficulty < _smallLevelsDifficulty) ? 5 : 8;

        // Elegimos el minimo de las dos opciones
        float aux1 = (float)gzWidth / (tabW * TILE_ORIG_SIZE);
        float aux2 = (float)gzHeight / (tabH * TILE_ORIG_SIZE);
        float scale = Mathf.Min(aux1, aux2);

        return scale;
    }

    /// <summary>
    /// Suma las monedas por ganar un challenge
    /// </summary>
    public void addChallengeCoins()
    {
        _gameManager.addCoins(_challengeRewardCoins);
    }

    /// <summary>
    /// Sale del nivel a la pantalla de seleccion o al menu principal si es challenge
    /// </summary>
    public void exitLevel()
    {
        if (_challenge)
        {
            SceneManager.LoadScene(0);

            // Guardamos el momento actual para que pueda controlar los 
            // 30 minutos de espera desde el titulo
            _gameManager.saveDate();
        }

        else
            SceneManager.LoadScene(1);
    }

}
