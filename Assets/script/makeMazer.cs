using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class makeMazer : MonoBehaviour
{
    Vector3[][] CoodinateSet;
    string[][] StatusSetIJCoodinate;
    public string START, GOAL;
    public Material[] ColorSet = new Material[2];

    public string[] StatusSet;
    public GameObject wall;
    public GameObject floor;
    public int mazeLength = 5;
    public int wallNum    = 5 - 1;
    public bool annoyance = true;
    public bool verticalTraverse = true; //経路が縦に横断するか

    int numberOfStates;
    // Start is called before the first frame update
    void Start()
    {
        CoodinateSet = new Vector3[mazeLength][];
        for (int i = 0; i < mazeLength; i++) CoodinateSet[i] = new Vector3[mazeLength];
        StatusSetIJCoodinate = new string[mazeLength][];
        for (int i = 0; i < mazeLength; i++) StatusSetIJCoodinate[i] = new string[mazeLength]; 
        StatusSet = new string[mazeLength * mazeLength];

        setCoodinate();
        setStartAndGoal();
        setWall();
        setOutWall();

        int count = 0;
        for(int i=0; i < mazeLength; i++)
        {
            for (int j = 0; j < mazeLength; j++)
            {
                if (StatusSetIJCoodinate[j][i] != "wall")
                {
                    StatusSet[count] = StatusSetIJCoodinate[j][i];
                    count += 1;
                }
            }
        }
        StatusSet[count] = "END";
        
        this.GetComponent<main>().enabled = true;
        this.GetComponent<motion>().enabled = true;
    }

    // Update is called once per frame
    void setCoodinate()
    {
        Vector3 Initial = new Vector3(0, 0, 0);
        Initial.x = -(mazeLength / 2) * 10;
        Initial.z = (mazeLength / 2) * 10;
        if (mazeLength - (mazeLength / 2) * 2 == 0)
        {
            Initial.x += 5.0f;
            Initial.z -= 5.0f;
        }
        string tag = "situations";
        numberOfStates = 0;

        //start,goal
        System.Security.Cryptography.RNGCryptoServiceProvider r =
            new System.Security.Cryptography.RNGCryptoServiceProvider();

        int iR = 0;
        int jR = 0;

        byte[] rndary = new byte[10];
        r.GetBytes(rndary);
        iR += rndary[0] + rndary[2] + rndary[4];
        jR += rndary[1] + rndary[3] + rndary[5];
        iR = iR % mazeLength;
        jR = jR % mazeLength;

        Debug.Log("Seting coodinate");
        for (int i = 0; i < mazeLength; i++)
        {
            for (int j = 0; j < mazeLength; j++)
            {
                CoodinateSet[i][j] = new Vector3(0, 0, 0);
                CoodinateSet[i][j].x = Initial.x + i * 10;
                CoodinateSet[i][j].z = Initial.z - j * 10;
                GenerateFloorAndSetTags(CoodinateSet[i][j], tag + numberOfStates);
                StatusSetIJCoodinate[i][j] = tag + numberOfStates;
                numberOfStates += 1;
            }
        }

        int c = rndary[6] + rndary[7];
        if (c % 4 == 3)
        {
            START = StatusSetIJCoodinate[iR][0];
            GOAL  = StatusSetIJCoodinate[jR][mazeLength - 1];
            StatusSetIJCoodinate[iR][0] = "START";
            StatusSetIJCoodinate[jR][mazeLength - 1] = "GOAL";
            verticalTraverse = false;
        }
        else if (c % 4 == 2)
        {
            START = StatusSetIJCoodinate[0][iR];
            GOAL  = StatusSetIJCoodinate[mazeLength - 1][jR];
            StatusSetIJCoodinate[0][iR] = "START";
            StatusSetIJCoodinate[mazeLength - 1][jR] = "GOAL";
            verticalTraverse = true;
        }
        else if (c % 4 == 1)
        {
            START = StatusSetIJCoodinate[iR][mazeLength - 1];
            GOAL  = StatusSetIJCoodinate[jR][0];
            StatusSetIJCoodinate[iR][mazeLength - 1] = "START";
            StatusSetIJCoodinate[jR][0] = "GOAL";
            verticalTraverse = false;
        }
        else
        {
            START = StatusSetIJCoodinate[mazeLength - 1][iR];
            GOAL  = StatusSetIJCoodinate[0][jR];
            StatusSetIJCoodinate[mazeLength - 1][iR] = "START";
            StatusSetIJCoodinate[0][jR] = "GOAL";
            verticalTraverse = true;
        }

        numberOfStates += 1;
    }

    void GenerateFloorAndSetTags(Vector3 coodinate, string tag)
    {
        GameObject objContainer = Instantiate(floor);
        Transform myTransform = objContainer.GetComponent<Transform>();
        myTransform.position = coodinate;
        objContainer.name = tag;
    }

    void setStartAndGoal()
    {
        GameObject objContainer = GameObject.Find(START);
        objContainer.GetComponent<MeshRenderer>().material = ColorSet[0];
        objContainer.name = "START";
        START = "START";

        Transform myTransform = this.transform;
        myTransform.position = objContainer.GetComponent<Transform>().position;
        
        objContainer = GameObject.Find(GOAL);
        objContainer.GetComponent<MeshRenderer>().material = ColorSet[1];
        objContainer.name = "GOAL";
        GOAL = "GOAL";

        Vector3 newCoodinater = myTransform.position;
        newCoodinater.y = 3.0f;
        myTransform.position = newCoodinater;
    }

    void setWall()
    {
        //start,goal
        System.Security.Cryptography.RNGCryptoServiceProvider r =
            new System.Security.Cryptography.RNGCryptoServiceProvider();

        int iR = 0;
        byte[] rndary = new byte[10];

        for (int i = 0; i< wallNum; i++)
        {
            GameObject objContainer = Instantiate(wall);
            Transform myTransform = objContainer.GetComponent<Transform>();

            r.GetBytes(rndary);
            iR = (rndary[0] + rndary[1] + rndary[2] + rndary[3] + rndary[4] + rndary[5]) % mazeLength;
            if (annoyance)
            {
                if (verticalTraverse)
                {
                    while (START == StatusSetIJCoodinate[iR][i] || GOAL == StatusSetIJCoodinate[iR][i])
                    {
                        r.GetBytes(rndary);
                        iR = (rndary[0] + rndary[1] + rndary[2] + rndary[3] + rndary[4] + rndary[5]) % mazeLength;
                    }
                    CoodinateSet[iR][i].y = 5.0f;
                    myTransform.position = CoodinateSet[iR][i]; // 横に障壁ができる
                    StatusSetIJCoodinate[iR][i] = "wall";
                }
                else
                {
                    while (START == StatusSetIJCoodinate[i][iR] || GOAL == StatusSetIJCoodinate[i][iR])
                    {
                        r.GetBytes(rndary);
                        iR = (rndary[0] + rndary[1] + rndary[2] + rndary[3] + rndary[4] + rndary[5]) % mazeLength;
                    }
                    CoodinateSet[i][iR].y = 5.0f;
                    myTransform.position = CoodinateSet[i][iR]; // 縦に障壁ができる
                    StatusSetIJCoodinate[i][iR] = "wall";
                }
            }
            else
            {
                while (START == StatusSetIJCoodinate[i][iR] || GOAL == StatusSetIJCoodinate[i][iR])
                {
                    r.GetBytes(rndary);
                    iR = (rndary[0] + rndary[1] + rndary[2] + rndary[3] + rndary[4] + rndary[5]) % mazeLength;
                }
                CoodinateSet[iR][i].y = 5.0f;
                myTransform.position = CoodinateSet[i][iR];
                StatusSetIJCoodinate[i][iR] = "wall";
            }
            objContainer.tag = "wall";
        }
    }

    void setOutWall()
    {
        GameObject[] cube = new GameObject[4];
        Transform[] myTransform = new Transform[4];
        Vector3 newCoodinater;
        for (int i = 0; i < 4; i++)
        {
            newCoodinater = new Vector3(0.0f, 0.0f, 0.0f);
            cube[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if(i<2) cube[i].transform.localScale = new Vector3(mazeLength * 10, 10.0f, 0.5f);
            else    cube[i].transform.localScale = new Vector3(0.5f, 10.0f, mazeLength * 10);
            
            myTransform[i] = cube[i].GetComponent<Transform>();
            newCoodinater.y = 5.0f;
            if (i == 0) newCoodinater.z = CoodinateSet[0][0].z + 5.0f;
            else if (i == 1) newCoodinater.z = CoodinateSet[mazeLength-1][mazeLength-1].z - 5.0f;
            else if (i == 2) newCoodinater.x = CoodinateSet[0][0].x - 5.0f;
            else newCoodinater.x = CoodinateSet[mazeLength-1][mazeLength-1].x + 5.0f;
            myTransform[i].position = newCoodinater;
            
            cube[i].tag = "wall";
            cube[i].GetComponent<Collider>().isTrigger = true;
            Rigidbody rigidbody = cube[i].AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
        }
    }
}