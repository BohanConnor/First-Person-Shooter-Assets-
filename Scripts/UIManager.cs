using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameObject hitUI;
    public GameObject deathUI;
    public TextMeshProUGUI ammoText;
    public Image healthBar;
    public Gradient healthGradient;
    public int enemiesLeft = 25;
    public TextMeshProUGUI enemiesLeftText;
    public GameObject winUI;
    private void Awake()
    {
        Time.timeScale = 1f;
        Instance = this;
        enemiesLeftText.text = "Enemies Left: " + enemiesLeft;
    }
    public void InstantiateHitUI()
    {
        Instantiate(hitUI, transform);
    }
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void EnableDeathUI()
    {
        deathUI.SetActive(true);
    }
    public void SetHealthValue(int health)
    {
        float floathealth = (float)health / 100f;
        healthBar.color = healthGradient.Evaluate(floathealth);
        healthBar.fillAmount = floathealth;
    }
    public void EnableWinUI()
    {
        winUI.SetActive(true);
    }
}
