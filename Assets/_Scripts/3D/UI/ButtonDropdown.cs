using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class ButtonDropdown : MonoBehaviour
{
    [SerializeField] PointPlacement3D PointManager;
    [SerializeField] Button Button;
    [SerializeField] GameObject ItemPrefab;
    [SerializeField] int ItemCount = 2;

    List<GameObject> ItemObjs = new List<GameObject>();
    bool Extended = false;

    

    private void Start()
    {
        for (int i = 0; i < ItemCount; i++)
        {
            //creating and setting object. use 2 prefabs and switch between if its bool, no idea how to check
            GameObject itemObj = Instantiate(ItemPrefab, transform);

            Slider valueSlider = itemObj.GetComponentInChildren<Slider>();
            valueSlider.minValue = 0.01f;
            valueSlider.maxValue = 100f;
            if(i == ItemCount - 1)
            {
                valueSlider.maxValue = 0.3f;
            }
            valueSlider.value = valueSlider.minValue;

            // If this variable should be int
            bool isInt = i == 0 || i == 1 || i == 4; // PX, PZ, PointDensity
            if (isInt) valueSlider.wholeNumbers = true;

            itemObj.GetComponentInChildren<TMP_Text>().text = PointManager.GenParam.VariableNames[i];

            //positioning
            RectTransform trans = itemObj.GetComponent<RectTransform>();
            Vector2 anchoredOffset = new Vector2(0f, ((i+1)*trans.rect.height) + (trans.rect.height/2f));
            trans.anchoredPosition -= anchoredOffset;

            VarLink link = PointManager.GenParam.Variables[i];
            valueSlider.onValueChanged.AddListener(val => link.Set(val));

            ItemObjs.Add(itemObj);
        }
    }

    private void Update()
    {
        for(int i = 0; i < ItemCount; ++i)
        {
            Slider slider = ItemObjs[i].GetComponentInChildren<Slider>();
            VarLink link = PointManager.GenParam.Variables[i];
            TMP_InputField readField = ItemObjs[i].GetComponentInChildren<TMP_InputField>();
            readField.text = slider.value.ToString("F2");
            slider.onValueChanged.AddListener(val => link.Set(val));
            
        }
    }


    public void ChangeState()
    {
        Extended = !Extended;
        if(Extended)
        {
            //make extra UI visible.
        }
    }
}
