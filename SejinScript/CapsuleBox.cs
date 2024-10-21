using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapsuleBox : MonoBehaviour
{
    public int maxCapsule = 100;
    public int leftCapsule;
    
    void Start()
    {
        leftCapsule = maxCapsule;
        maxCapsule = 100;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool canGetCapsule() { return leftCapsule > 0; }

    public void minusCapsule() { leftCapsule--; }

    public void plusCapsule() { leftCapsule++; }
}
