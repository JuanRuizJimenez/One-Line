using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Advertisements;

/// <summary>
/// Clase que contiene la informacion que se debera guardar al finalizar la partida
/// </summary>
public class SaveData
{
    // Numero de monedas
    public int _SDcoins = 0;

    // Array contenedor de los indices de niveles superados de cada dificultad
    public int[] _SDnIndexPerDifficulty = null;

    // Numero de dificultades disponibles, debemos guardarlo puesto que son variables
    // en funcion de lo que decida el diseñador
    public int _SDnDifficulties = 5;

    // Gestiona si se ha comprado la opcion de desactivar anuncios o no
    public bool _SDnoAds = false;

    // Tiempo restante para desbloquear el desafio "challenge"
    public double _SDchallengeTime = 1800;

    // Ultima vez que se guardo el tiempo (se guarda como fecha OLE)
    // Debemos guardarlo para controlar el tiempo del challenge aun cerrando la app
    // o cambiando de escena
    public double _SDlastTime = 0;

    // Ultima vez que se recibio el premio diario (se guarda como fecha OLE)
    // Lo guardamos para controlar cuando se puede volver a recibir el premio
    public int _SDlastDailyReward = 0;

    // Cadena hash que usaremos para controlar si se ha editado el archivo de guardado de manera externa
    public string _hash;
}

/// <summary>
/// Clase encargada de gestionar los aspectos generales del juego
/// como pueden ser los datos de guardado, todo lo relevante y comun entre escenas
/// y las funcionalidades generales de la partida relativas a esos datos
/// 
/// Se plantea de manera singleton para que sea invocable desde cualquier script
/// </summary>
public class GameManager : MonoBehaviour
{
    // Patron singleton
    static GameManager _instance = null;

    // Nivel actual
    int _level = 1;

    // Dificultad actual
    int _difficulty = 0;

    // Numero de monedas
    int _coins = 0;

    // Ultimas monedas recibidas, lo usaremos para duplicar el premio cuando sea necesario
    int _lastCoinsEarned = 0;

    // Se ha a comprado o no la desactivacion de los anuncios
    bool _noAds = false;

    // Tiempo restante para activar el challenge
    double _challengeTime = 0;

    // Ultima vez que se guardo el tiempo (fecha OLE)
    double _lastTime = 0;

    // Ultimo dia en el que se recibio el premio diario
    int _lastDailyReward = 0;

    // Nombre del archivo de guardado
    string _SDfilename = "sav.json";

    // Patron singleton, devuelve la instancia de la clase
    public static GameManager Instance() { return _instance; }

    // Numero de dificultades disponibles
    [Range(5,10)]
    public int _nDifficulties = 5;

    // Tamaño nDif, numero de niveles disponibles en cada dificultad 
    public int[] _nLevelsPerDifficulty;

    // Indices de los niveles superados de cada dificultad
    // Tamaño nDif + 1 porque tambien lleva la cuenta de los niveles challenge superados
    public int[] _nIndexPerDifficulty;

    // Ruta donde se encuentran los archivos de nivel
    private string levelsPath = "Levels/";

