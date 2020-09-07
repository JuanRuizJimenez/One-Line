using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_ANDROID
using UnityEngine.Advertisements;
#endif

/// <summary>
/// Clase encargada del gestionar el funcionamiento de los anuncios para la version de Android
/// Hay dos tipos de anuncio: video sin recompensa y con recompensa
/// </summary>
public class Advertisements : MonoBehaviour
{
#if UNITY_ANDROID
    // Id del juego, necesaria para inicializar y posteriormente lanzar los anuncios
    string gameId = "3420272";
    // Controla si los anuncios van a ser lanzados para una fase de testing del juego
    bool testMode = true;
#endif

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_ANDROID
        // Initializamos el servicio de anuncios
        Advertisement.Initialize(gameId, testMode);
#endif
    }

    public void showAd()
    {
#if UNITY_ANDROID
        // Si podemos lanzar el anuncio de caracter video sin recompensa
        // y no esta comprada la version sin anuncios
        if (Advertisement.IsReady("video") && !GameManager.Instance().getDisableAdsState())
        {
            // Mostramos el anuncio
            Advertisement.Show();
        }
#endif
    }

    public void showRewardedAd()
    {
#if UNITY_ANDROID
        // Si podemos lanzar el anuncio de caracter video con recompensa
        if (Advertisement.IsReady("rewardedVideo"))
        {
            // A partir de este callback controlaremos si el usuario se ha saltado
            // el anuncio, lo ha visto al completo o si ha habido algun error
            var options = new ShowOptions { resultCallback = HandleShowResult };
            // Mostramos el anuncio
            Advertisement.Show("rewardedVideo", options);
        }
#endif
    }

#if UNITY_ANDROID
    private void HandleShowResult(ShowResult result)
    {
        switch (result)
        {
            // Si el usuario ha visto el anuncio al completo
            case ShowResult.Finished:
                Debug.Log("The ad was successfully shown.");
                // Duplicamos el premio en monedas como recompensa
                GameManager.Instance().duplicateReward();
                break;

            // Si el usuario se ha saltado el video
            case ShowResult.Skipped:
                Debug.Log("The ad was skipped before reaching the end.");
                break;

            // Si ha habido algun error
            case ShowResult.Failed:
                Debug.LogError("The ad failed to be shown.");
                break;
        }
    }
#endif

    /// <summary>
    /// Activa la opcion de que aparezcan anuncios de caracter video sin recompensa
    /// </summary>
    public void enableAds()
    {
        GameManager.Instance().enableAds();
    }

    /// <summary>
    /// Desactiva la opcion de que aparezcan anuncios de caracter video sin recompensa
    /// </summary>
    public void disableAds()
    {
        GameManager.Instance().disableAds();
    }
}
