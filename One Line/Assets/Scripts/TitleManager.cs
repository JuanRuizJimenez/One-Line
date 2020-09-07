using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Clase que se encarga de la funcionalidad general de la escena de titulo
/// Asigna el valor de varios spriteSheetText en la escena
/// Crea los botones de dificultad menos el de challenge (siempre está)
/// Gesiona los tiempos de la recompensa diaria y el acceso a challenge
/// </summary>
public class TitleManager : MonoBehaviour
{
    [Tooltip("Boton que activa la recompensa diaria")]
    public GameObject _dailyRewardObject;
    [Tooltip("SpriteSheetText de las monedas")]
    public SpriteSheetText _coinText;
    [Tooltip("SpriteSheetText del coste de un reto")]
    public SpriteSheetText _challengeCostText;
    [Tooltip("SpriteSheetText de la recompensa diaria")]
    public SpriteSheetText _loginRewardText;
    [Tooltip("SpriteSheetText del numero de challenges completados")]
    public SpriteSheetText _challengerText;
    [Tooltip("SpriteSheetText del tiempo disponible en un challenge")]
    public SpriteSheetText _challengeTimeText;

    [Tooltip("Panel que bloquea challenge cuando está en cooldown")]
    public GameObject _disableChallengePanel;
    [Tooltip("Prefab usado para instanciar los botones de dificultad")]
    public GameObject _difficultyButton;
    [Tooltip("Boton de desafio")]
    public GameObject _challengeButton;
    [Tooltip("Objeto padre de los botones de dificultades")]
    public GameObject _titleFather;

    [Tooltip("Coste para realizar un challenge sin ver anuncios")]
    public int _challengeCost = 25;
    [Tooltip("Cantidad de monedas de la recompensa diaria")]
    public int _loginReward = 35;

    // Segundos restantes para activar el challenge
    private double _seconds;

    // Bool que indica si el desafio esta disponible
    private bool _challengeAvailable = true;

    // Bool que indica si se ha pausado o no el juego
    private bool _isPaused = false;

    // GameManager
    private GameManager _gameManager; 

    // Start is called before the first frame update
    void Start()
    {
        _gameManager = GameManager.Instance();

        // Actualizamos el texto de las monedas
        updateCoins(); 
        // Creamos los botones de dificultad
        difficultyButtonsSetUp(); 
        // Asignamos textos del HUD
        _challengeCostText.text = _challengeCost.ToString();
        _loginRewardText.text = "+" + _loginReward;
        _seconds = 0.0;

        if (_gameManager.getLastDailyReward() < (int)System.DateTime.Now.ToOADate())
        {
            _dailyRewardObject.SetActive(true);
        }

        // Calculamos el tiempo que ha pasado desde la ultima vez que se guardo 
        calculateTimeDiff();

        if (_gameManager.getChallengeTime() > 0)
            _challengeAvailable = false;
    }

    // Update is called once per frame
    private void Update()
    {
        // Si la opcion de challenge no esta activa
        if (!_challengeAvailable) {

            // Restamos el tiempo transcurrido al total del challenge
            double t = Time.deltaTime;
            _gameManager.addToChallengeTime(-t);

            _seconds = _gameManager.getChallengeTime();

            // Si no se ha acabado el tiempo
            if (_seconds > 0)
            {
                _disableChallengePanel.SetActive(true);

                // Escribimos el tiempo restante
                int mins = (int)_seconds / 60;
                int secs = (int)_seconds % 60;
                string s = mins.ToString().PadLeft(2, '0') + "p" + secs.ToString().PadLeft(2, '0');

                // Si ha llegado a pasar un segundo actualizamos el texto
                if (s != _challengeTimeText.text)
                {
                    _challengeTimeText.text = s;
                    _challengeTimeText.createSprites();

                    // Guardamos el tiempo actual para poder llevar el conteo de cuanto
                    // tiempo le queda al challenge aun estando fuera de esta
                    // pantalla o con el juego cerrado
                    _gameManager.saveDate();
                }
            }

            // Si se ha acabado el tiempo desbloqueamos el challenge
            else
            {
                _disableChallengePanel.SetActive(false);
                _challengeAvailable = true;
            }
        }

        updateCoins();
    }

