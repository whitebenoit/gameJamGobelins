
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

    //[HideInInspector]
    //public string ObstacleEffect;
    private bool isIn_MUD = false;
    private string MUD_TAG = "MUD";
    private bool isIn_OIL = false;
    private string OIL_TAG = "OIL";

    [Header("EFFECT PROPERTIES")]
    [Range(0.0f, 2.0f)]
    public float mudDrag = 0.5f;
    [Range(0.0f, 2.0f)]
    public float oilUnDrag = 0.5f;
    [Range(0.0f, 5.0f)]
    public float oilDuration = 3;
    private float oilDurationCooldown = 0;



    private Rigidbody2D rb;
    private float wheelAngle;

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
        Vector2 lateralDragForce = - nonDirecSpeedReduc * Mathf.Abs(normeVelo) * scalRight * transform.right;
        rb.AddForce(lateralDragForce);



        // Applaying the effects:
            // MUD EFFECTS
        if (isIn_MUD) rb.AddForce(-mudDrag      * rb.velocity * 2 * speed);
            // OIL EFFECTS
        if (isIn_OIL) oilDurationCooldown = oilDuration;
        if (oilDurationCooldown > 0)
        {
            oilDurationCooldown -= Time.fixedDeltaTime;
            rb.AddForce(oilUnDrag * rb.velocity * speed);
            Debug.Log("oilDurationCooldown :" + oilDurationCooldown);
        }
        //Debug.Log(rb.velocity +":" + rb.velocity.magnitude);
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!string.IsNullOrEmpty(collision.gameObject.tag)) TilesEffect(collision.gameObject, true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!string.IsNullOrEmpty(collision.gameObject.tag)) TilesEffect(collision.gameObject, false);
    }

    void TilesEffect(GameObject go, bool isIn)
    {
        string goTag = go.tag;
        if (goTag == MUD_TAG) isIn_MUD = isIn;
        if (goTag == OIL_TAG) isIn_OIL = isIn;
    }

}