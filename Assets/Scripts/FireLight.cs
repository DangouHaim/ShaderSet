using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireLight : MonoBehaviour
{
    public float MinRange = 2;
    public float MaxRange = 4;
    public float Delay = 0.01f;
    public float AngleStep = 0.1f;
    public float IntensityStep = 0.01f;

    private Light _light;

    // Start is called before the first frame update
    void Start()
    {
        _light = GetComponent<Light>();
        StartCoroutine(Lighting());
    }

    private IEnumerator Lighting()
    {
        while (true)
        {
            float value = Random.Range(MinRange, MaxRange);

            if (_light.spotAngle < value)
            {
                while (_light.spotAngle < value)
                {
                    _light.spotAngle += AngleStep;
                    _light.intensity += IntensityStep;
                    yield return new WaitForSeconds(Delay);
                }
            }
            else
            {
                while (_light.spotAngle > value)
                {
                    _light.spotAngle -= AngleStep;
                    _light.intensity -= IntensityStep;
                    yield return new WaitForSeconds(Delay);
                }
            }
        }
    }
}
