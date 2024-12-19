
using UnityEngine;

public class CameraShake : MonoBehaviour //sacado de: https://gist.github.com/ftvs/5822103
{

	
    // How long the object should shake for.
    public float shakeDuration = 0f;
	
    // Amplitude of the shake. A larger value shakes the camera harder.
    public float shakeAmount = 0.7f;
    public float decreaseFactor = 1.0f;
	
    Transform camTransform;
    Vector3 originalPos;
	
    void Awake()
    {
        camTransform = transform;
    }
	
    void OnEnable()
    {
        originalPos = camTransform.localPosition;
    }

    void Update()
    {
        if (shakeDuration > 0)
        {
            camTransform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
			
            shakeDuration -= Time.deltaTime * decreaseFactor;
        }
        else
        {
            shakeDuration = 0f;
            camTransform.localPosition = originalPos;
        }
    }

    public void Shake(float duration) => shakeDuration = duration;
}