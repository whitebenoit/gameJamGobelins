using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Controller : MonoBehaviour {

    public GameObject truckPlayer;
    public float xOffset = 0;
    public float yOffset = 0;
    public float zOffset = -10;

    private Transform truckTransform;
    private Vector3 offset;

    // Use this for initialization
    void Start()
    {
        offset = new Vector3(xOffset, yOffset, zOffset);
        truckTransform = truckPlayer.GetComponent<Rigidbody2D>().transform;
    }

    // LateUpdate is called after Update each frame
    void LateUpdate()
    {
        transform.position = truckTransform.position + offset;
    }
}
