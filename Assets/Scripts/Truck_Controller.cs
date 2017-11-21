
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private Rigidbody2D rb;
    private float wheelAngle;

    private float agitation;
    //Variables to get Medium past speed
    private int indexCurrentVelocity        = 0;
    private int indexMaxCurrVelocity        = 10;
    private float[] oldVelocityMagnitude    = new float[10];
    private static float MAX_SPEED          = 2.5f;
    private float mediumPastVeloMagn        = 0;

    [Header("AGITATION PROPERTIES")]
    private float passiveAgiReduction       = 0.01f;
    public float agiSpeedInfluence          = 0.1f;
    public float DeltaVInfluence            = 0.3f;
    //public float ndpTailleReduc = 0.6f;
    public float reducMaxNPD                = 0.2f;
    public float augmentMaxNDP              = 0.4f;



    [Header("UI PROPERTIES")]
    public Canvas canvas;

    //[HideInInspector]
    //public string ObstacleEffect;
    private bool isIn_MUD = false;
    private string MUD_TAG = "MUD";
    private bool isIn_OIL = false;
    private string OIL_TAG = "OIL";
        // Nid de poule
    private bool isIn_NDP = false;
    private string NDP_TAG = "NDP";

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
        Debug.Log("lateralDragForce :" + lateralDragForce + "/wheelForceFront"+ wheelForceFront);



        // TILES EFFECTS:
        // MUD EFFECTS
        if (isIn_MUD) rb.AddForce(-mudDrag      * rb.velocity * 2 * speed);
            // OIL EFFECTS
        if (isIn_OIL) oilDurationCooldown = oilDuration;
        if (oilDurationCooldown > 0)
        {
            oilDurationCooldown -= Time.fixedDeltaTime;
            rb.AddForce(oilUnDrag * rb.velocity * speed);
            //Debug.Log("oilDurationCooldown :" + oilDurationCooldown);
        }

        // AGITATION CALCUL
        agitation = CalculateAgitation();

        //Debug.Log("velocity magnitude :" + rb.velocity.magnitude);
        //Debug.Log("Agiation :" + agitation);

    }

    private void LateUpdate()
    {
       
        oldVelocityMagnitude[indexCurrentVelocity] = rb.velocity.magnitude;
        indexCurrentVelocity++;
        if (indexCurrentVelocity >= indexMaxCurrVelocity) indexCurrentVelocity = 0;
        float sum = 0.0f;
        foreach(float value in oldVelocityMagnitude) { sum += value; }
        mediumPastVeloMagn = sum / indexMaxCurrVelocity;
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
    }

    private float CalculateAgitation()
    {
        //Variables
            // RETURN New Value of the Agitation of the vehicule
        float newAgitation = 0;
            // Speed above which passive agitation increase
        float speedForZero = MAX_SPEED/1.6f; 
            // Velocity Magnitude of the truck
        float velocityMagnitude = rb.velocity.magnitude;

        // Passive Agitation calcul
        float speedAgitation = agiSpeedInfluence * (Mathf.Log10((velocityMagnitude+0.1f) / (speedForZero)))/100;

        // Bump/ Nids de poule Agitation calcul
        float speedNDP = 0;
        if (isIn_NDP)
        {
            float speedNDPAugment   = augmentMaxNDP * (Mathf.Max(0, MAX_SPEED - Mathf.Abs(2 * velocityMagnitude -   MAX_SPEED)));
            float speedNDPReduc     = reducMaxNPD   * (Mathf.Min(0,-MAX_SPEED + Mathf.Abs(2 * velocityMagnitude -3* MAX_SPEED)));
            speedNDP = speedNDPAugment + speedNDPReduc;
        }

        // Delta Vitesse Agiation calcul
        float speedDeltaV = 0;
        float maxDeltaV = 0.0f;
        foreach(float value in oldVelocityMagnitude)
        {
            maxDeltaV = Mathf.Max(maxDeltaV, Mathf.Abs(value - velocityMagnitude));
        }
        speedDeltaV = DeltaVInfluence *maxDeltaV / MAX_SPEED;

        // New Agitation is All the other speed
        Debug.Log("passiveAgiReduction" + passiveAgiReduction);
        newAgitation = Mathf.Max(0, agitation + speedAgitation + speedDeltaV);


        return newAgitation;
    }

}