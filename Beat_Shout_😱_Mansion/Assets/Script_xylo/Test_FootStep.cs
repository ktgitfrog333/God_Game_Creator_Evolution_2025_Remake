using UnityEngine;
using System.Collections;

public class Test_FootStep : MonoBehaviour
{
    public float minInterval = 1.5f;  // Å‰‚Ì‘«‰¹ŠÔŠui•bj
    public float maxInterval = 0.9f;  // ÅŒã‚Ì‘«‰¹ŠÔŠui•bj
    public float accelerationTime = 4.0f;  // ‰½•b‚©‚¯‚Ä‰Á‘¬‚·‚é‚©

    private Coroutine footstepCoroutine;
    private bool isPlaying = false;
    // [2026/07/03] Amagata Support for Implementing Custom Sound Effects start
    /// <summary>
    /// 0: Default, 1: Wood
    /// </summary>
    private int _floorType = 0;
    // [2026/07/03] Amagata Support for Implementing Custom Sound Effects end

    void Update()
    {
        // FƒL[‚ª‰Ÿ‚³‚ê‚½‚Æ‚«
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartFootsteps();
        }
        // FƒL[‚ª—£‚³‚ê‚½‚Æ‚«
        if (Input.GetKeyUp(KeyCode.F))
        {
            StopFootsteps();
        }
    }

    void StartFootsteps()
    {
        if (!isPlaying)
        {
            isPlaying = true;
            footstepCoroutine = StartCoroutine(FootstepRoutine());
        }
    }

    void StopFootsteps()
    {
        if (isPlaying)
        {
            if (footstepCoroutine != null)
            {
                StopCoroutine(footstepCoroutine);
            }
            isPlaying = false;
        }
    }

    IEnumerator FootstepRoutine()
    {
        float elapsedTime = 0f;

        while (true)
        {
            // Œo‰ßŠÔ‚ÉŠî‚Ã‚¢‚ÄŠÔŠu‚ğŒvZ
            float t = Mathf.Clamp01(elapsedTime / accelerationTime);
            float currentInterval = Mathf.Lerp(minInterval, maxInterval, t);

            // ‘«‰¹‚ğÄ¶
            PlayFootStep();

            // ŒvZ‚³‚ê‚½ŠÔŠu‚¾‚¯‘Ò‹@
            yield return new WaitForSeconds(currentInterval);

            elapsedTime += currentInterval;
        }
    }

    void PlayFootStep()
    {
        // [2026/07/03] Amagata Support for Implementing Custom Sound Effects start
        //SE_Picker.Instance.PlayFootStep(1);
        var type = _floorType;
        switch (type)
        {
            case 1:
                SE_Picker.Instance.PlayBUB_footstep_wood(1);

                break;
            default:
                SE_Picker.Instance.PlayBUB_footstep(1);

                break;
        }
        // [2026/07/03] Amagata Support for Implementing Custom Sound Effects end
    }
    // [2026/07/03] Amagata Support for Implementing Custom Sound Effects start
    public void SetFloorType(int floorType)
    {
        _floorType = floorType;
    }
    // [2026/07/03] Amagata Support for Implementing Custom Sound Effects end
}