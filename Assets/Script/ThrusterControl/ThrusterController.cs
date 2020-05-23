using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ROS2;
using ROS2.Utils;
public class ThrusterController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        RCLdotnet.Init();
        node = RCLdotnet.CreateNode(nodeName);
        jointCmdSub = node.CreateSubscription<std_msgs.msg.Float32> (
            jointCmdTopic, msg => currentJointCmd = msg.Data);
        thrustCmdSub = node.CreateSubscription<std_msgs.msg.Float32> (
            thrustCmdTopic, msg => currentThrustCmd = msg.Data);
    }

    // Update is called once per frame
    void Update()
    {
        var rigidbody = this.GetComponent<Rigidbody>();
        if(rigidbody == null)
        {
            Debug.LogError("Rigidbody is null");
            return;
        }
        for(int i = 0; i < maximumSpinInteration; i++)
        {
            RCLdotnet.SpinOnce(node, (long)0.0);
        }
        var target_angle = Mathf.Clamp(currentJointCmd,-1*maximumJointAngle,maximumJointAngle)/Mathf.PI*180;
        var base_dir = transform.eulerAngles;
        enginePivot.transform.eulerAngles = new Vector3(-90+base_dir.x, base_dir.y, base_dir.z+target_angle);
        var pos = enginePivot.transform.TransformPoint(enginePivot.transform.position);
        var dir = enginePivot.transform.TransformDirection(enginePivot.transform.forward);
        var q = Quaternion.Euler(enginePivot.transform.eulerAngles);
        var thrust_cmd = new Vector3(Mathf.Clamp(currentThrustCmd,-1*maximumThrust,maximumThrust)*rigidbody.mass*-1,0,0);
        var vec = q * thrust_cmd;
        rigidbody.AddForceAtPosition(vec,pos);
    }

    [SerializeField] private string jointCmdTopic = "/engine_joint/cmd";
    [SerializeField] private string thrustCmdTopic = "/thrust/cmd";
    [SerializeField] private string nodeName = "thruster_controller";
    [SerializeField] private GameObject enginePivot = null;
    [SerializeField,Range(1, 30)] private int maximumSpinInteration = 10;
    [SerializeField,Range(0,Mathf.PI)] private float maximumJointAngle = (float)0.5*Mathf.PI;
    [SerializeField,Range(0,30)] private float maximumThrust = 15.0f;
    private INode node = null;
    private ISubscription<std_msgs.msg.Float32> jointCmdSub = null;
    private ISubscription<std_msgs.msg.Float32> thrustCmdSub = null;
    private float currentJointCmd = 0.0f;
    private float currentThrustCmd = 0.0f;
}
