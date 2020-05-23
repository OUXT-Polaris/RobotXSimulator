using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ROS2;
using ROS2.Utils;

public class SubscriberExample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        RCLdotnet.Init ();
        node = RCLdotnet.CreateNode ("listener");
        chatter_sub = node.CreateSubscription<std_msgs.msg.String> (
        "test", msg => Debug.Log ("I heard: [" + msg.Data + "]"));
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < spinSomeIterations; i++)
        {
            RCLdotnet.SpinOnce(node, (long)0.0);
        }
    }
private INode node;
private ISubscription<std_msgs.msg.String> chatter_sub;
private int spinSomeIterations = 10;
}
