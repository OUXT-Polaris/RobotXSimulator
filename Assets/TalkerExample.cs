using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading;

using UnityEngine;

using ROS2;
using ROS2.Utils;

public class TalkerExample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
	RCLdotnet.Init();
        node = RCLdotnet.CreateNode ("talker");
	pose_pub = node.CreatePublisher<geometry_msgs.msg.Pose> ("pose");
    }

    // Update is called once per frame
    void Update()
    {
	    geometry_msgs.msg.Pose msg = new geometry_msgs.msg.Pose();
　　　　　pose_pub.Publish (msg);
    }

    void OnDestroy()
    {
        pose_pub = null;
        node = null;
    }
private IPublisher<geometry_msgs.msg.Pose> pose_pub;
private INode node;
}
