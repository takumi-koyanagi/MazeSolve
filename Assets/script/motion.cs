using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class motion : MonoBehaviour
{
    const int R = 1;
    const int L = 2;
    const int U = 3;
    const int D = 4;

    Transform myTransform;

    float dis;
    const float DIS = 10.0f;
    const float delta = 0.4f;
    Vector3 start = new Vector3(0, 0, 0);
    Vector3 target = new Vector3(0, 0, 0);
    const float ERROR = 99999999.0f;

    public bool animeEnd = true;
    public bool go = false;
    public bool MissingCollision = false;
    public int dir = 0;

    void Start()
    {
        myTransform = this.transform;
    }

    void Update()
    {
        if (animeEnd && go)
        {
            start = myTransform.position;
            target = new Vector3(0, 0, 0);
            switch (dir)
            {
                case R:
                    target.x = delta;
                    break;
                case L:
                    target.x = -delta;
                    break;
                case U:
                    target.z = delta;
                    break;
                case D:
                    target.z = -delta;
                    break;
                default:
                    break;
            }
            animeEnd = false;
            MissingCollision = false;
            dis = 0.0f;
        }
        else if(go)
        {
            if (dis >= DIS) 
            {
                go = false;
                animeEnd = true;
                Vector3 Ftarget = myTransform.position;
                Ftarget.x = Mathf.Floor(Ftarget.x + 0.5f);
                Ftarget.y = Mathf.Floor(Ftarget.y + 0.5f);
                Ftarget.z = Mathf.Floor(Ftarget.z + 0.5f);
                myTransform.position = Ftarget;
            }
            else
            {
                myTransform.Translate(target, Space.World);
                dis += delta;
            }
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "wall")
        {
            myTransform.position = start;
            dis = ERROR;
            MissingCollision = true;
        }
    }
}
