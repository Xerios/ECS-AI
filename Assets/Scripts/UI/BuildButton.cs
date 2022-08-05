using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BuildButton : MonoBehaviour
{
    public GameObject buildPrefab;
    public Action BuildAction;

    public TMPro.TextMeshProUGUI titleText, metaText;

    private Button button;
    private GameManager GameManager;
    private FloatingPopupMeta meta;
    private ResourceCost resource;

    // Start is called before the first frame update
    void Start ()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);

        GameManager = FindObjectOfType<GameManager>();

        meta = buildPrefab.GetComponent<FloatingPopupMeta>();

        Debug.Assert(meta != null, $"Prefab {buildPrefab} doesn't have FloatingPopupMeta");

        titleText.text = meta.Label;
        metaText.text = "";

        resource = buildPrefab.GetComponent<ResourceCost>();

        if (resource != null) {
            if (resource.Cost.Wood != 0) metaText.text += $"Wood: <indent=60%>{resource.Cost.Wood}</indent>\n";
            if (resource.Cost.Iron != 0) metaText.text += $"Iron: <indent=60%>{resource.Cost.Iron}</indent>\n";
            if (resource.Cost.Faith != 0) metaText.text += $"Faith: <indent=60%>{resource.Cost.Faith}</indent>\n";
        }else{
            Debug.LogWarning($"Prefab {buildPrefab} doesn't have ResourceCost");
        }
    }

    private void OnClick ()
    {
        // if (GameResources.Current.CanSubstract(resource.Cost)) {
        GameManager.buildCheck = CanBuild;
        GameManager.buildConfirm = Build;
        GameManager.buildPrefab = buildPrefab;
        GameManager.OnBuildClick();
        // GameResources.Current.Substract(resource.Cost);
        // }
    }

    public bool CanBuild (Vector3 pos)
    {
        Debug.Assert(resource != null, $"Prefab {buildPrefab} doesn't have ResourceCost");
        if (!GameResources.Current.CanSubstract(resource.Cost)) return false;

        return !Physics.CheckCapsule(pos + Vector3.up * 10f, pos, 1f, RaycastManager.Instance.layerMaskBuildings);
    }

    public void Build ()
    {
        GameResources.Current.Substract(resource.Cost);
    }

    // Update is called once per frame
    void Update ()
    {
        // button.interactable =
    }
}