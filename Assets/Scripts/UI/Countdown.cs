using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Countdown : Bolt.EntityBehaviour<GameManager>
{
    public Animator CountdownAnimation;
    public TextMeshProUGUI CounterText;
    public int StartCounterInteger = 5;

    private void Awake()
    {
        if(CountdownAnimation == null)
        {
            CountdownAnimation = GetComponent<Animator>();
        }
        CountdownAnimation.enabled = false;
    }

    public void StartingGame()
    {
        CountdownAnimation.enabled = true;
        print(StartCounterInteger);
        //if(CounterText.text.Contains)
        CounterText.text = StartCounterInteger.ToString();
        if(StartCounterInteger > 0)
        {
            CountdownAnimation.Play("CountDownTimer", -1, 0);
            StartCounterInteger--;                                 // animation plays first when we enable it, then this function is called which is why we shift this variable by one.
        }

        else if (StartCounterInteger == 0)
        {
            CounterText.text = "GO!!";
            CountdownAnimation.Play("StartGame");
            StartCoroutine(ShakeText());
            if (BoltNetwork.IsServer)
            {
                Invoke("DisableCountdown", 5f);
                var evnt = StartGame.Create();
                evnt.Message = "Game Has Started!!!";
                evnt.Send();
                // teleport players here
            }
        }
    }
    
    private void DisableCountdown()
    {
        gameObject.SetActive(false);
    }
    public IEnumerator ShakeText()      // create noise on the text to create a shaking effect.
    {
        while (true)
        {
            CounterText.transform.localPosition = Vector3.zero;
            yield return new WaitForSeconds(0.01f);
            CounterText.transform.localPosition = CounterText.transform.localPosition + new Vector3(Random.Range(0,0.1f),Random.Range(0,0.1f),0);
        }

    }
}
