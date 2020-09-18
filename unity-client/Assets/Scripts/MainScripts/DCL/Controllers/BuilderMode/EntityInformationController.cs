using DCL;
using DCL.Controllers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EntityInformationController : MonoBehaviour
{
    [Header("Scene references")]

    public TextMeshProUGUI titleTxt;
    public TextMeshProUGUI descTxt;

    DecentralandEntity currentEntity;
    ParcelScene parcelScene;

    bool isEnable = false;

    int framesBetweenUpdate = 5;
    int framesCount = 0;

    private void LateUpdate()
    {
        if(isEnable)
        {
            if (currentEntity != null)
            {
                if (framesCount >= framesBetweenUpdate)
                {
                    UpdateInfo();
                    framesCount = 0;
                }
                else framesCount++;
            }
        }
    }
    public void SetEntity(DecentralandEntity _entity,ParcelScene currentScene)
    {
        currentEntity = _entity;
        parcelScene = currentScene;
        titleTxt.text = currentEntity.entityId;
        UpdateInfo();
    }
    public void Enable()
    {
        gameObject.SetActive(true);
        isEnable = true;
    }

    public void Disable()
    {
        gameObject.SetActive(false);
        isEnable = false;
    }


    public void UpdateInfo()
    {

        Vector3 positionConverted = SceneController.i.ConvertUnityToScenePosition(currentEntity.gameObject.transform.position, parcelScene);
        Vector3 currentRotation = currentEntity.gameObject.transform.rotation.eulerAngles;
        Vector3 currentScale = currentEntity.gameObject.transform.localScale;
        //string desc = "POSITION:   X: " + positionConverted.x + "  Y: " + positionConverted.y + "  Z:"+ positionConverted.z;
        //desc +=   "\n\nROTATION:   X: " + currentRotation.x +   "  Y: " + currentRotation.y +   "  Z:" + currentRotation.z;
        //desc +=   "\n\nSCALE:      X: " + currentScale.x +      "  Y: " + currentScale.y +      "  Z:" + currentScale.z;

        string desc = AppendUsageAndLimit("POSITION:   ", positionConverted, "0.#");
        desc += "\n\n"+AppendUsageAndLimit("ROTATION:  ", currentRotation, "0");
        desc += "\n\n"+ AppendUsageAndLimit("SCALE:        ", currentScale, "0.##");
         
        descTxt.text = desc;

    }


    string AppendUsageAndLimit(string name, Vector3 currentVector,string format)
    {
        return name + "X: " + currentVector.x.ToString(format) + "  Y: " + currentVector.y.ToString(format) + "  Z:" + currentVector.z.ToString(format);
    }
}
