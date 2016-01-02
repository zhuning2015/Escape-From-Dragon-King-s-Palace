using UnityEngine;
using System.Collections;

public class GameSceneButtons : MonoBehaviour
{
    public void showSettingCanvas()
    {
        GameScene gameSceneScript = 
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<GameScene>();
        gameSceneScript.settingCanvas.SetActive(true);
        if (gameSceneScript.showElimatedCards)
        {
            gameSceneScript.showEliminatedCardsButton.SetActive(true);
            gameSceneScript.noShowEliminatedCardsButton.SetActive(false);
        }else
        {
            gameSceneScript.showEliminatedCardsButton.SetActive(false);
            gameSceneScript.noShowEliminatedCardsButton.SetActive(true);
        }
        gameSceneScript.gameOverCanvas.SetActive(false);
        gameSceneScript.winOneRoundCanvas.SetActive(false);
        gameSceneScript.loseOneRoundCanvas.SetActive(false);
    }

    public void hideCanvases()
    {
        GameScene gameSceneScript =
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<GameScene>();
        gameSceneScript.settingCanvas.SetActive(false);
        gameSceneScript.gameOverCanvas.SetActive(false);
        gameSceneScript.winOneRoundCanvas.SetActive(false);
        gameSceneScript.loseOneRoundCanvas.SetActive(false);
    }

    public void showEliminatedCards()
    {
        GameScene gameSceneScript =
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<GameScene>();
        gameSceneScript.showElimatedCards = true;
        gameSceneScript.showEliminatedCardsButton.SetActive(true);
        gameSceneScript.noShowEliminatedCardsButton.SetActive(false);
        gameSceneScript.updateElimatedCardBacks();
    }

    public void noShowEliminatedCards()
    {
        GameScene gameSceneScript =
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<GameScene>();
        gameSceneScript.showElimatedCards = false;
        gameSceneScript.showEliminatedCardsButton.SetActive(false);
        gameSceneScript.noShowEliminatedCardsButton.SetActive(true);
        gameSceneScript.updateElimatedCardBacks();
    }

    public void continueGame()
    {
        hideCanvases();
        GameScene gameSceneScript =
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<GameScene>();
        gameSceneScript.gameSceneState = GameScene.GameState.Initializing;
    }
}
