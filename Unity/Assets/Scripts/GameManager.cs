using System;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    enum GameState
    {
        MainMenu,
        Game,
        Pause,
        Results
    }

    GameState currentState;
    Canvas PauseScreen;
    public GameObject Player;
    float score;
    public TextMeshProUGUI scoreCard;
    public GameObject playUICanvas;
    public GameObject resultsCanvas;
    TerrainGenerator terrainGenerator;
    PlayerManager playerManager;
    public void StartGame()
    {
        Player.SetActive(true);
        currentState = GameState.Game;
    }

    public void RestartGame()
    {
        terrainGenerator.SetVariablesForRestart();
        Player.SetActive(true);
        playerManager.StartPlayer();
        currentState = GameState.Game;
        score = 0;
    }

    public void PauseGame()
    {
        currentState = GameState.Pause;
        playerManager.Pause();
        PauseScreen.enabled = true;
    }

    public void PlayGame()
    {
        currentState = GameState.Game;
        playerManager.Play();
        PauseScreen.enabled = false;
    }

    public void GameOver()
    {
        currentState = GameState.Results;
        playUICanvas.SetActive(false);
        resultsCanvas.SetActive(true);
        resultsCanvas.GetComponentsInChildren<TextMeshProUGUI>().First(it => it.name.Equals("Score")).text = Mathf.RoundToInt(score).ToString();
        playerManager.Pause();
        foreach (Transform transform in terrainGenerator.transform)
        {
            Destroy(transform.gameObject);
        }
        Player.SetActive(false);
    }

    void Start()
    {
        score = 0;
        terrainGenerator = gameObject.GetComponent<TerrainGenerator>();
        playerManager = Player.GetComponentInChildren<PlayerManager>();
        currentState = GameState.MainMenu;
    }

    void Update()
    {

        if (currentState == GameState.Game)
        {
            score += Time.deltaTime;
            scoreCard.text = Mathf.RoundToInt(score).ToString();
            terrainGenerator.HandleBlocks();
            int moveSpeed = Mathf.Min(60 + (int)score, 160);
            terrainGenerator.moveSpeed = moveSpeed;
            int difficulty = 1;

            if (score > Math.Pow(5, 1) && score < Math.Pow(5, 2))
            {
                difficulty = 2;
            }
            else if (score > Math.Pow(5, 2) && score < Math.Pow(5, 3))
            {
                difficulty = 3;
            }
            terrainGenerator.difficulty = difficulty;

            float animatorSpeed = (moveSpeed-60)/ 100f + 1;
            playerManager.SetAnimatorSpeed(animatorSpeed);
        }

        if (playerManager.isDead && currentState != GameState.Results)
        {
            GameOver();
        }

    }

    void FixedUpdate()
    {
        if (currentState == GameState.Game)
        {
            terrainGenerator.AddColliders();
        }
    }
}