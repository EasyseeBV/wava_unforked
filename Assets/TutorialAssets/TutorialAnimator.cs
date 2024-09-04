using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class TutorialAnimator : MonoBehaviour
{
    public Transform ItemParent;
    public Transform IndicatorParent;
    public GameObject ItemPrefab;
    public GameObject IndicatorPrefab;
    public Button PreviousButton;
    public Button NextButton;
    public bool ChangeButtonEvents = true;

    public List<TutorialInfo> ItemData;

    [System.Serializable]
    public class TutorialInfo {
        public Sprite ItemSprite;
        public string ItemTitle;
        [TextArea(5, 10)]
        public string ItemDescription;
        public string NextButtonText;
        public UnityEvent NextButtonEvents;

        public UnityEvent ActivationEvents;

        [HideInInspector]
        public ItemContainer container;
        [HideInInspector]
        public IndicatorInfo indicator;
    }
    [System.Serializable]
    public class IndicatorInfo {
        public GameObject Indicator;
        public Image On;
        public Image Off;
    }

    private int CurrentPage = 0;


    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform item in ItemParent) {
            Destroy(item.gameObject);
        }
        foreach (Transform item in IndicatorParent) {
            Destroy(item.gameObject);
        }

        foreach (var item in ItemData) {
            GameObject newItem = Instantiate(ItemPrefab, ItemParent);
            ItemContainer newContainer = newItem.GetComponent<ItemContainer>();
            newContainer.Init(item.ItemSprite, item.ItemTitle, item.ItemDescription);
            item.container = newContainer;

            GameObject newIndicator = Instantiate(IndicatorPrefab, IndicatorParent);
            item.indicator = new IndicatorInfo { 
                Indicator = newIndicator,
                On = newIndicator.transform.Find("On").GetComponent<Image>(),
                Off = newIndicator.transform.Find("Off").GetComponent<Image>() };
        }

        LoadPage(CurrentPage);
    }

    public void NextPage() {
        if (CurrentPage >= ItemData.Count - 1)
            return;
        NextButton.interactable = false;
        PreviousButton.interactable = false;
        StartCoroutine(IENextPage());
    }

    private IEnumerator IENextPage() {
        Animator currentAni = ItemData[CurrentPage].container.GetComponent<Animator>();
        ItemData[CurrentPage].indicator.On.enabled = false;
        currentAni.Play("FadeOutLeft");
        yield return new WaitForSecondsRealtime(0.1f);
        
        CurrentPage++;
        LoadPage(CurrentPage);
        

        currentAni = ItemData[CurrentPage].container.GetComponent<Animator>();
        currentAni.Play("FadeInRight");
        yield return new WaitForSecondsRealtime(0.2f);
        NextButton.interactable = true;
        PreviousButton.interactable = true;

        yield return new WaitForSecondsRealtime(0.1f);
        //yield return new WaitUntil(() => currentAni.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
        ItemData[CurrentPage-1].container.gameObject.SetActive(false);

        
    }

    public void PreviousPage() {
        if (CurrentPage <= 0)
            return;
        NextButton.interactable = false;
        PreviousButton.interactable = false;
        StartCoroutine(IEPReviousPage());
    }

    private IEnumerator IEPReviousPage() {
        Animator currentAni = ItemData[CurrentPage].container.GetComponent<Animator>();
        ItemData[CurrentPage].indicator.On.enabled = false;
        currentAni.Play("FadeOutRight");
        yield return new WaitForSecondsRealtime(0.1f);

        CurrentPage--;
        LoadPage(CurrentPage);
        

        currentAni = ItemData[CurrentPage].container.GetComponent<Animator>();
        currentAni.Play("FadeInLeft");
        //yield return new WaitUntil(() => currentAni.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
        yield return new WaitForSecondsRealtime(0.2f);
        NextButton.interactable = true;
        PreviousButton.interactable = true;
        yield return new WaitForSecondsRealtime(0.1f);
        ItemData[CurrentPage + 1].container.gameObject.SetActive(false);

       
    }

    public void LoadPage(int pageID) {
        ItemData[pageID].container.gameObject.SetActive(true);
        ItemData[pageID].indicator.On.enabled = true;
        ItemData[pageID].ActivationEvents.Invoke();
        if (ChangeButtonEvents) {
            NextButton.onClick.RemoveAllListeners();
            NextButton.onClick.AddListener(ItemData[pageID].NextButtonEvents.Invoke);    
            NextButton.GetComponentInChildren<TextMeshProUGUI>().text = ItemData[pageID].NextButtonText;
        }
    }
}
