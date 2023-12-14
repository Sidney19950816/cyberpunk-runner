using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TermsView : BaseView
{
    [SerializeField] private TextMeshProUGUI termsAndConditionsText;
    [SerializeField] private Button acceptButton;

    private void Start()
    {
        // Set the main text content
        string mainText = "In order to play Rokencity Battle Racing game, you need to confirm that you agree to our {0} and have read our {1}.";

        // Set the clickable links
        string termsAndConditionsLink = "<link=https://fooplix.com/terms-of-service><color=#E4A5FF><u>Terms and Conditions</u></color></link>";
        string privacyPolicyLink = "<link=https://fooplix.com/privacy-policy><color=#E4A5FF><u>Privacy Policy</u></color></link>";

        // Replace placeholders with clickable links
        string finalText = string.Format(mainText, termsAndConditionsLink, privacyPolicyLink);

        // Set the text with clickable links, underline, and color(#E4A5FF)
        termsAndConditionsText.text = finalText;

        // Create an EventTrigger for the link
        EventTrigger trigger = termsAndConditionsText.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => { HandleLinkClick((PointerEventData)data, termsAndConditionsText); });
        trigger.triggers.Add(entry);

        //Add Listener to AcceptButton
        acceptButton.onClick.AddListener(OnAcceptButtonClick);
    }

    private void HandleLinkClick(PointerEventData data, TextMeshProUGUI text)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, data.position, null);
        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];
            string url = linkInfo.GetLinkID();

            Application.OpenURL(url);
        }
    }

    public override void UpdateView(BaseState state)
    {
        if(state is MainMenuState && !PlayerPrefsUtil.GetTermsAccepted())
        {
            Show();
        }    
    }

    private void OnAcceptButtonClick()
    {
        PlayerPrefsUtil.SetTermsAccepted();
        Hide();
    }
}