    // Calcula el tiempo que ha pasado desde la ultima vez que se guardo el tiempo
    // hasta el momento actual y se lo resta al tiempo de challenge
    private void calculateTimeDiff()
    {
        // Fecha OLE actual
        double actTime = System.DateTime.Now.ToOADate();

        // Segundos
        actTime = actTime * 24 * 60 * 60;

        // Fecha OLE desde que se guardo el tiempo
        double oldTime = _gameManager.getDate();
        // Segundos
        oldTime = oldTime * 24 * 60 * 60;

        // Segundos que han pasado desde que se guardo el tiempo por ultima vez
        double dif = actTime - oldTime;

        _gameManager.addToChallengeTime(-dif);
    }

    /// <summary>
    /// Actualiza el texto de las monedas
    /// </summary>
    public void updateCoins()
    {
        _coinText.text = _gameManager.getCoins().ToString();
        _coinText.createSprites();
    }

    /// <summary>
    /// Crea los botones de las distintas dificultades y asigna sus textos
    /// </summary>
    public void difficultyButtonsSetUp()
    {
        GameObject aux;
        // Creamos los botones para las dificultades
        for(int i = 0; i < _gameManager.getNDifficulties(); i++)
        {
            aux = Instantiate(_difficultyButton, _titleFather.transform);
            aux.GetComponent<MainMenuButton>()._difficulty = i;
        }
        // Obtenemos el numero de desafios completados y creamos el texto
        _challengerText.text = _gameManager.getLevelIndex(_gameManager.getNDifficulties()).ToString().PadLeft(1, '0');
        _challengerText.createSprites();
        // Ponemos el boton de challenge al final
        _challengeButton.transform.SetAsLastSibling();
    }

    /// <summary>
    /// Lleva a un challenge
    /// </summary>
    /// <param name="pay">
    /// Indica si pagamos por ir
    /// </param>
    public void goToChallenge(bool pay)
    {
        if (!pay)
        {
            // Si no vamos pagando monedas sino viendo el video
            _gameManager.setDifficulty(_gameManager.getNDifficulties());
            SceneManager.LoadScene(2);
            _gameManager.setChallengeTime(1800);
        }

        else if (_gameManager.getCoins() >= _challengeCost)
        {
            // Si vamos pagando monedas restamos el coste al total
            _gameManager.addCoins(-_challengeCost);
            _gameManager.setDifficulty(_gameManager.getNDifficulties());
            SceneManager.LoadScene(2);
            _gameManager.setChallengeTime(1800);
        }
    }

    /// <summary>
    /// Suma la recompensa diaria y guarda la fecha
    /// </summary>
    public void gainLoginReward()
    {
        _gameManager.addCoins(_loginReward);

        _gameManager.setLastDailyReward((int)System.DateTime.Now.ToOADate());
    }

    /// <summary>
    /// Cierra la aplicacion, solo funcional desde el movil
    /// </summary>
    public void closeGame()
    {
        _gameManager.quitApp();
    }

    // Debemos comprobar si se ha pausado o no el juego (Android)
    // para controlar el tiempo de challenge cuando se vuelva a reanudar
    private void OnApplicationPause(bool pauseStatus)
    {
        // Si no estaba pausado antes y ahora pasa a estar pausado
        if (!_isPaused && pauseStatus)
        {
            _gameManager.saveDate();
        }

        // Si estaba pausado antes y ya deja de estarlo
        else if (_isPaused && !pauseStatus)
        {
            // Calculamos el tiempo que ha estado pausado el juego
            calculateTimeDiff();
        }

        _isPaused = pauseStatus;
    }
}
