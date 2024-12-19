using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Level2Tutorial : MonoBehaviour
{
    private TextMeshProUGUI _text;

    private void Start()
    {
        _text = GetComponentInChildren<TextMeshProUGUI>();
        
        PlayerPrefs.SetString("lastScene", "Level 2");

    }

    public void HookTutorial() => StartCoroutine(HookTutorialCor());
    public void JumpKickTutorial() => StartCoroutine(JumpKickTutorialCor());

    private IEnumerator HookTutorialCor()
    {
        yield return new WaitForSeconds(.5f);

        _text.text =
            "¡Acabas de obtener el GANCHO! Apunta a un muro o enemigo y pulsa R para engancharte, y saldrás disparadx hacia este. " +
            "Mientras estés enganchadx a un enemigo, eres invulnerable.\n" +
            "Pulsa T para desengancharte en culquier momento.\n\nPulsa ENTER para continuar.";

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
        yield return null;
        _text.text = "";
    }

    private IEnumerator JumpKickTutorialCor()
    {
        yield return new WaitForSeconds(.5f);

        _text.text =
            "¡Has desbloqueado el PATADÓN! Ahora, cuando estés enganchadx a un enemigo y te acerques a él con el gancho, la cuerda se pondrá roja justo antes de alcanzarlo. " +
            "En ese momento, pulsa ESPACIO para darle un patadón que le causará daño masivo y te impulsará hacia arriba.\n\nPulsa ENTER para continuar.";

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
        yield return null;
        _text.text = "";
    }
}