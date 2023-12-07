using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppsFlyerSDK;
using System;

// This class is intended to be used the the AppsFlyerObject.prefab

public class AppsFlyerObjectScript : MonoBehaviour , IAppsFlyerConversionData
{

    // These fields are set from the editor so do not modify!
    //******************************//
    public string devKey;
    public string appID;
    public string UWPAppID;
    public string macOSAppID;
    public bool isDebug;
    public bool getConversionData;
    //******************************//

    #region AppsFlyer Related Fields

    private bool _didReceivedDeepLink; // marks if we got a DL and processed it
    private bool _deferred_deep_link_processed_flag; // only for Legacy Links users - marks if the Deffered DL was processed by UDL or not
    private string _userInviteLink;
    private Dictionary<string, object> _conversionDataDictionary;
    private Dictionary<string, object> _deepLinkParamsDictionary;

    #endregion


    void Start()
    {
        // These fields are set from the editor so do not modify!
        //******************************//
        AppsFlyer.setIsDebug(isDebug);
#if UNITY_WSA_10_0 && !UNITY_EDITOR
        AppsFlyer.initSDK(devKey, UWPAppID, getConversionData ? this : null);
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
    AppsFlyer.initSDK(devKey, macOSAppID, getConversionData ? this : null);
#else
        AppsFlyer.initSDK(devKey, appID, getConversionData ? this : null);
#endif
        //******************************/

        // set a custom method to handle deep link received - only on deep linking implementation
        AppsFlyer.OnDeepLinkReceived += OnDeepLink;

        AppsFlyer.startSDK();
    }


    void Update()
    {

    }

    // Mark AppsFlyer CallBacks
    public void onConversionDataSuccess(string conversionData)
    {
        AppsFlyer.AFLog("didReceiveConversionData", conversionData);
        Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
        // add deferred deeplink logic here
    }

    public void onConversionDataFail(string error)
    {
        AppsFlyer.AFLog("didReceiveConversionDataWithError", error);
    }

    public void onAppOpenAttribution(string attributionData)
    {
        AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
        Dictionary<string, object> attributionDataDictionary = AppsFlyer.CallbackStringToDictionary(attributionData);
        // add direct deeplink logic here
    }

    public void onAppOpenAttributionFailure(string error)
    {
        AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
    }

    #region Deep Link

    /** All the DeepLink handling has to be done under the onDeepLink handler **/
    void OnDeepLink(object sender, EventArgs args)
    {
        if (!(args is DeepLinkEventsArgs deepLinkEventArgs)) return;

        AppsFlyer.AFLog("DeepLink Status", deepLinkEventArgs.status.ToString());

        switch (deepLinkEventArgs.status)
        {
            case DeepLinkStatus.FOUND:

                _didReceivedDeepLink = true;

                if (deepLinkEventArgs.isDeferred())
                {
                    AppsFlyer.AFLog("OnDeepLink", "This is a deferred deep link");
                    // **only for Legacy Links users**
                    // lets the onConversionDataSuccess know we got the Deferred Deep Link and assume we can process it
                    // this can be changed later on if we got an Extended Deferred DeepLinking that can not be processed by UDL
                    // we will know the type of the DDL only on the ParseDeepLinkParams() method
                    _deferred_deep_link_processed_flag = true;
                }
                else
                {
                    AppsFlyer.AFLog("OnDeepLink", "This is a direct deep link");
                }

                var deepLinkParamsDictionary = GetDeepLinkParamsDictionary(deepLinkEventArgs);
                if (deepLinkParamsDictionary != null)
                {
                    ParseDeepLinkParams(deepLinkParamsDictionary);
                }
                break;

            case DeepLinkStatus.NOT_FOUND:
                AppsFlyer.AFLog("OnDeepLink", "Deep link not found");

                _deepLinkParamsDictionary = new Dictionary<string, object>()
                {
                    ["deep_link_not_found"] = true
                };

                break;

            default:
                AppsFlyer.AFLog("OnDeepLink", "Deep link error");

                _deepLinkParamsDictionary = new Dictionary<string, object>()
                {
                    ["deep_link_error"] = true
                };

                break;
        }
    }


    /** Get the DeepLink params depending on the device OS **/
    private Dictionary<string, object> GetDeepLinkParamsDictionary(DeepLinkEventsArgs deepLinkEventArgs)
    {
#if UNITY_IOS && !UNITY_EDITOR
    if (deepLinkEventArgs.deepLink.ContainsKey("click_event") && deepLinkEventArgs.deepLink["click_event"] != null)
    {
        return deepLinkEventArgs.deepLink["click_event"] as Dictionary<string, object>;
    }
#elif UNITY_ANDROID && !UNITY_EDITOR
    return deepLinkEventArgs.deepLink;
#endif

        return null;
    }


    /**
    Parse the DeepLink params, according to the conventions between the campaign manager and the developer. 
    Both have to agree on the meaning of each key of the Deep Link parameters.

    In this app:
    deep_link_value is the start level
    deep_link_sub1 is the quantity of the extra butterflies
    deep_link_sub2 is the extra points
    deep_link_sub3 is the referrer name if the link was generated using UserInvite

    If you don't want/can't use deep_link_value, you can add a custom param.
    the campaign manager and the developer agreed on the param 'start_level'as the deep link value param,
    instead of deep_link_value.
    **/
    private void ParseDeepLinkParams(Dictionary<string, object> deepLinkParamsDictionary)
    {
        _deepLinkParamsDictionary = new Dictionary<string, object>(); // reset the DL params from the previous DL processing

        // check if we got deep_link_value and its not null
        //if (deepLinkParamsDictionary.TryGetValue("deep_link_value", out var deepLinkValueObj) && int.TryParse(deepLinkValueObj?.ToString(), out var deepLinkValue))
        //{
        //    _startLevel = deepLinkValue;
        //    _deepLinkParamsDictionary.Add("Start Level", _startLevel);
        //}
        //// **only for Legacy Links users**
        //else
        //{
        //    // onDeepLink(UDL) cant handle Extended Deferred DeepLinking, mark to the onConversionDataSuccess to process it
        //    _deferred_deep_link_processed_flag = false;
        //}
        // check for others DeepLink params
        //if (deepLinkParamsDictionary.TryGetValue("deep_link_sub1", out var extraButterfliesBonusObj))
        //{
        //    if (int.TryParse(extraButterfliesBonusObj?.ToString(), out var extraButterfliesBonus))
        //    {
        //        _extraButterfliesBonus = extraButterfliesBonus;
        //        _deepLinkParamsDictionary.Add("Extra Butterflies", _extraButterfliesBonus);
        //    }
        //}

        if (deepLinkParamsDictionary.TryGetValue("deep_link_sub3", out var referrerNameObj))
        {
            var referrerName = referrerNameObj?.ToString();
            _deepLinkParamsDictionary.Add("Referrer Name", referrerName);
        }
    }

    #endregion

}
