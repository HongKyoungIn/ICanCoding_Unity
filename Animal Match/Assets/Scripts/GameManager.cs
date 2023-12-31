using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public static GameManager instance; // 싱글 톤 방식
    private List<Card> allCards;
    private Card flippedCard; // 뒤집힌 카드 정보를 저장하기 위한 참조 변수
    private bool isFlipping = false;

    [SerializeField]
    private Slider timeoutSlider; // 타임 슬라이더 컴포넌트

    [SerializeField]
    private TextMeshProUGUI timeoutText; // 제한 시간 텍스트를 저장

    [SerializeField]
    private float timeLimit = 60; // 제한 시간 저장하는 변수

    [SerializeField]
    private GameObject gameOverPanel;

    [SerializeField]
    private TextMeshProUGUI gameOverText;

    private bool isGameOver = false;

    private float currentTime; // 현재 남은시간 저장하는 변수
    private int totalMatches = 10; // 총 찾는 매치 쌍 값, 게임 종료 조건
    private int matchesFound = 0; // 찾은 매치 쌍

    private void Awake() {
        if(instance == null) {
            instance = this;
        }
    }

    void Start() {
        Board board = FindObjectOfType<Board>(); // 보드 객체 찾아오기
        allCards = board.GetCards(); // 게임 매니저에서 20장의 카드에 저장하고 접근하기 위한 변수

        currentTime = timeLimit; // 현재 시간에 제한 시간 값 대입
        SetCurrentTimeText();
        StartCoroutine("FlipAllCardsRoutine"); // 모든 카드 뒤집는 코루틴 실행
    }

    void SetCurrentTimeText() {
        int timeSec = Mathf.CeilToInt(currentTime); // 올림을 이용하여 소수를 정수로 표현
        timeoutText.SetText(timeSec.ToString());
    }

    IEnumerator FlipAllCardsRoutine() {
        isFlipping = true;
        yield return new WaitForSeconds(0.5f);
        FlipAllCards();
        yield return new WaitForSeconds(3f);
        FlipAllCards();
        yield return new WaitForSeconds(0.5f);
        isFlipping = false;

        yield return StartCoroutine("CountDownTimerRoutine"); // 타임아웃 코루틴 실행
    }

    IEnumerator CountDownTimerRoutine() {
        while(currentTime > 0) {
            currentTime -= Time.deltaTime; // 현재 시간에서 1초씩 감소
            timeoutSlider.value = currentTime / timeLimit; // 남은시간에 따른 슬라이더 fill의 value값 설정
            SetCurrentTimeText(); // 시간 텍스트 감소 시키기
            yield return null;
        }

        GameOver(false);
    }
    
    void FlipAllCards() { // 모든 카드 한 번 뒤집는 메소드
        foreach (Card card in allCards) {
            card.FlipCard();
        }
    }

    public void CardCliked(Card card) {
        if(isFlipping || isGameOver) {
            return;
        }

        card.FlipCard();

        if(flippedCard == null) { // 아직 뒤집힌 카드가 없다면
            flippedCard = card; // 지금 뒤집힌 카드를 저장
        }
        else {
            StartCoroutine(CheckMatchRoutine(flippedCard, card));
        }
    }

    IEnumerator CheckMatchRoutine(Card card1, Card card2) {
        isFlipping = true;


        if(card1.cardID == card2.cardID) {
            card1.SetMatched();
            card2.SetMatched();
            matchesFound++;

            if(matchesFound == totalMatches) {
                GameOver(true);
            }
        }
        else {
            yield return new WaitForSeconds(1f);

            card1.FlipCard();
            card2.FlipCard();

            yield return new WaitForSeconds(0.4f);
        }

        isFlipping = false;
        flippedCard = null;
    }

    void GameOver(bool success) {

        if(!isGameOver) {
            isGameOver = true;

            StopCoroutine("CountDownTimerRoutine");

            if (success) {
                gameOverText.SetText("Great Job");
            }
            else {
                gameOverText.SetText("Game Over");
            }

            Invoke("ShowGameOverPanel", 0.5f);
        }
    }

    void ShowGameOverPanel() {
        gameOverPanel.SetActive(true);
    }

    public void Restart() {
        SceneManager.LoadScene("SampleScene");
    }
}
