using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurpleTruck_Controller : MonoBehaviour {

    public float speed = 20;
    public float carSize = 20;
    public float nonDirecSpeedReduc = 0.20f;
    // Maximum Angle (both +/-) that the front wheel can have regarding forward direction
    public float wheelMaxAngle = 45;


    public float maxAngularVelocity = 20;

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
        Debug.Log("wheelAngle = " + wheelAngle);


        // Calculating the forces
        float forwardForceAmount = moveVertical * speed;
        Vector2 wheelForceFront = forwardForceAmount * (Mathf.Cos(wheelAngle * Mathf.Deg2Rad) * transform.up + Mathf.Sin(wheelAngle * Mathf.Deg2Rad) * transform.right);
        Vector2 wheelForceBack = forwardForceAmount * transform.up;
        // Applaying the forces
        rb.AddForceAtPosition(wheelForceFront, carSize / 2 * transform.up + transform.position);
        rb.AddForceAtPosition(wheelForceBack, -carSize / 2 * transform.up + transform.position);

        rb.angularVelocity = Mathf.Sign(rb.angularVelocity) * Mathf.Min(maxAngularVelocity, Mathf.Abs(rb.angularVelocity));
 
        // Reducing non forward velocity

        // VELOCITY METHOD 
        //float normeVelo = rb.velocity.magnitude;
        //float scalUp = Vector2.Dot(transform.up, rb.velocity.normalized);
        //float scalRight = Vector2.Dot(transform.right, rb.velocity.normalized);

        //Debug.Log("Magnitude " + rb.velocity.magnitude);
        //Debug.Log("Normalized " + rb.velocity.normalized);
        //Debug.Log("Scal " + scalUp);

        //rb.velocity = normeVelo * scalUp * transform.up + normeVelo * scalRight * 0.99f * transform.right;


        // FORCES METHOD
        float normeVelo = rb.velocity.magnitude;
        float scalRight = Vector2.Dot(transform.right, rb.velocity.normalized);
        Vector2 lateralDragForce = -5 * Mathf.Abs(normeVelo) * scalRight * transform.right;
        rb.AddForce(lateralDragForce);


    }
}
