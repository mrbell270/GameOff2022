using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private static LevelManager instance;

    public int curLevel;
    public int totalLevels;
    [HideInInspector]
    public int offsetPlayable;
    [HideInInspector]
    public bool isFinished = false;

    [SerializeField]
    SpriteRenderer fade;
    [SerializeField]
    SpriteRenderer finalFade;

    public static LevelManager GetInstance()
    {
        return instance;
    }

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        instance = this;
        
        offsetPlayable = 2;
        fade = GetComponent<SpriteRenderer>();
        fade.color = Vector4.zero;
        finalFade.color = Vector4.zero;

        StartCoroutine(LevelLoadingInitial(1));
    }

    IEnumerator LevelLoadingInitial(int loadIdx)
    {
        if (SceneManager.sceneCount < 2)
        {
            SceneManager.LoadSceneAsync(loadIdx, LoadSceneMode.Additive);
        }
        while (!SceneManager.GetSceneAt(1).isLoaded)
        {
            yield return null;
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));

        if (Player.GetInstance() != null)
            Player.GetInstance().GetPlayerBody();

        curLevel = SceneManager.GetActiveScene().buildIndex - offsetPlayable;
        totalLevels = SceneManager.sceneCountInBuildSettings - offsetPlayable;
    }

    public void LoadNextLevel()
    {
        int currentBuild = curLevel + offsetPlayable;
        curLevel++;
        if (curLevel + 1 > totalLevels)
        {
            curLevel--;
        }
        int newBuild = curLevel + offsetPlayable;
        StartCoroutine(LevelLoading(newBuild, currentBuild));
    }

    public void LoadLevel(int levelNum, bool finalFadeFlg)
    {
        int currentBuild = curLevel + offsetPlayable;
        int newBuild = levelNum + offsetPlayable;
        StartCoroutine(LevelLoading(newBuild, currentBuild, finalFadeFlg));
        curLevel = levelNum;
    }

    IEnumerator LevelLoading(int loadIdx, int unloadIdx, bool finalFadeFlg=false)
    {
        if (unloadIdx != 1)
        {
            float overloadTimer = 0.5f;
            LightController[] lights = FindObjectsOfType<LightController>();
            AudioManager.GetInstance().PlayClip("lights_overload");
            foreach (LightController lc in lights)
            {
                lc.Overload(overloadTimer);
            }
        }

        if (finalFadeFlg)
        {
            finalFade.color = Color.white;
        }
        else
        {
            yield return new WaitForSeconds(1f);
            fade.color = Color.black;
        }

        yield return new WaitForSeconds(1.5f);

        SceneManager.UnloadSceneAsync(unloadIdx);
        SceneManager.LoadScene(loadIdx, LoadSceneMode.Additive);
        while (!SceneManager.GetSceneAt(1).isLoaded)
        {
            yield return null;
        }

        if (finalFadeFlg)
        {
            finalFade.color = Vector4.zero;
        }
        else
        {
            fade.color = Vector4.zero;
        }

        SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));
        if (Player.GetInstance() != null)
            Player.GetInstance().GetPlayerBody();
    }

    public bool hasNextLevel()
    {
        return curLevel + 1 < totalLevels;
    }

    public void LoadEning()
    {
        LoadLevel(-1, true);
        isFinished = true;
        Debug.Log("FIN!");
    }
}
