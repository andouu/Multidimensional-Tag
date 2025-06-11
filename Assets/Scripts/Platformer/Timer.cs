using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public Image timerFill;
    public Gradient colorBanding;
    private float timerFillVelocity = 0f;
    
    public bool isPaused = true;
    public int durationSeconds = 60 * 5; // 5 minutes
    private int _secondsLeft = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        _secondsLeft = durationSeconds;
        StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        while (_secondsLeft > 0)
        {
            if (!isPaused)
            {
                yield return new WaitForSeconds(1f);
                _secondsLeft--;
            }
            else
            {
                yield return null;
            }
        }

        Util.ResetLevel();
    }

    public void Update()
    {
        timerText.text = TimeLeftString();
        timerFill.fillAmount = Mathf.SmoothDamp(timerFill.fillAmount, PercentCompleted(), ref timerFillVelocity, 0.1f);
        timerFill.color = colorBanding.Evaluate(PercentCompleted());
    }

    public void ToggleTimer()
    {
        isPaused = !isPaused;
    }

    public void ResetTimer()
    {
        _secondsLeft = durationSeconds;
    }

    public float PercentCompleted()
    {
        return (float) _secondsLeft / durationSeconds;
    }

    public string TimeLeftString()
    {
        if (isPaused)
        {
            return "Time Left In Stage: Paused";
        }
        return $"Time Left In Stage: {FormatTime(_secondsLeft)}";
    }

    public static string FormatTime(int seconds)
    {
        string minuteStr = (seconds / 60).ToString("0");
        string secondStr = (seconds % 60).ToString("00");
        return $"{minuteStr}:{secondStr}";
    }
}
