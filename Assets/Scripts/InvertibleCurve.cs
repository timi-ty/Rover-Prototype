using UnityEngine;
using System.Collections;

[System.Serializable]
public class InvertibleCurve
{
    private static string NOT_INITIALIZED = " was not initialized. Must call Init() on this object first.";
    private bool isInitialized { get; set; }
    public AnimationCurve normalCurve;
    public AnimationCurve invertedCurve;

    public void Init()
    {
        invertedCurve = new AnimationCurve();

        float totalTime = normalCurve.keys[normalCurve.length - 1].time;
        float sampleX = 0;
        float deltaX = 0.01f;
        float lastY = normalCurve.Evaluate(sampleX);
        while (sampleX <= totalTime)
        {
            float y = normalCurve.Evaluate(sampleX);
            float deltaY = y - lastY;
            float tangent = deltaX / deltaY;
            Keyframe invertedKey = new Keyframe(y, sampleX, tangent, tangent);
            invertedCurve.AddKey(invertedKey);

            sampleX += deltaX;
            lastY = y;
        }

        for (int i = 0; i < invertedCurve.length; i++)
        {
            invertedCurve.SmoothTangents(i, 0.1f);
        }

        isInitialized = true;
    }

    /// <summary>
    /// Evaluate the Y value that corresponds to the given X value.
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public float Evaluate(float x)
    {
        if(isInitialized)
        {
            return normalCurve.Evaluate(x);
        }
        else
        {
            Debug.LogError(this.ToString() + NOT_INITIALIZED);
            return 0;
        }
    }

    /// <summary>
    /// Evaluate the X value that corresponds to the given Y value.
    /// </summary>
    /// <param name="velocity"></param>
    /// <returns></returns>
    public float InverseEvaluate(float y)
    {
        if (isInitialized)
        {
            return invertedCurve.Evaluate(y);
        }
        else
        {
            Debug.LogError(this.ToString() + NOT_INITIALIZED);
            return 0;
        }
    }

    /// <summary>
    /// Swaps the X and Y axes of the curve.
    /// </summary>
    public void Invert()
    {
        if (isInitialized)
        {
            AnimationCurve temp = normalCurve;
            normalCurve = invertedCurve;
            invertedCurve = temp;
        }
        else
        {
            Debug.LogError(this.ToString() + NOT_INITIALIZED);
        }
    }
}
