using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.UI;


public class GoogleAdsBanner : MonoBehaviour
{
    private string _appId;
    private string _adUnitId;
    private BannerView _bannerView;
    private float _bannerViewHeight = 100;
    private float _bannerViewWidth = 650;
    #if UNITY_ANDROID && !UNITY_EDITOR
    private string _androidTestAppId = "ca-app-pub-3940256099942544~3347511713";
    private string _androidTestAdUnitId = "ca-app-pub-3940256099942544/6300978111";
    #endif
    #if UNITY_IOS && !UNITY_EDITOR
    private string _iOSTestAppId = "ca-app-pub-3940256099942544~1458002511";
    private string _iOSTestAdUnitId = "ca-app-pub-3940256099942544/2934735716";
    #endif
    
    public string androidAppId;
    public string androidUnitId;
    public string iOSAppId;
    public string iOSUnitId;
    public bool useTestIds = true;
    public AdPosition adPlacement =AdPosition.Top;
    public AdSize adType = AdSize.Banner;
    public bool useGhostAd;
    public RectTransform ghostAdRectTransform;
    public GameObject noNetworkText;
    public Text infoText;

    private void OnValidate()
    {
        if (useGhostAd && ghostAdRectTransform != null)
            LoadGhostAd();
        if(useGhostAd == false)
            ghostAdRectTransform.gameObject.SetActive(false);
    }

    public void Start()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
            if (androidAppId != null && useTestIds == false)
                _appId = androidAppId;
            if (useTestIds)
                _appId = _androidTestAppId;
        #elif UNITY_IOS && !UNITY_EDITOR
            if (iOSAppId != null && useTestIds == false)
                _appId = iOSAppId;
            if (useTestIds)
                _appId = _iOSTestAppId;
        #else
            UpdateDisplayText("Looks like you are running this in the Editor. The ads will only show up on the actual device");    
            _appId = "unexpected_platform";

            if (noNetworkText != null && useGhostAd && ghostAdRectTransform != null)
            {
                noNetworkText.gameObject.SetActive(true);
                noNetworkText.GetComponent<Text>().text = "Your ad will display here";
                LoadGhostAd();
            }
        #endif
        
        if(_appId == null)
            UpdateDisplayText("Looks like you forgot to assign the App Id");
        MobileAds.Initialize(_appId);
        #if UNITY_IOS && !UNITY_EDITOR
            RequestBanner();
        #endif
    }

    private void RequestBanner()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
            if (androidUnitId != null && useTestIds == false)
                _adUnitId = androidUnitId;
            if (useTestIds)
                _adUnitId = _androidTestAdUnitId;

        #elif UNITY_IOS && !UNITY_EDITOR
            if(iOSUnitId != null && useTestIds == false)
                _adUnitId = iOSUnitId;
            if(useTestIds)
                _adUnitId = _iOSTestAdUnitId;
        #else
            _adUnitId = "unexpected_platform";
        #endif

        _bannerView = new BannerView(_adUnitId, adType, adPlacement);
        #if !UNITY_EDITOR
            _bannerViewHeight = _bannerView.GetHeightInPixels();
            _bannerViewWidth = _bannerView.GetWidthInPixels();
        #endif
        if (useGhostAd && ghostAdRectTransform != null)
            LoadGhostAd();
        _bannerView.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        _bannerView.OnAdLoaded += HandleOnAdLoaded;
        var request = new AdRequest.Builder().Build();
        _bannerView.LoadAd(request);
       
    }

    private void HandleOnAdLoaded(object sender, EventArgs args)
    {
        ghostAdRectTransform.gameObject.SetActive(false);
        noNetworkText.SetActive(false);
        UpdateDisplayText("Ad loaded");
    }

    private void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        UpdateDisplayText("Failed to load the ad: "+args.Message);
        if(noNetworkText != null)
            noNetworkText.SetActive(true);
    }

    private void OnApplicationQuit()
    {
        _bannerView?.Destroy();
    }
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) return;
        _bannerView?.Destroy();
        UpdateDisplayText("Refreshing the ad");
        RequestBanner();
    }
    private void LoadGhostAd()
    {
        switch (adPlacement)
        {
            case AdPosition.Top:
                AdjustLayoutForGhost(new Vector2((float) 0.5,1), new Vector2((float) 0.5, 1), new Vector2((float) 0.5,1), new Vector3(0,0,0));
                break;
            case AdPosition.TopLeft:
                AdjustLayoutForGhost(new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector3(0,0,0));
                break;
            case AdPosition.TopRight:
                AdjustLayoutForGhost(new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector3(0,0,0));
                break;
            case AdPosition.Bottom:
                AdjustLayoutForGhost(new Vector2((float) 0.5,0), new Vector2((float) 0.5, 0), new Vector2((float) 0.5,0), new Vector3(0,0,0));
                break;
            case AdPosition.BottomLeft:
                AdjustLayoutForGhost(new Vector2(0,0), new Vector2(0, 0), new Vector2(0,0), new Vector3(0,0,0));
                break;
            case AdPosition.BottomRight:
                AdjustLayoutForGhost(new Vector2(1,0), new Vector2(1, 0), new Vector2(1,0), new Vector3(0,0,0));
                break;
            case AdPosition.Center:
                AdjustLayoutForGhost(new Vector2((float) 0.5,(float) 0.5), new Vector2((float) 0.5, (float) 0.5), new Vector2((float) 0.5,(float) 0.5), new Vector3(0,0,0));
                break;
            default:
             AdjustLayoutForGhost(new Vector2((float) 0.5,1), new Vector2((float) 0.5, 1), new Vector2((float) 0.5,1), new Vector3(0,0,0));
             break;
        }
    }

    private void AdjustLayoutForGhost(Vector2 updateAnchorMin, Vector2 updateAnchorMax, Vector2 updatePivot, Vector3 updatePosition)
    {
        var testRec = ghostAdRectTransform;
        testRec.anchorMin = updateAnchorMin;
        testRec.anchorMax = updateAnchorMax;
        testRec.pivot = updatePivot;
        testRec.anchoredPosition = updatePosition;
        testRec.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _bannerViewWidth);
        testRec.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,_bannerViewHeight);
        testRec.ForceUpdateRectTransforms();
        ghostAdRectTransform.gameObject.SetActive(true);
    }

    private void UpdateDisplayText(string updateText)
    {
        if (infoText == null)
            return;
        infoText.text = updateText;
    }
}
