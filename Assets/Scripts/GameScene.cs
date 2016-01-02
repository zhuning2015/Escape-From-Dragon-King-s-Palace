using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameScene : MonoBehaviour {

    // Gameobject showing the back of a card
    public GameObject cardBack;
    // Gameobject showing the 54 cards
    public GameObject[] cards = new GameObject[54];

    // Canvas shown when the game is over (win or lose)
    public GameObject gameOverCanvas;
    // Text shown in the gameOverCanvas when the player loses
    public Text highestScoreText;
    // Text shown in the gameOverCanvas when the player wins
    public Text currentScoreText;
    // Text shown the score the player gets
    public Text scoreText;
    // Music played during the game is playing 
    public GameObject settingCanvas;
    public GameObject winOneRoundCanvas;
    public GameObject loseOneRoundCanvas;
    public GameObject showEliminatedCardsButton;
    public GameObject noShowEliminatedCardsButton;

    public AudioSource backgroundMusic;
    // Music played when the player loses
    public AudioClip winOneRoundMusic;
    // Music played when the player wins
    public AudioClip gameOverMusic;
    // Music when the cards are turned
    public AudioClip cardTurnMusic;
    public AudioClip loseOneRoundMusic;

    public int lifeCount = 3;
    public Text lifeText;

    public bool showElimatedCards;

    public enum GameState {Initializing, Playing, LosingOneRound, LoseOneRound, WinningOneRound,
        WinOneRound, GameOver};
    public GameState gameSceneState = GameState.Initializing;

    // The map for locating the cards
    private Vector3[] maps_pos       = new Vector3[54];
    private Vector3[] maps_rotate    = new Vector3[54];
    private int[][]   maps_hierarchy = new int[54][];

    private GameObject[] gameObjectsOnCardMap    = new GameObject[54];
    private GameObject[] gameObjectsOffCardMap   = new GameObject[54];
    private GameObject[] cardBacksOffCardMap = new GameObject[54];
    private List<GameObject> cardsTurnedOnCardMap = new List<GameObject>();
    private GameObject selctedCard = null;
    private int eliminatedCardCount = 0;
    private int score = 0;
    private int preWinLifeScore = 0;
    public int LIFEADDTHRESHOLD = 100;

	// Use this for initialization
	void Start ()
    {
        settingCanvas.SetActive(false);
        winOneRoundCanvas.SetActive(false);
        loseOneRoundCanvas.SetActive(false);
        showEliminatedCardsButton.SetActive(false);
        noShowEliminatedCardsButton.SetActive(false);
	}

    private void Initialize()
    {
        Init();
        gameSceneState = GameState.Playing;
    }

    private void Play()
    {
        if (backgroundMusic.volume < 1.0f)
            backgroundMusic.volume += 0.1f;
        gameOverCanvas.SetActive(false);
        if (Input.GetButtonDown("Fire1"))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                GameObject hitGameObject = hit.collider.gameObject;
                if (hitGameObject.tag == "CardBack")
                {
                    int indexOfHitCard = int.Parse(hitGameObject.name);
                    if (CanTurnBack(indexOfHitCard))
                    {
                        int[] turnedCardsIndexes = new int[] { indexOfHitCard };
                        TurnCommand turnCommand =
                            new TurnCommand(this, turnedCardsIndexes);
                        UIButtonUndo.invoker.AddCommand(turnCommand);
                        TurnCards(turnedCardsIndexes);
                    }
                }else
                {
                    if (selctedCard != null && selctedCard != hitGameObject)
                    {
                        if (selctedCard.CompareTag(hitGameObject.tag))
                        {
                            int firstCardIndex = int.Parse(selctedCard.name);
                            int secondCardIndex = int.Parse(hitGameObject.name);
                            EliminateCommand eliminateCmd =
                                new EliminateCommand(this, firstCardIndex, secondCardIndex);
                            UIButtonUndo.invoker.AddCommand(eliminateCmd);
                            EliminateCards(firstCardIndex, secondCardIndex);
                        }
                        else
                        {
                            selctedCard = hitGameObject;
                        }
                    }
                    else
                    {
                        selctedCard = hitGameObject;
                    }

                }
            }
        }

        if (DidOneRoundLose())
        {
            lifeCount--;
            lifeText.text = "" + lifeCount;
            gameSceneState = GameState.LosingOneRound;
        }

        if (DidOneRoundWin())
            gameSceneState = GameState.WinningOneRound;
    }

    private void LoseOneRound()
    {
        if (backgroundMusic.volume > 0.0f)
            backgroundMusic.volume -= 0.1f;
        if (backgroundMusic.volume <= 0.0f)
        {
            if (lifeCount <= 0)
            { 
                AudioSource.PlayClipAtPoint(gameOverMusic,
                    gameObject.transform.position);
                gameSceneState = GameState.GameOver;
            }
            else
            {
                AudioSource.PlayClipAtPoint(loseOneRoundMusic,
                    gameObject.transform.position);
                gameSceneState = GameState.LoseOneRound;                
            }
        }
    }

    // Update is called once per frame
    void Update () {
        switch (gameSceneState)
        {
            case GameState.Initializing:
                Initialize();
                break;
            case GameState.Playing:
                Play();
                break;
            case GameState.LosingOneRound:
                LoseOneRound();
                break;
            case GameState.LoseOneRound:
                loseOneRoundCanvas.SetActive(true);
                break;
            case GameState.WinningOneRound:
                if (backgroundMusic.volume > 0.0f)
                    backgroundMusic.volume -= 0.1f;
                if (backgroundMusic.volume <= 0.0f)
                {
                    AudioSource.PlayClipAtPoint(winOneRoundMusic,
                        gameObject.transform.position);
                    gameSceneState = GameState.WinOneRound;
                }
                break;

            case GameState.WinOneRound:
                winOneRoundCanvas.SetActive(true);
                break;

            case GameState.GameOver:
                gameOverCanvas.SetActive(true);
                currentScoreText.text = "当前得分: " + score;
                highestScoreText.text = "历史最高: " + "功能正在开发, \n                      敬请期待!";
                break;

        }
	}

    private void Init()
    {
        for (int i = 0; i < 54; i++)
        {
			if (gameObjectsOnCardMap[i]  != null)
			{
				Destroy(gameObjectsOnCardMap[i]);
				gameObjectsOnCardMap[i] = null;
			}
			if (gameObjectsOffCardMap[i] != null)
			{
				Destroy(gameObjectsOffCardMap[i]);
				gameObjectsOffCardMap[i] = null;
			}
            if (cardBacksOffCardMap[i] != null)
            {
                Destroy(cardBacksOffCardMap[i]);
                cardBacksOffCardMap[i] = null;
            }
        }
		cardsTurnedOnCardMap.Clear();
		selctedCard = null;
		eliminatedCardCount = 0;
        //UIButtonUndo.invoker.Clear();

        BuildCardsMap();

        ShuffleCards();

        InitGameObjectsOnCardsMap();

        InitUI();
    }

    private void BuildCardsMap()
    {
        // Setting the position of each card
        maps_pos[0]  = new Vector3(-1.65f, 3.40f, 0.0f);
        maps_pos[1]  = new Vector3(-0.55f, 3.40f, 0.0f);
        maps_pos[2]  = new Vector3( 0.55f, 3.40f, 0.0f);
        maps_pos[3]  = new Vector3( 1.65f, 3.40f, 0.0f);
        maps_pos[4]  = new Vector3(-1.65f, 1.80f, 0.0f);
        maps_pos[5]  = new Vector3(-0.55f, 1.80f, 0.0f);
        maps_pos[6]  = new Vector3( 0.55f, 1.80f, 0.0f);
        maps_pos[7]  = new Vector3( 1.65f, 1.80f, 0.0f);
        maps_pos[8]  = new Vector3(-1.65f, 0.20f, 0.0f);
        maps_pos[9]  = new Vector3(-0.55f, 0.20f, 0.0f);
        maps_pos[10] = new Vector3( 0.55f, 0.20f, 0.0f);
        maps_pos[11] = new Vector3( 1.65f, 0.20f, 0.0f);
        maps_pos[12] = new Vector3(-1.65f,-1.40f, 0.0f);
        maps_pos[13] = new Vector3(-0.55f,-1.40f, 0.0f);
        maps_pos[14] = new Vector3( 0.55f,-1.40f, 0.0f);
        maps_pos[15] = new Vector3( 1.65f,-1.40f, 0.0f);
        maps_pos[16] = new Vector3(-1.10f, 2.60f,-0.1f);
        maps_pos[17] = new Vector3( 0.00f, 2.60f,-0.1f);
        maps_pos[18] = new Vector3( 1.10f, 2.60f,-0.1f);
        maps_pos[19] = new Vector3(-1.10f, 1.00f,-0.1f);
        maps_pos[20] = new Vector3( 0.00f, 1.00f,-0.1f);
        maps_pos[21] = new Vector3( 1.10f, 1.00f,-0.1f);
        maps_pos[22] = new Vector3(-1.10f,-0.60f,-0.1f);
        maps_pos[23] = new Vector3( 0.00f,-0.60f,-0.1f);
        maps_pos[24] = new Vector3( 1.10f,-0.60f,-0.1f);
        maps_pos[25] = new Vector3(-0.55f, 1.80f,-0.2f);
        maps_pos[26] = new Vector3( 0.55f, 1.80f,-0.2f);
        maps_pos[27] = new Vector3(-0.55f, 0.20f,-0.2f);
        maps_pos[28] = new Vector3( 0.55f, 0.20f,-0.2f);
        maps_pos[29] = new Vector3( 0.00f, 1.00f,-0.3f);
        maps_pos[30] = new Vector3( 0.00f, 4.15f,-0.2f);
        maps_pos[31] = new Vector3( 2.15f, 4.15f,-0.2f);
        maps_pos[32] = new Vector3( 2.44f, 1.00f,-0.2f);
        maps_pos[33] = new Vector3( 2.15f,-2.15f,-0.2f);
        maps_pos[34] = new Vector3( 0.00f,-2.15f,-0.2f);
        maps_pos[35] = new Vector3(-2.15f,-2.15f,-0.2f);
        maps_pos[36] = new Vector3(-2.44f, 1.00f,-0.2f);
        maps_pos[37] = new Vector3(-2.15f, 4.15f,-0.2f);
        maps_pos[38] = new Vector3( 0.00f, 4.90f,-0.3f);
        maps_pos[39] = new Vector3( 2.60f, 4.60f,-0.3f);
        maps_pos[40] = new Vector3( 3.19f, 1.00f,-0.3f);
        maps_pos[41] = new Vector3( 2.60f,-2.60f,-0.3f);
        maps_pos[42] = new Vector3( 0.00f,-2.90f,-0.3f);
        maps_pos[43] = new Vector3(-2.60f,-2.60f,-0.3f);
        maps_pos[44] = new Vector3(-3.19f, 1.00f,-0.3f);
        maps_pos[45] = new Vector3(-2.60f, 4.60f,-0.3f);
        maps_pos[46] = new Vector3( 0.00f, 5.65f,-0.4f);
        maps_pos[47] = new Vector3( 3.15f, 5.15f,-0.4f);
        maps_pos[48] = new Vector3( 3.89f, 1.00f,-0.4f);
        maps_pos[49] = new Vector3( 3.15f,-3.15f,-0.4f);
        maps_pos[50] = new Vector3( 0.00f,-3.65f,-0.4f);
        maps_pos[51] = new Vector3(-3.15f,-3.15f,-0.4f);
        maps_pos[52] = new Vector3(-3.89f, 1.00f,-0.4f);
        maps_pos[53] = new Vector3(-3.15f, 5.15f,-0.4f);

        // Setting the rotation of each card
        for (int i = 0; i < 54; i++)
        {
            maps_rotate[i] = new Vector3(0.0f, 0.0f, 0.0f);
        }
        maps_rotate[31] = new Vector3(0.0f, 0.0f, -45.0f);
        maps_rotate[39] = new Vector3(0.0f, 0.0f, -45.0f);
        maps_rotate[47] = new Vector3(0.0f, 0.0f, -45.0f);

        maps_rotate[32] = new Vector3(0.0f, 0.0f, 90.0f);
        maps_rotate[40] = new Vector3(0.0f, 0.0f, 90.0f);
        maps_rotate[48] = new Vector3(0.0f, 0.0f, 90.0f);

        maps_rotate[33] = new Vector3(0.0f, 0.0f, 45.0f);
        maps_rotate[41] = new Vector3(0.0f, 0.0f, 45.0f);
        maps_rotate[49] = new Vector3(0.0f, 0.0f, 45.0f);

        maps_rotate[35] = new Vector3(0.0f, 0.0f, -45.0f);
        maps_rotate[43] = new Vector3(0.0f, 0.0f, -45.0f);
        maps_rotate[51] = new Vector3(0.0f, 0.0f, -45.0f);

        maps_rotate[36] = new Vector3(0.0f, 0.0f, 90.0f);
        maps_rotate[44] = new Vector3(0.0f, 0.0f, 90.0f);
        maps_rotate[52] = new Vector3(0.0f, 0.0f, 90.0f);

        maps_rotate[37] = new Vector3(0.0f, 0.0f, 45.0f);
        maps_rotate[45] = new Vector3(0.0f, 0.0f, 45.0f);
        maps_rotate[53] = new Vector3(0.0f, 0.0f, 45.0f);

        // Setting the cards pressing each card 
        maps_hierarchy[0]  = new int[]{16, 37};
        maps_hierarchy[1]  = new int[]{16, 17, 30};
        maps_hierarchy[2]  = new int[]{17, 18, 30};
        maps_hierarchy[3]  = new int[]{18, 31};
        maps_hierarchy[4]  = new int[]{16, 19, 36};
        maps_hierarchy[5]  = new int[]{16, 17, 19, 20};
        maps_hierarchy[6]  = new int[]{17, 18, 20, 21};
        maps_hierarchy[7]  = new int[]{18, 21, 32};
        maps_hierarchy[8]  = new int[]{19, 22, 36};
        maps_hierarchy[9]  = new int[]{19, 20, 22, 23};
        maps_hierarchy[10] = new int[]{20, 21, 23, 24};
        maps_hierarchy[11] = new int[]{21, 24, 32};
        maps_hierarchy[12] = new int[]{22, 35};
        maps_hierarchy[13] = new int[]{22, 23, 34};
        maps_hierarchy[14] = new int[]{23, 24, 34};
        maps_hierarchy[15] = new int[]{24, 33};
        maps_hierarchy[16] = new int[]{25};
        maps_hierarchy[17] = new int[]{25, 26};
        maps_hierarchy[18] = new int[]{26};
        maps_hierarchy[19] = new int[]{25, 27};
        maps_hierarchy[20] = new int[]{25, 26, 27, 28};
        maps_hierarchy[21] = new int[]{26, 28};
        maps_hierarchy[22] = new int[]{27};
        maps_hierarchy[23] = new int[]{27, 28};
        maps_hierarchy[24] = new int[]{28};
        maps_hierarchy[25] = new int[]{29};
        maps_hierarchy[26] = new int[]{29};
        maps_hierarchy[27] = new int[]{29};
        maps_hierarchy[28] = new int[]{29};
        maps_hierarchy[29] = null;
        maps_hierarchy[30] = new int[]{38};
        maps_hierarchy[31] = new int[]{39};
        maps_hierarchy[32] = new int[]{40};
        maps_hierarchy[33] = new int[]{41};
        maps_hierarchy[34] = new int[]{42};
        maps_hierarchy[35] = new int[]{43};
        maps_hierarchy[36] = new int[]{44};
        maps_hierarchy[37] = new int[]{45};
        maps_hierarchy[38] = new int[]{46};
        maps_hierarchy[39] = new int[]{47};
        maps_hierarchy[40] = new int[]{48};
        maps_hierarchy[41] = new int[]{49};
        maps_hierarchy[42] = new int[]{50};
        maps_hierarchy[43] = new int[]{51};
        maps_hierarchy[44] = new int[]{52};
        maps_hierarchy[45] = new int[]{53};
        maps_hierarchy[46] = null;
        maps_hierarchy[47] = null;
        maps_hierarchy[48] = null;
        maps_hierarchy[49] = null;
        maps_hierarchy[50] = null;
        maps_hierarchy[51] = null;
        maps_hierarchy[52] = null;
        maps_hierarchy[53] = null;
    }

    private void ShuffleCards()
    {
        int currentIndex;
        GameObject tempValue;
        for (int i = 0; i < 54; i++)
        {
            currentIndex = Random.Range(0, 54 - i);
            tempValue = cards[currentIndex];
            cards[currentIndex] = cards[53 - i];
            cards[53 - i] = tempValue;
        }
    }

    private void InitGameObjectsOnCardsMap()
    {
        for (int i = 0; i < 54; i++)
        {
            if (maps_hierarchy[i] == null)
            {
                GameObject obj = (GameObject)Instantiate(cards[i], 
                    maps_pos[i], Quaternion.Euler(maps_rotate[i]));
                obj.name = "" + i;
                cardsTurnedOnCardMap.Add(obj);
                gameObjectsOnCardMap[i] = obj;
                continue;
            }
            else
            {
                GameObject obj = (GameObject)Instantiate(cardBack, maps_pos[i], Quaternion.Euler(maps_rotate[i]));
                obj.name = "" + i;
                gameObjectsOnCardMap[i] = obj;
            }
        }
    }

    private void InitUI()
    {
        gameOverCanvas.SetActive(false);
        scoreText.text = "" + score;
        lifeText.text = "" + lifeCount;
    }

    private bool DidOneRoundWin()
    {
        return cardsTurnedOnCardMap.Count == 0;
    }

    private bool DidOneRoundLose()
    {
        for (int i = 0; i < cardsTurnedOnCardMap.Count; i++)
        {
            List<GameObject> allGameObjects = 
                cardsTurnedOnCardMap.FindAll(card => (card.CompareTag(cardsTurnedOnCardMap[i].tag) && card != cardsTurnedOnCardMap[i]));
            if (allGameObjects != null && allGameObjects.Count >= 1)
            {
                return false;
            }
        }
        return true;
    }

    private void TurnbackReversedCard(GameObject hitGameObject)
    {
        int indexOfHitCard = int.Parse(hitGameObject.name);
        if (CanTurnBack(indexOfHitCard))
        {
            Destroy(hitGameObject);
            GameObject obj = (GameObject)Instantiate(cards[indexOfHitCard],
                maps_pos[indexOfHitCard],
                Quaternion.Euler(maps_rotate[indexOfHitCard]));
            obj.name = "" + indexOfHitCard;
            gameObjectsOnCardMap[indexOfHitCard] = obj;
            cardsTurnedOnCardMap.Add(obj);

            UIButtonUndo.invoker.AddCommand(new TurnCommand(this, 
                new int[] { indexOfHitCard }));
        }
    }

    private bool CanTurnBack(int indexOfCard)
    {
        if (maps_hierarchy[indexOfCard] == null)
            return true;
        else
        {
            int[] upCards = maps_hierarchy[indexOfCard];
            bool aboveCardsCleard = true;
            foreach (int i in upCards)
            {
                aboveCardsCleard &= (gameObjectsOnCardMap[i] == null);
            }
            return aboveCardsCleard;
        }
    }

    private Vector3 GetEliminatedCardPosition(int count)
    {
        if (count <= 27)
            return new Vector3(-4.55f + (count - 1) * 0.35f,  -5.78f, -0.1f - 0.01f * count);
        else
            return new Vector3(-4.55f + (count - 28) * 0.35f, -7.30f, -0.1f - 0.01f * count);
    }

    public void TurnCards(int[] cardsIndex)
    {
        foreach (int i in cardsIndex)
        {
            GameObject obj = gameObjectsOnCardMap[i];
            Destroy(obj);
            obj = (GameObject)Instantiate(cards[i],
                maps_pos[i],
                Quaternion.Euler(maps_rotate[i]));
            obj.name = "" + i;
            gameObjectsOnCardMap[i] = obj;
            cardsTurnedOnCardMap.Add(obj);
        }
        AudioSource.PlayClipAtPoint(cardTurnMusic,
            gameObject.transform.position);
    }

    public void UnturnCards(int[] cardsIndex)
    {
        foreach (int i in cardsIndex)
        {
            GameObject obj = gameObjectsOnCardMap[i];
            cardsTurnedOnCardMap.Remove(obj);
            Destroy(obj);
            obj = (GameObject)Instantiate(cardBack,
                maps_pos[i],
                Quaternion.Euler(maps_rotate[i]));
            obj.name = "" + i;
            gameObjectsOnCardMap[i] = obj;
        }
        AudioSource.PlayClipAtPoint(cardTurnMusic,
            gameObject.transform.position);
    }

    public void EliminateCards(int firstCardIndex, int secondCardIndex)
    {
        GameObject firstCard = gameObjectsOnCardMap[firstCardIndex];
        gameObjectsOnCardMap[firstCardIndex] = null;
        gameObjectsOffCardMap[firstCardIndex] = firstCard;
        cardsTurnedOnCardMap.Remove(firstCard);
        eliminatedCardCount++;
        firstCard.transform.position = GetEliminatedCardPosition(eliminatedCardCount);
        firstCard.transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 0.0f));
        GameObject cardBackForFirstCard = (GameObject)Instantiate(cardBack, firstCard.transform.position,
            firstCard.transform.rotation);
        cardBackForFirstCard.transform.position = new Vector3(cardBackForFirstCard.transform.position.x,
            cardBackForFirstCard.transform.position.y, cardBackForFirstCard.transform.position.z - 0.01f);
        cardBacksOffCardMap[firstCardIndex] = cardBackForFirstCard;
        cardBackForFirstCard.SetActive(!showElimatedCards);

        GameObject secondCard = gameObjectsOnCardMap[secondCardIndex];
        gameObjectsOnCardMap[secondCardIndex] = null;
        gameObjectsOffCardMap[secondCardIndex] = secondCard;
        cardsTurnedOnCardMap.Remove(secondCard);
        eliminatedCardCount++;
        secondCard.transform.position = GetEliminatedCardPosition(eliminatedCardCount);
        secondCard.transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 0.0f));
        GameObject cardBackForSecondCard = (GameObject)Instantiate(cardBack, secondCard.transform.position,
            secondCard.transform.rotation);
        cardBackForSecondCard.transform.position = new Vector3(cardBackForSecondCard.transform.position.x,
            cardBackForSecondCard.transform.position.y, cardBackForSecondCard.transform.position.z - 0.01f);
        cardBacksOffCardMap[secondCardIndex] = cardBackForSecondCard;
        cardBackForSecondCard.SetActive(!showElimatedCards);

        TurnAvailiableCards();

        selctedCard = null;
        score += 10;
        if (score - preWinLifeScore >= LIFEADDTHRESHOLD)
        {
            lifeCount++;
            lifeText.text = "" + lifeCount;
            preWinLifeScore = score;
        }
        scoreText.text = "" + score;
    }

    public void UneliminateCards(int firstCardIndex, int secondCardIndex)
    {
        GameObject firstCard = gameObjectsOffCardMap[firstCardIndex];
        gameObjectsOnCardMap[firstCardIndex] = firstCard;
        gameObjectsOffCardMap[firstCardIndex] = null;
        cardsTurnedOnCardMap.Add(firstCard);
        eliminatedCardCount--;
        firstCard.transform.position = maps_pos[firstCardIndex];
        firstCard.transform.rotation = Quaternion.Euler(maps_rotate[firstCardIndex]);
        Destroy(cardBacksOffCardMap[firstCardIndex]);
        cardBacksOffCardMap[firstCardIndex] = null;

        GameObject secondCard = gameObjectsOffCardMap[secondCardIndex];
        gameObjectsOnCardMap[secondCardIndex] = secondCard;
        gameObjectsOffCardMap[secondCardIndex] = null;
        cardsTurnedOnCardMap.Add(secondCard);
        eliminatedCardCount--;
        secondCard.transform.position = maps_pos[secondCardIndex];
        secondCard.transform.rotation = Quaternion.Euler(maps_rotate[secondCardIndex]);
        Destroy(cardBacksOffCardMap[secondCardIndex]);
        cardBacksOffCardMap[secondCardIndex] = null;

        UnturnAvailiableCards();

        selctedCard = null;
        if (score == preWinLifeScore)
        {
            preWinLifeScore = score - LIFEADDTHRESHOLD;
            lifeCount--;
            lifeText.text = "" + lifeCount;
        }
        score -= 10;
        scoreText.text = "" + score;

        if (gameSceneState != GameState.Playing)
            gameSceneState = GameState.Playing;

        if (lifeCount <= 0)
            lifeCount++;
    }

    public void updateElimatedCardBacks()
    {
        for (int i = 0; i < 54; i++)
        {
            if (cardBacksOffCardMap[i] != null)
                cardBacksOffCardMap[i].SetActive(!showElimatedCards);
        }
    }

    private void TurnAvailiableCards()
    {
        List<int> turnedCardsIndex = new List<int>();
        for (int i = 0; i < 54; i++)
        {
            if (null != gameObjectsOnCardMap[i] && gameObjectsOnCardMap[i].CompareTag("CardBack"))
            {
                if (CanTurnBack(i))
                {
                    turnedCardsIndex.Add(i);
                }
            }
        }
        TurnCards(turnedCardsIndex.ToArray());
    }

    private void UnturnAvailiableCards()
    {
        List<int> unturnedCardsIndex = new List<int>();
        foreach (GameObject card in cardsTurnedOnCardMap)
        {
            int index = int.Parse(card.name);
            if (!CanTurnBack(index))
            {
                unturnedCardsIndex.Add(index);
            }
        }
        UnturnCards(unturnedCardsIndex.ToArray());
    }

    public abstract class Command
    {
        public abstract void Execute();
        public abstract void UnExecute();
    }

    private class TurnCommand : Command
    {
        private int[] turnedCardsIndexes;
        private GameScene gm;

        public TurnCommand(GameScene gm, int[] turnedCardsIndexes)
        {
            this.gm = gm;
            this.turnedCardsIndexes = turnedCardsIndexes;
        }

        public override void Execute()
        {
            gm.TurnCards(turnedCardsIndexes);
        }

        public override void UnExecute()
        {
            gm.UnturnCards(turnedCardsIndexes);
        }
    }

    private class EliminateCommand : Command
    {
        private int firstCardIndex;
        private int secondCardIndex;
        private GameScene gm;

        public EliminateCommand(GameScene gm, int firstCardIndex, int secondCardIndex)
        {
            this.gm = gm;
            this.firstCardIndex = firstCardIndex;
            this.secondCardIndex = secondCardIndex;
        }
        public override void Execute()
        {
            gm.EliminateCards(firstCardIndex, secondCardIndex);
        }

        public override void UnExecute()
        {
            gm.UneliminateCards(firstCardIndex, secondCardIndex);
        }
    }
}
