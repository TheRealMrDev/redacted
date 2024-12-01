using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CameraInstruction
{
    public string message;
    public string inputHint;
}

public class CameraTutorial : MonoBehaviour
{
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private GameObject triggerObject;
    [SerializeField] private CameraInstruction[] cameraInstructions = new CameraInstruction[]
    {
        new CameraInstruction { message = "Left Click to Take Photo", inputHint = "Left Mouse" },
        new CameraInstruction { message = "Right Click to Zoom", inputHint = "Right Mouse" },
        new CameraInstruction { message = "Press E to Put Down Camera", inputHint = "E Key" }
    };

    private void Start()
    {
        if (triggerObject != null)
        {
            StartCoroutine(WatchTriggerObject());
        }
    }

    private IEnumerator WatchTriggerObject()
    {
        while (true)
        {
            yield return new WaitUntil(() => !triggerObject.activeSelf);
            ShowCameraTutorial();
            yield return new WaitUntil(() => triggerObject.activeSelf);
        }
    }

    public void ShowCameraTutorial()
    {
        foreach (CameraInstruction instruction in cameraInstructions)
        {
            string fullMessage = instruction.inputHint != null 
                ? $"{instruction.message} ({instruction.inputHint})" 
                : instruction.message;
            
            tutorialManager.QueueTutorialMessage(fullMessage);
        }
    }
}