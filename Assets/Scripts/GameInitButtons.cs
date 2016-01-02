using UnityEngine;
using System.Collections;

public class GameInitButtons : MonoBehaviour
{
    public void showScoresListCanvas()
    {
        GameInit gameInitScript = 
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<GameInit>();
        gameInitScript.scoresListCanvas.SetActive(true);
        gameInitScript.helpCanvas.SetActive(false);
    }

    public void showGameHelpCanvas()
    {
        GameInit gameInitScript =
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<GameInit>();
        gameInitScript.scoresListCanvas.SetActive(false);
        gameInitScript.helpCanvas.SetActive(true);
    }

    public void hideCanvases()
    {
        GameInit gameInitScript =
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<GameInit>();
        gameInitScript.scoresListCanvas.SetActive(false);
        gameInitScript.helpCanvas.SetActive(false);
    }

    public void exitGame()
    {
        Application.Quit();
    }
}
