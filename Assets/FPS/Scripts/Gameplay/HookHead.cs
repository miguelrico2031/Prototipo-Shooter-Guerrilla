using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class HookHead : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private GameObject _renderer;

    private bool _lastEnabled;
    private float _angle;

    private void Awake()
    {
        _renderer.SetActive(false);
        transform.parent = null;
    }

    private void Update()
    {
        if (!_lineRenderer.enabled)
        {
            if (_lastEnabled) //se acaba de desactivar
            {
                _renderer.SetActive(false);
                _lastEnabled = false;
            }

            return;
        }

        transform.position = _lineRenderer.GetPosition(1);
        
        if (!_lastEnabled) //se acaba de activar
        {
            transform.rotation = Quaternion.LookRotation(_lineRenderer.GetPosition(1) - _lineRenderer.GetPosition(0));
            _renderer.SetActive(true);
            _angle = Random.Range(0f, 360f);
            _lastEnabled = true;
            transform.Rotate(transform.forward, _angle, Space.World);
        }

    }
}