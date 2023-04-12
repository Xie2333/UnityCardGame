using DG.Tweening;
using Protocol.Dto.Fight;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOver_Net : MonoBehaviour
{

    [System.Serializable]
    public class Player
    {
        public Text txt_UserName;
        public Text txt_CoinCount;
    }
    public Player lose_1;
    public Player lose_2;
    public Player win;

    public Button btn_Again;
    public Button btn_MainMenu;

    private void Awake()
    {
        EventCenter.AddListener<GameOverDto>(EventDefine.GameOverBRO, GameOver);
        btn_Again.onClick.AddListener(OnagainButtonClick);
        btn_MainMenu.onClick.AddListener(OnMainMenuButtonClick);
    }
    private void OnDestroy()
    {
        EventCenter.RemoveListener<GameOverDto>(EventDefine.GameOverBRO, GameOver);
    }
    private void OnMainMenuButtonClick()
    {
        SceneManager.LoadScene("2.Main");
    }
    private void OnagainButtonClick()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    private void GameOver(GameOverDto dto)
    {
        transform.DOScale(Vector3.one, 0.3f);
        win.txt_UserName.text = dto.winDto.userName;
        win.txt_CoinCount.text = dto.winCount.ToString();

        lose_1.txt_UserName.text = dto.loseDtoList[0].userName;
        lose_1.txt_CoinCount.text = (-dto.loseDtoList[0].stakeSum).ToString();
        lose_2.txt_UserName.text = dto.loseDtoList[1].userName;
        lose_2.txt_CoinCount.text = (-dto.loseDtoList[1].stakeSum).ToString();
    }
}
