using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Level1Tutorial : MonoBehaviour
{
    private TextMeshProUGUI _text;

    private IEnumerator Start()
    {
        _text = GetComponentInChildren<TextMeshProUGUI>();
        
        PlayerPrefs.SetString("lastScene", "Level 1");

        yield return new WaitForSeconds(2f);

        _text.text =
            "Bienvenidx. Este es un tutorial de última hora muy simple. Es un poco chapucero, pero te enseñará lo básico para jugar a este pequeño prototipo." +
            "\n\nPulsa ENTER para continuar.";
       
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
        yield return null;

        _text.text = "Presiona TAB o P para abrir el menú de pausa. Allí encontrarás los controles básicos. (Ignora lo del jetpack, no lo usaremos).\n" +
                     "Con esto deberías saber cómo moverte, saltar, disparar y apuntar. Mata a los robots para completar el nivel. ¡Buena suerte!" +
                     "\n\nPulsa ENTER para continuar.";

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
        yield return null;
        
        _text.text =
            "¡ESPERA! Olvidé decirte: Si disparas a una superficie estando cerca, obtendrás un impulso hacia el lado opuesto. " +
            "Puedes usar eso, por ejemplo, para saltar más alto disparando al suelo...\n\nPulsa ENTER para continuar.";

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
        yield return null;
        
        _text.text = "";
    }

}
