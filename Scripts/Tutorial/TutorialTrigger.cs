using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private string[] tutorialMessages;
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && hasTriggered)
            return;

        if (other.CompareTag("Player"))
        {
            foreach (string message in tutorialMessages)
            {
                tutorialManager.QueueTutorialMessage(message);
            }

            hasTriggered = true;
        }
    }
}