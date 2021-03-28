using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct RayData
{
    public int Direction;
    public Vector2 Offset;
    public float Weight;
    public float Length;
}
public class CameraController : MonoBehaviour
{
    
    public Transform target;
    public float lerpSpeed;
    public int NumRayCasts;
    public LayerMask layerMask;
    public float rayLength;
    public float offSetRadius;
    public RayData[] rays;

    int threeSixtyOverNumRayCasts;

    public Vector2 offSet;
    public float maxOffSetWidth;
    public float maxOffSetHeight;

    public float environmentCheckStrength;
    public float environmentCheckInterval;
    float currentCheckTime;
    // Start is called before the first frame update
    void Start()
    {
        threeSixtyOverNumRayCasts = 360 / NumRayCasts;
    }

    // Update is called once per frame
    void Update()
    {
        currentCheckTime += Time.deltaTime;
        CheckEnvironment();
        UpdatePosition();
    }

    private void FixedUpdate()
    {
        
        //CheckEnvironment_2();
        
    }

    //******************************* IMPORTANT: WEIGHTS MUST BE IMPLEMENTED *************************************
    void CheckEnvironment()
    {
        Vector2 averagePos = Vector2.zero;
        int numHits = 0;
        for(int i = 0; i < rays.Length; i++)
        {
            Vector2 dir = (Vector2)(Quaternion.Euler(0, 0, rays[i].Direction) * Vector2.right);
            //dir = Vector2.up;
            float moddedRayLength = rays[i].Length;
            if (rays[i].Direction < 360 && rays[i].Direction > 180)
            {
                moddedRayLength *= 2;
            }
                
            RaycastHit2D hit = Physics2D.Raycast(target.position, dir, rays[i].Length, layerMask);

            if (hit.collider != null)
            {
                averagePos += (hit.point - ((Vector2)target.position + dir * rays[i].Length));
                numHits++;
                Debug.DrawLine(target.position, hit.point, Color.green);
                //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * distance, );
            }
            else
            {
                Debug.DrawLine(target.position, (Vector2)target.position + dir * rays[i].Length, Color.red);
            }
        }

        if(averagePos != Vector2.zero)
        {
            //averagePos /= numHits;
            
        }
        offSet = averagePos;

        //offSet = offSet * environmentCheckStrength;
        offSet.x = Mathf.Clamp(offSet.x, -maxOffSetWidth * .5f, maxOffSetWidth * .5f);
        offSet.y = Mathf.Clamp(offSet.y, -maxOffSetHeight * .5f, maxOffSetHeight * .5f);

        
    }

    void UpdatePosition()
    {
        Vector2 newPos = Vector2.MoveTowards(transform.position, (Vector2)target.position + offSet, lerpSpeed);
        //newPos = Vector2.MoveTowards(transform.position, (Vector2)target.position, lerpSpeed);
        transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
    }
    void CheckEnvironment_2()
    {
        if(currentCheckTime > environmentCheckInterval)
        {
            Vector2 averagePos = Vector2.zero;
            int numHits = 0;
            for (int i = 0; i < NumRayCasts; i++)
            {
                Vector2 dir = (Vector2)(Quaternion.Euler(0, 0, i * threeSixtyOverNumRayCasts) * Vector2.right);
                //dir = Vector2.up;
                float moddedRayLength = rayLength;
                if (i * threeSixtyOverNumRayCasts < 360 && i * threeSixtyOverNumRayCasts > 180)
                {
                    //moddedRayLength *= 2;
                }

                RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, moddedRayLength, layerMask);

                if (hit.collider != null)
                {
                    averagePos += (hit.point - ((Vector2)transform.position + dir * rayLength));
                    numHits++;
                    Debug.DrawLine(transform.position, hit.point, Color.green);
                    //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * distance, );
                }
                else
                {
                    Debug.DrawLine(transform.position, (Vector2)transform.position + dir * rayLength, Color.red);
                }
            }

            if (averagePos != Vector2.zero)
            {
                //averagePos /= numHits;
                offSet = averagePos;

                //offSet = Vector2.ClampMagnitude(offSet, offSetRadius);
            }
            Debug.DrawLine(transform.position, (Vector2)transform.position + offSet, Color.blue);

            

            currentCheckTime = 0;
        }
        
    }
}
