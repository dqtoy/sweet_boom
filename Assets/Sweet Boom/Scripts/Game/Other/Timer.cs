using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Timer : MonoBehaviour
{
    public int Interval { get; private set; }
    public event Action<int> Tick;
    public event Action TimerEnd;
    public bool IsRunning { get; private set; }

    public Timer()
    {
        IsRunning = false;
    }

    public void StartTimer(int time)
    {
        Interval = time;
        IsRunning = true;
        CoroutineManager.CoroutineStart(Loop());
    }

    public void BreakTimer()
    {
        CoroutineManager.CoroutineStop(Loop());
        StopCoroutine(Loop());
        IsRunning = false;
    }

    private IEnumerator Loop()
    {
        ++Interval;
        while (--Interval >= 0)
        {
            Tick?.Invoke(Interval);
            yield return new WaitForSeconds(1);
        }
        IsRunning = false;
        TimerEnd?.Invoke();
    }
}
