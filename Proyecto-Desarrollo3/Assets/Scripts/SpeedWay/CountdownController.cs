using System.Collections;
using UnityEngine;
using TMPro;
using System;

[RequireComponent(typeof(AkGameObj))] //requiere para que funcione el llamado de la m�sica de gameplay


public class CountdownController : MonoBehaviour
{
    [SerializeField] private float countdownTime;
    [SerializeField] private TextMeshProUGUI countdownDisplay;

    [SerializeField] private CarController carController;
    [SerializeField] private ObstacleSpawner obstacleSpawner;
    [SerializeField] private RaceTime raceTime;

    
    public event Action OnFinishCountdown;

    private void Start()
    {
        carController.CanMove = false;
        StartCoroutine(CountdownToStart());
    }

    IEnumerator CountdownToStart()
    {
        while (countdownTime > 0)
        {
            countdownDisplay.text = countdownTime.ToString();

            yield return new WaitForSeconds(1f);

            countdownTime--;
        }

        countdownDisplay.text = "GO!";

        AkSoundEngine.PostEvent("Play_gameplay", gameObject); //Empieza la m�sica.
        
        OnFinishCountdown?.Invoke();
        carController.CanMove = true;
        obstacleSpawner.EnableCanSpawnObstacle();
        raceTime.EnableUpdatingTime();

        yield return new WaitForSeconds(1f);

        countdownDisplay.gameObject.SetActive(false);
    }
}
