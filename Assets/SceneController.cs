using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    [SerializeField]
    private GameObject enemyPrefab;

    [SerializeField]
    private int enemyCount = 5;

    [SerializeField]
    public TMPro.TextMeshProUGUI enemyCountText;

    [SerializeField]
    public GameObject pausePanel;

    [SerializeField]
    private GameObject player;

    private float nextEnemyTime = 3f;

    protected static CancellationTokenSource cancelSource;

    public static SceneController instance = null;

    private async void Start()
    {
        cancelSource = new();

        if (!instance)
            instance = this;

        enemyCountText.text = $"Enemies Left: {enemyCount}";

        pausePanel.SetActive(false);

        try
        {
            await SpawnEnemies(cancelSource.Token);
        }
        catch(System.OperationCanceledException)
        {
            return;
        }
    }

    private void LateUpdate()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = 0;
            //CharacterController controller = player.GetComponent<CharacterController>();
            //controller.enabled = false;
            MouseLook look = player.GetComponent<MouseLook>();
            look.enabled = false;
            FPSInput input = player.GetComponent<FPSInput>();
            input.enabled = false;
            PlayerStats stats = player.GetComponent<PlayerStats>();
            stats.enabled = false;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            pausePanel.SetActive(true);
        }
    }

    protected void OnDestroy()
    {
        cancelSource.Cancel();
        cancelSource.Dispose();
    }

    private async Task SpawnEnemies(CancellationToken token)
    {
        for(int i = 0; i <= enemyCount; i++)
        {
            await Task.Delay(100 * (int)(nextEnemyTime * 10f));

            token.ThrowIfCancellationRequested();

            nextEnemyTime = Random.Range(2f, 5f);

            float distance = Random.Range(5f, 25f);
            Vector3 pos;
            do
            {
                pos = WanderingAI.RandomNavSphere(transform.position, distance, -1);
            } while (float.IsInfinity(pos.x));

            //pos.y += 15f;

            float angle = Random.Range(0f, 360f);

            Quaternion rotation = Quaternion.Euler(0, angle, 0);

            Instantiate<GameObject>(enemyPrefab, pos, rotation);
        }
    }

    public async void removeEnemy()
    {
        enemyCount--;
        enemyCountText.text = $"Enemies Left: {enemyCount}";

        if(enemyCount == 0)
        {
            enemyCountText.text = "YOU WIN!!!";

            await Task.Delay(3000);

            Time.timeScale = 0;

            //CharacterController controller = player.GetComponent<CharacterController>();
            //controller.enabled = false;
            MouseLook look = player.GetComponent<MouseLook>();
            look.enabled = false;
            FPSInput input = player.GetComponent<FPSInput>();
            input.enabled = false;
            PlayerStats stats = player.GetComponent<PlayerStats>();
            stats.enabled = false;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            pausePanel.SetActive(true);
        }
    }

    public void ContinueButtonHandler()
    {
        pausePanel.SetActive(false);
        //CharacterController controller = player.GetComponent<CharacterController>();
        //controller.enabled = false;
        MouseLook look = player.GetComponent<MouseLook>();
        look.enabled = true;
        FPSInput input = player.GetComponent<FPSInput>();
        input.enabled = true;
        PlayerStats stats = player.GetComponent<PlayerStats>();
        stats.enabled = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Time.timeScale = 1;
    }

    public void QuitButtonHandler()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(1);
#endif
    }

    public void PlayerLose()
    {
        Time.timeScale = 0;
        //CharacterController controller = player.GetComponent<CharacterController>();
        //controller.enabled = false;
        MouseLook look = player.GetComponent<MouseLook>();
        look.enabled = false;
        FPSInput input = player.GetComponent<FPSInput>();
        input.enabled = false;
        PlayerStats stats = player.GetComponent<PlayerStats>();
        stats.enabled = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        pausePanel.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Press Continue to restart level";

        pausePanel.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        pausePanel.GetComponentInChildren<Button>().onClick.AddListener(delegate { PlayerLoseContinueButton(); });

        pausePanel.SetActive(true);
    }

    public void PlayerLoseContinueButton()
    {
        SceneManager.LoadScene(0);
    }
}
