
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Truck_Controller : MonoBehaviour
{
    // INTRINSIC PROPERTIES
    [Header("INTRINSIC PROPERTIES")]
    public float speed = 50;
    public float carSize = 20;
    // Lateral Drag
    public float nonDirecSpeedReduc = 0.20f;
    // Maximum Angle (both +/-) that the front wheel can have regarding forward direction
    [Range(0.0f, 90.0f)]
    public float wheelMaxAngle = 30;
    // Maximum Angular Velocity of the truck
    public float maxAngularVelocity = 20;
    // Name of wheels
    private GameObject wheel_TL;
    private GameObject wheel_TR;
    private GameObject wheel_BL;
    private GameObject wheel_BR;
    private GameObject[] wheels;
    [HideInInspector]
    public bool[] isWheelInGFR;

    private Rigidbody2D rb;
    private float wheelAngle;

    private float agitation;
    //Variables to get Medium past speed
    private int indexCurrentVelocity        = 0;
    private int indexMaxCurrVelocity        = 10;
    private float[] oldVelocityMagnitude    = new float[10];
    private static float MAX_SPEED          = 2.5f;
    private float passiveAgiReduction       = 0.01f;
    private float agiSpeedInfluence         = 0.1f;
    private float DeltaVInfluence           = 0.05f;

    [Header("AGITATION PROPERTIES")]
    public float maxAgitation               = 1.0f;
    public float reducMaxNPD                = 0.02f;
    public float augmentMaxNDP              = 0.01f;



    [Header("UI PROPERTIES")]
    public Canvas canvas;
    public UnityEngine.UI.Image agitationBar;
    public UnityEngine.UI.Image speedBar;
    private float speedBarStratingAngle;
    private float speedBarAngleAmplitude = 110.0f;
    


    //[HideInInspector]
    //public string ObstacleEffect;
    private bool isIn_MUD = false;
    private string MUD_TAG = "MUD";
    private bool isIn_OIL = false;
    private string OIL_TAG = "OIL";
        // Nid de poule
    private bool isIn_NDP = false;
    private string NDP_TAG = "NDP";
    private bool isIn_GFR = false;
    private string GFR_TAG = "GFR";

    [Header("OBSTACLES PROPERTIES")]
    [Range(0.0f, 2.0f)]
    public float mudDrag = 0.5f;
    [Range(0.0f, 2.0f)]
    public float oilUnDrag = 0.5f;
    [Range(0.0f, 5.0f)]
    public float oilDuration = 3;
    private float oilDurationCooldown = 0;




    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        wheelAngle = 0.0f;
        speedBarStratingAngle = speedBar.transform.eulerAngles.z;

        wheel_TL = rb.transform.Find("Wheel_TL").parent.gameObject;
        wheel_TR = rb.transform.Find("Wheel_TR").parent.gameObject;
        wheel_BL = rb.transform.Find("Wheel_BL").parent.gameObject;
        wheel_BR = rb.transform.Find("Wheel_BR").parent.gameObject;
        wheels = new GameObject[4] { wheel_TL, wheel_TR, wheel_BL, wheel_BR };
        isWheelInGFR = new bool[4];
    }

    private void FixedUpdate()
    {
        // Getting the inputs of the player
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // TRUCK MOVING
            // Making the "wheels" turn
        wheelAngle = wheelMaxAngle * moveHorizontal;
            // Calculating the forces
        float forwardForceAmount = moveVertical * speed;
        Vector2 wheelForceFront = forwardForceAmount * (Mathf.Cos(wheelAngle * Mathf.Deg2Rad) * transform.up + Mathf.Sin(wheelAngle * Mathf.Deg2Rad) * transform.right);
        Vector2 wheelForceBack = forwardForceAmount * transform.up;
            // Applaying the forces
        rb.AddForceAtPosition(wheelForceFront, carSize / 2 * transform.up + transform.position);
        rb.AddForceAtPosition(wheelForceBack, -carSize / 2 * transform.up + transform.position);

            //Cap the Angular Velocity
        rb.angularVelocity = Mathf.Sign(rb.angularVelocity) * Mathf.Min(maxAngularVelocity, Mathf.Abs(rb.angularVelocity));

            // Reducing non forward velocity
                // FORCES METHOD
        float normeVelo = rb.velocity.magnitude;
        float scalRight = Vector2.Dot(transform.right, rb.velocity.normalized);
        Vector2 lateralDragForce = - nonDirecSpeedReduc * Mathf.Abs(normeVelo) * scalRight * transform.right * rb.mass;
        rb.AddForce(lateralDragForce);



        // TILES EFFECTS:
            // MUD EFFECTS
        if (isIn_MUD) rb.AddForce(-mudDrag * rb.velocity * 2 * rb.mass);
            // OIL EFFECTS
        if (isIn_OIL) oilDurationCooldown = oilDuration;
        if (oilDurationCooldown > 0)
        {
            oilDurationCooldown -= Time.fixedDeltaTime;
            rb.AddForce(oilUnDrag * rb.velocity * rb.mass);
        }

        // GOUFFRE EFFECTS
        int wheelActuallyInGouffre = 0;
        foreach(bool isWheeIn in isWheelInGFR)
        {
            if (isWheeIn) wheelActuallyInGouffre++;
        }
        if (wheelActuallyInGouffre >= 2)
        {
            TruckFallInGouffre();
        }


        // AGITATION CALCUL
        agitation = CalculateAgitation();
        if(agitation > maxAgitation)
        {
            ExplosionNitro();
        }

        //Debug.Log("velocity magnitude :" + rb.velocity.magnitude);
        //Debug.Log("Agiation :" + agitation);


        // UI Function:
        UpdateUI();


    }


    private void LateUpdate()
    {
        oldVelocityMagnitude[indexCurrentVelocity] = rb.velocity.magnitude;
        indexCurrentVelocity++;
        if (indexCurrentVelocity >= indexMaxCurrVelocity) indexCurrentVelocity = 0;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!string.IsNullOrEmpty(collision.gameObject.tag)) updatesIsInTiles(collision.gameObject, true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!string.IsNullOrEmpty(collision.gameObject.tag)) updatesIsInTiles(collision.gameObject, false);
    }

    void updatesIsInTiles(GameObject go, bool isIn)
    {
        string goTag = go.tag;
        if (goTag == MUD_TAG) isIn_MUD = isIn;
        if (goTag == OIL_TAG) isIn_OIL = isIn;
        if (goTag == NDP_TAG) isIn_NDP = isIn;
        if (goTag == GFR_TAG) isIn_GFR = isIn;
    }

    private float CalculateAgitation()
    {
        //Variables
            // RETURN New Value of the Agitation of the vehicule
        float newAgitation = 0;
            // Speed above which passive agitation increase
        float speedForZero = MAX_SPEED/1.15f; 
            // Velocity Magnitude of the truck
        float velocityMagnitude = rb.velocity.magnitude;

        // Passive Agitation calcul
        float speedAgitation = agiSpeedInfluence * (Mathf.Log10((velocityMagnitude+0.1f) / (speedForZero)))/6;

        // Bump/ Nids de poule Agitation calcul
        float speedNDP = 0;
        if (isIn_NDP)
        {
            float speedNDPAugment   = augmentMaxNDP * (Mathf.Max(0, MAX_SPEED - Mathf.Abs(2 * velocityMagnitude -   MAX_SPEED)));
            float speedNDPReduc     = reducMaxNPD   * (Mathf.Min(0,-MAX_SPEED + Mathf.Abs(2 * velocityMagnitude -3* MAX_SPEED)));
            speedNDP = (speedNDPAugment + speedNDPReduc)/50;
        }

        // Delta Vitesse Agiation calcul
        float speedDeltaV = 0;
        float maxDeltaV = 0.0f;
        foreach(float value in oldVelocityMagnitude)
        {
            maxDeltaV = Mathf.Max(maxDeltaV, Mathf.Abs(Mathf.Pow(1+value - velocityMagnitude,4))-1);
        }
        speedDeltaV = DeltaVInfluence *maxDeltaV / Mathf.Pow(MAX_SPEED,4);
        //Debug.Log("speedDeltaV :" + speedDeltaV + "/ speedAgitation:"+ speedAgitation);

        // New Agitation is All the other speed
        newAgitation = Mathf.Max(0, agitation + (speedAgitation + speedDeltaV + speedNDP)/1.2f);


        return newAgitation;
    }

    private void ExplosionNitro()
    {
        
    }

    private void TruckFallInGouffre()
    {
    }

    private void UpdateUI()
    {
        //agitationBar = canvas.GetComponentInChildren<UnityEngine.UI.Image>();
        float ratioAgitation = agitation / maxAgitation;
        agitationBar.fillAmount = ratioAgitation;
        agitationBar.color = new Color(255, (int)(255 *(1 - ratioAgitation)), (int)(255 * (1 - ratioAgitation))) ;

        float ratioSpeed = rb.velocity.magnitude / (3 * MAX_SPEED);
        Vector3 speedBarEulerAngles = speedBar.rectTransform.eulerAngles;
        speedBar.rectTransform.eulerAngles.Set(
            speedBarEulerAngles.x,
            speedBarEulerAngles.y,
            speedBarStratingAngle - ratioSpeed*speedBarAngleAmplitude);

        //Debug.Log("ratioSpeed :" + ratioSpeed.ToString() + "/speedBarAngle -:" + (speedBarStratingAngle - ratioSpeed * speedBarAngleAmplitude).ToString());
    }
}