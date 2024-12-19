
using System;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.Events;

public class HookPickUp : MonoBehaviour
{
    public UnityEvent OnPick;
    [SerializeField] private bool _isKick;

    private Vector3 _startPos;

    private void Awake()
    {
        _startPos = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerCharacterController>(out var player))
        {
            player.IsHookEnabled = true;
            if (_isKick) player.IsHookJumpEnabled = true;
            OnPick.Invoke();
            Destroy(gameObject, .01f);
        }
    }

    private void Update()
    {
        transform.position = _startPos + Vector3.up * (Mathf.Sin(Time.time * 3f) * .3f);
    }
}
