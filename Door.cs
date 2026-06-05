using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour, Interactable
{
    [Header("Door Settings")]
    public Transform doorModel;
    public Collider doorCollider;

    public float openAngle = 90f;
    public float openSpeed = 3f;
    public float autoCloseDelay = 2f;

    private bool isOpen;
    private Quaternion closedRot;
    private Quaternion openRot;
    private Coroutine autoCloseRoutine;

    void Start()
    {
        closedRot = doorModel.localRotation;
        openRot = Quaternion.Euler(0, openAngle, 0) * closedRot;
    }

    public void Interact()
    {
        if (autoCloseRoutine != null)
            StopCoroutine(autoCloseRoutine);

        isOpen = !isOpen;

        if (isOpen)
        {
            doorCollider.enabled = false;
            StartCoroutine(OpenDoor());
            autoCloseRoutine = StartCoroutine(AutoClose());
        }
        else
        {
            StartCoroutine(CloseDoor());
        }
    }

    IEnumerator OpenDoor()
    {
        while (Quaternion.Angle(doorModel.localRotation, openRot) > 0.1f)
        {
            doorModel.localRotation = Quaternion.Slerp(
                doorModel.localRotation,
                openRot,
                Time.deltaTime * openSpeed
            );
            yield return null;
        }

        doorModel.localRotation = openRot;
    }

    IEnumerator CloseDoor()
    {
        while (Quaternion.Angle(doorModel.localRotation, closedRot) > 0.1f)
        {
            doorModel.localRotation = Quaternion.Slerp(
                doorModel.localRotation,
                closedRot,
                Time.deltaTime * openSpeed
            );
            yield return null;
        }

        doorModel.localRotation = closedRot;

        doorCollider.enabled = true;
        isOpen = false;
    }

    IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(autoCloseDelay);

        if (isOpen)
            StartCoroutine(CloseDoor());
    }
}