    private void Awake()
    {
        // Si ya hay una instancia la destruimos
        if (_instance != null)
        {
            // Destruimos la anterior instancia
            Destroy(this.gameObject);
            // Cargamos los datos de guardado
            load();
        }
        // Si no hay una instancia, sera esta
        else
        {
            // Cargamos los datos de guardado
            load();
            // Cargamos los niveles disponibles de cada dificultad
            loadLevelsPerDifficulty();

            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    // Carga el numero de niveles que tiene cada dificultad
    private void loadLevelsPerDifficulty()
    {
        _nLevelsPerDifficulty = new int[_nDifficulties];
        for (int i = 0; i < _nDifficulties; i++)
        {
            string[] levelFileLines = Resources.Load<TextAsset>(levelsPath + i
                + "levels").text.Split('\n');
            bool found = false;
            int lineIndex = 0;

            // Buscamos el index del nivel
            while (!found && lineIndex < levelFileLines.Length) // No hemos encontrado y no es el final
            {
                var aux = levelFileLines[lineIndex].Split(':');
                if (aux[0] == "//levels")
                {
                    _nLevelsPerDifficulty[i] = int.Parse(aux[1]);
                    found = true;
                }
                lineIndex++;
            }
        }
    }
    
    /// <summary>
    /// Asigna el nivel actual
    /// </summary>
    /// <param name="level"> Nivel actual </param>
    public void setLevel(int level)
    {
        _level = level;
    }

    /// <summary>
    /// Devuelve el nivel actual
    /// </summary>
    /// <returns> Nivel actual </returns>
    public int getLevel()
    {
        return _level;
    }

    /// <summary>
    /// Devuelve el numero de dificultades disponibles
    /// </summary>
    /// <returns> Numero de dificultades disponibles </returns>
    public int getNDifficulties()
    {
        return _nDifficulties;
    }

    /// <summary>
    /// Asigna la dificultad actual
    /// </summary>
    /// <param name="difficulty"> Dificultad actual </param>
    public void setDifficulty(int difficulty)
    {
        _difficulty = difficulty;
    }

    /// <summary>
    /// Devuelve la dificultad actual
    /// </summary>
    /// <returns> Dificultad actual </returns>
    public int getDifficulty()
    {
        return _difficulty;
    }   

    /// <summary>
    /// Devuelve el tiempo restante para activar el challenge
    /// </summary>
    /// <returns> Tiempo del challenge </returns>
    public double getChallengeTime()
    {
        return _challengeTime;
    }

    /// <summary>
    /// Añade o resta una cantidad de tiempo dada al tiempo del challenge
    /// </summary>
    /// <param name="t"> Tiempo a añadir o restar al challenge </param>
    public void addToChallengeTime(double t)
    {
        _challengeTime += t;

        if (_challengeTime < 0)
            _challengeTime = 0;

        // Un dato relevante ha cambiado, guardamos
        // save();
        // No es necesario en este caso porque cada segundo que decremente
        // el tiempo de challenge hacemos un saveDate() que ya hace un save()
        // asi ahorramos llamadas
    }

    /// <summary>
    /// Asigna un tiempo dado al tiempo del challenge
    /// </summary>
    /// <param name="t"> Tiempo restante del challenge </param>
    public void setChallengeTime(double t)
    {
        _challengeTime = t;

        // Un dato relevante ha cambiado, guardamos
        // save();
        // No es necesario en este caso porque cada ssegundo que decremente
        // el tiempo de challenge hacemos un saveDate() que ya hace un save()
        // asi ahorramos llamadas
    }

    /// <summary>
    /// Devuelve el ultimo dia en formato OLE en que se recibio el premio diario
    /// </summary>
    /// <returns> Ultimo dia en que se recibio el premio </returns>
    public int getLastDailyReward()
    {
        return _lastDailyReward;
    }

    /// <summary>
    /// Asigna el ultimo dia en formato OLE en que se recibio el premio diario
    /// </summary>
    /// <param name="t"> Ultimo dia en que se recibio el premio </param>
    public void setLastDailyReward(int t)
    {
        _lastDailyReward = t;

        // Un dato relevante ha cambiado, guardamos
        save();
    }

    /// <summary>
    /// Guarda la fecha actual en formato OLE
    /// </summary>
    public void saveDate()
    {
        // Calculamos la fecha como OLE
        double OADate = System.DateTime.Now.ToOADate();

        _lastTime = OADate;

        // Un dato relevante ha cambiado, guardamos
        save();
    }

    /// <summary>
    /// Devuelve la ultima fecha guardada en formato OLE
    /// </summary>
    /// <returns> Ultima fecha guardada en formato OLE </returns>
    public double getDate()
    {
        return _lastTime;
    }

    /// <summary>
    /// Devuelve el numero de niveles de una dificultad dada
    /// </summary>
    /// <param name="d"> Dificultad a consultar </param>
    /// <returns> Numero de niveles </returns>
    public int getLevelsPerDifficulty(int d)
    {
        return _nLevelsPerDifficulty[d];
    }

    /// <summary>
    /// Devuelve el indice de niveles superados de una dificultad dada
    /// </summary>
    /// <param name="difficulty"> Dificultad a consultar </param>
    /// <returns> Indice de niveles superados </returns>
    public int getLevelIndex(int difficulty)
    {
        return _nIndexPerDifficulty[difficulty];
    }

    /// <summary>
    /// Aumenta el indice de niveles superados de una dificultad dada
    /// </summary>
    /// <param name="difficulty"> Dificultad sobre la que actuar </param>
    public void upgradeLevelIndex(int difficulty)
    {
        _nIndexPerDifficulty[difficulty]++;

        // Un dato relevante ha cambiado, guardamos
        save();
    }

    /// <summary>
    /// Devuelve el numero de monedas conseguido
    /// </summary>
    /// <returns> Numero de monedas0 </returns>
    public int getCoins()
    {
        return _coins;
    }

    /// <summary>
    /// Asigna un valor al numero de monedas
    /// </summary>
    /// <param name="coins"> Monedas a asignar </param>
    public void setCoins(int coins)
    {
        _coins = coins;

        // Un dato relevante ha cambiado, guardamos
        save();
    }

    /// <summary>
    /// Añade una cantidad de monedas dada al total
    /// </summary>
    /// <param name="coins"> Numero de monedas a añadir </param>
    public void addCoins(int coins)
    {
        _coins += coins;
        _lastCoinsEarned = coins;

        // Un dato relevante ha cambiado, guardamos
        save();
    }

    /// <summary>
    /// Duplica el premio recibido al sumar de nuevo el ultimo de monedas obtenidas
    /// </summary>
    public void duplicateReward()
    {
        addCoins(_lastCoinsEarned);
    }

    /// <summary>
    /// Activa la opcion de que aparezcan anuncios de caracter video sin recompensa
    /// </summary>
    public void enableAds()
    {
        _noAds = false;

        // Un dato relevante ha cambiado, guardamos
        save();
    }

    /// <summary>
    /// Desactiva la opcion de que aparezcan anuncios de caracter video sin recompensa
    /// </summary>
    public void disableAds()
    {
        _noAds = true;

        // Un dato relevante ha cambiado, guardamos
        save();
    }

    /// <summary>
    /// Consulta si se ha activado o no la opcion de mostrar anuncios de caracter
    /// video sin recompensa
    /// </summary>
    /// <returns> El estado del mostrado de anuncios </returns>
    public bool getDisableAdsState()
    {
        return _noAds;
    }

    // Genera una cadena de texto hash a partir de un SaveData dado
    private string generateHash(SaveData sd)
    {
        // Guardamos todo en un JSON como cadena de texto
        // Este string solo lo utilizaremos para generar el hash luego lo desecharemos
        string saveString = JsonUtility.ToJson(sd, true);

        // Setup de SHA
        SHA256Managed crypt = new SHA256Managed();
        string hash = System.String.Empty;

        // Computamos el hash
        byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(saveString), 0, Encoding.UTF8.GetByteCount(saveString));

        // Convertimos a hexadecimal
        foreach (byte bit in crypto)
        {
            hash += bit.ToString("x2");
        }

        return hash;
    }

    /// <summary>
    /// Guarda los datos relevantes del estado de la partida
    /// </summary>
    public void save()
    {
        // Creamos el save data
        SaveData sd = new SaveData();

        // Guardamos todos los datos importantes
        sd._SDcoins = _coins;
        sd._SDnIndexPerDifficulty = _nIndexPerDifficulty;
        sd._SDnDifficulties = _nDifficulties;
        sd._SDnoAds = _noAds;
        sd._SDlastTime = _lastTime;
        sd._SDchallengeTime = _challengeTime;
        sd._SDlastDailyReward = _lastDailyReward;

        string hash = generateHash(sd);

        // Guardamos el hash en nuestro SaveData
        sd._hash = hash;

        // Guardamos el archivo
        string savePath = Path.Combine(Application.persistentDataPath, _SDfilename);
        File.WriteAllText(savePath, JsonUtility.ToJson(sd, true));
    }

    /// <summary>
    /// Carga los datos relevantes del estado de la partida
    /// </summary>
    public void load()
    {
        string savePath = Path.Combine(Application.persistentDataPath, _SDfilename);

        // Leemos el archivo
        try
        {
            // Guardamos el json como cadena de texto
            string JSONstring = File.ReadAllText(savePath);

            // Creamos el objeto a partir del json
            SaveData sd = JsonUtility.FromJson<SaveData>(JSONstring);

            // Cargamos los datos
            _coins = sd._SDcoins;
            _nIndexPerDifficulty = sd._SDnIndexPerDifficulty;
            _noAds = sd._SDnoAds;
            _lastTime = sd._SDlastTime;
            _challengeTime = sd._SDchallengeTime;
            _lastDailyReward = sd._SDlastDailyReward;

            // El numero de dificultades se elige desde el editor
            // Esto solo nos valdra para saber si tenemos que redimensionar el array de indices
            // Tamaño del array de indices : nDifficulties + 1, donde la ultima posicion siempre es challenge
            int _oldNdifficulties = sd._SDnDifficulties;

            // Redimensionamos el array de indices manteniendo los datos anteriores que no se ven afectados
            // por el cambio de tamaño
            if (_oldNdifficulties != _nDifficulties)
            {
                int[] aux = _nIndexPerDifficulty;
                _nIndexPerDifficulty = new int[_nDifficulties + 1];

                for (int i = 0; i < _nDifficulties; i++)
                {
                    if (i < _oldNdifficulties)
                        _nIndexPerDifficulty[i] = aux[i];
                    else
                        _nIndexPerDifficulty[i] = 1;
                }

                _nIndexPerDifficulty[_nDifficulties] = aux[aux.Length - 1];
            }

            string oldHash = sd._hash;
            sd._hash = null;

            // Si convertimos lo que acabamos de leer en un hash y lo comparamos con el
            // que acabamos de guardar, deben ser iguales, si no es asi el usuario habra
            // modificado el archivo de guardado
            string actualHash = generateHash(sd);

            if (oldHash != actualHash)
            {
                Debug.Log("NO SE HA CARGADO EL ARCHIVO DE GUARDADO PORQUE HA SIDO MODIFICADO");

                // Reseteamos los datos, por tramposo
                resetData(true);
            }
        }

        // Si no se puede leer el archivo de guardado, ponemos los datos por defecto
        catch
        {
            Debug.Log("NO SE HA PODIDO LEER EL ARCHIVO DE GUARDADO");
            resetData(false);
        }
    }

    // Cuando se cierre la aplicacion guardamos
    // Solo funcional desde el editor
    private void OnApplicationQuit()
    {
        // Ya no es necesario puesto que guardamos a cada cambio relevante que ocurra
        // save();
    }

    /// <summary>
    /// Cierra la aplicacion, solo funcional desde el movil
    /// </summary>
    public void quitApp()
    {
        Application.Quit();
    }

    // Resetea los datos relevantes de guardado y cargado a unos valores por defecto
    private void resetData(bool isCheated)
    {
        _coins = 0;
        _nIndexPerDifficulty = new int[_nDifficulties + 1];
        for (int i = 0; i < _nIndexPerDifficulty.Length; i++)
        {
            if (i < _nIndexPerDifficulty.Length - 1)
                _nIndexPerDifficulty[i] = 1;
            else
                _nIndexPerDifficulty[i] = 0;
        }
        _noAds = false;
        _lastTime = 0;
        _challengeTime = 0;

        // Si se ha hecho trampas ponemos como dia en el que se recibio el premio al actual
        // para que el usuario pierda el premio diario por tramposo
        // No se hara en el caso de ser una partida recien creada
        _lastDailyReward = isCheated ? (int)System.DateTime.Now.ToOADate() : 0;
    }
}
