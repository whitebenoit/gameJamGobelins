using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel_Controller : MonoBehaviour {

    public GameObject truckParent;
    public int index;
    private Truck_Controller truckController;
    private bool isInGFR;
    private string GFR_TAG = "GFR";
    // Use this for initialization
    void Start () {
        truckController = truckParent.GetComponent<Truck_Controller>();
        isInGFR = false;
        //truckController.isWheelInGFR[index] = false;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == GFR_TAG) truckController.isWheelInGFR[index] = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == GFR_TAG) truckController.isWheelInGFR[index] = false;
    }
}
