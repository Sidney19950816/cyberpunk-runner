using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Managers;
using Assets.Scripts;

public class TutorialWindow : MonoBehaviour
{
    [Header("Images")]
    [SerializeField] private Image leftArrow;
    [SerializeField] private Image rightArrow;
    [SerializeField] private Image uiMaskImage;

    [Space, Header("Sprites")]
    [SerializeField] private Sprite circleMaskImage;
    [SerializeField] private Sprite rectangleMaskImage;

    [Space, Header("Transform")]
    [SerializeField] private RectTransform boxTransform;
    [SerializeField] private RectTransform handTransform;

    [Space, Header("Text")]
    [SerializeField] private Text infoText;

    public void Awake()
    {
        DisableAll();
    }

    public void SetState(TutorialManager.TutorialState tutorialState, string info)
    {
        switch(tutorialState)
        {
            case TutorialManager.TutorialState.TiltPhone:
                leftArrow.SetActive(true);
                rightArrow.SetActive(true);
                boxTransform.SetActive(true);
                boxTransform.localPosition = Vector3.zero;

                uiMaskImage.SetActive(true);
                uiMaskImage.sprite = circleMaskImage;
                uiMaskImage.rectTransform.sizeDelta = new Vector2(90, 90);
                break;
            case TutorialManager.TutorialState.TapToBreak:
                leftArrow.SetActive(false);
                rightArrow.SetActive(false);
                handTransform.SetActive(true);
                handTransform.localPosition = new Vector3(470, -95, 0);
                handTransform.rotation = Quaternion.Euler(0, 0, 30);
                boxTransform.localPosition = new Vector3(470, 150, 0);
                uiMaskImage.transform.position = handTransform.position;
                break;
            case TutorialManager.TutorialState.TapToShoot:
                if (StateManager.CurrentState is FightState)
                {
                    FightState fightState = StateManager.CurrentState as FightState;
                    StartCoroutine(MoveHand(fightState.Enemies[0], 1)); // delayTime is set to 1 sec, because the camera transition time takes 1 sec
                }
                break;
            case TutorialManager.TutorialState.EarnRokens:
                uiMaskImage.sprite = rectangleMaskImage;
                uiMaskImage.rectTransform.sizeDelta = new Vector2(290, 125);

                uiMaskImage.rectTransform.anchorMax = Vector2.one;
                uiMaskImage.rectTransform.anchorMin = Vector2.one;
                uiMaskImage.rectTransform.pivot = Vector2.one;
                uiMaskImage.rectTransform.anchoredPosition = Vector3.zero;

                handTransform.SetActive(true);
                handTransform.position = GetBottomLeftPoint(uiMaskImage.rectTransform);
                handTransform.rotation = Quaternion.Euler(0, 0, -60);

                boxTransform.SetActive(true);
                boxTransform.pivot = new Vector2(0.5f, 1);
                boxTransform.position = GetBottomRightPosition(handTransform);
                break;
            case TutorialManager.TutorialState.MotorbikeBattery:
                uiMaskImage.SetActive(true);
                uiMaskImage.sprite = rectangleMaskImage;
                uiMaskImage.rectTransform.sizeDelta = new Vector2(500, 125);

                uiMaskImage.rectTransform.anchorMax = new Vector2(0.5f, 1);
                uiMaskImage.rectTransform.anchorMin = new Vector2(0.5f, 1);
                uiMaskImage.rectTransform.pivot = new Vector2(0.5f, 1);
                uiMaskImage.rectTransform.anchoredPosition = Vector3.zero;

                handTransform.SetActive(true);
                handTransform.position = GetBottomCenterPosition(uiMaskImage.rectTransform);
                handTransform.rotation = Quaternion.Euler(0, 0, 0);

                boxTransform.SetActive(true);
                boxTransform.pivot = new Vector2(0.5f, 1);
                boxTransform.position = new Vector3(GetBottomCenterPosition(handTransform).x + 20, GetBottomCenterPosition(handTransform).y - 20);
                break;
            default:
                transform.SetActive(false);
                break;
        }

        infoText.text = info;
    }

    private Vector3 GetBottomLeftPoint(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        return corners[0];
    }

    private Vector3 GetBottomCenterPosition(RectTransform rectTransform)
    {
        Vector3 pivotOffset = new Vector3(
            0f,
            rectTransform.rect.height * (-rectTransform.pivot.y),
            0f);

        Vector3 worldBottomCenter = rectTransform.TransformPoint(pivotOffset);

        return worldBottomCenter;
    }

    private Vector3 GetBottomRightPosition(RectTransform rectTransform)
    {
        Vector3 pivotOffset = new Vector3(
            rectTransform.rect.width * (1f - rectTransform.pivot.x),
            rectTransform.rect.height * -rectTransform.pivot.y,
            0f);

        Vector3 worldBottomRight = rectTransform.TransformPoint(pivotOffset);

        return worldBottomRight;
    }

    private Vector3 GetTopRightPosition(RectTransform rectTransform)
    {
        Vector3 pivotOffset = new Vector3(
            rectTransform.rect.width * (1f - rectTransform.pivot.x),
            rectTransform.rect.height * (1f - rectTransform.pivot.y),
            0f);

        Vector3 worldTopRight = rectTransform.TransformPoint(pivotOffset);

        return worldTopRight;
    }

    private IEnumerator MoveHand(Enemy enemy, float delayTime)
    {
        handTransform.SetActive(false);
        boxTransform.SetActive(false);
        uiMaskImage.SetActive(false);

        yield return new WaitForSecondsRealtime(delayTime);

        handTransform.SetActive(true);
        boxTransform.SetActive(true);
        uiMaskImage.SetActive(true);

        Vector3 enemyPos = Camera.main.WorldToScreenPoint(enemy.Head.transform.position);
        handTransform.position = enemyPos;
        handTransform.rotation = Quaternion.Euler(0, 0, 30);

        boxTransform.pivot = new Vector2(0.5f, 0);
        boxTransform.position = GetTopRightPosition(handTransform);
        uiMaskImage.transform.position = handTransform.position;
    }

    private void DisableAll()
    {
        foreach(Transform child in transform)
        {
            child.SetActive(false);
        }
    }
}
