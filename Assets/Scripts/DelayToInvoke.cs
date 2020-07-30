using System.Collections;
using System;
using UnityEngine;

public class DelayToInvoke : MonoBehaviour
{
    public static IEnumerator DelayToInvokeDo(Action action, float delaySeconds)
    {
        //Debug.Log("DelayToInvokeDo Before: " + delaySeconds);
        yield return new WaitForSeconds(delaySeconds);
        //Debug.Log("DelayToInvokeDo After: " + delaySeconds);
        action();
    }
}
