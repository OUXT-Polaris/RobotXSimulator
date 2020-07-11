using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;

using ROS2;
using ROS2.Utils;

public class LidarSensor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(splitEquidistant)
        {
            verticalAngles = new List<float>();
            float vertical_resolution = (upperAngle - lowerAngle)/(numberOfLines-1);
            for(int i=0; i<numberOfLines; i++)
            {
                verticalAngles.Add(lowerAngle + vertical_resolution * (float)i);
            }
        }
        scanDeltaTime = 1.0f/((360.0f*frequency)/horizontalResoluation);
        pointsMsg = getEmptyCloud();
        RCLdotnet.Init();
        node = RCLdotnet.CreateNode(nodeName);
        pointsPub = node.CreatePublisher<sensor_msgs.msg.PointCloud2>(pointcloudTopic);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void FixedUpdate()
    {
        var delta = Time.fixedDeltaTime;
        int num_scan = Convert.ToInt32(Math.Ceiling(delta/scanDeltaTime));
        for(int i=0; i<num_scan; i++){
            foreach(var vertical_angle in verticalAngles)
            {
                var result = Raycast(currentScanAngle,vertical_angle);
                /*If the Raycast was succeeded*/
                if(result.Item1){
                    var hit_position_world = result.Item2.point;
                    var hit_position_local = transform.InverseTransformPoint(hit_position_world);
                    float hit_position_x_ros = hit_position_local.z;
                    float hit_position_y_ros = hit_position_local.x * -1;
                    float hit_position_z_ros = hit_position_local.y;
                    byte[] hit_position_x_ros_byte = BitConverter.GetBytes(hit_position_x_ros);
                    byte[] hit_position_y_ros_byte = BitConverter.GetBytes(hit_position_y_ros);
                    byte[] hit_position_z_ros_byte = BitConverter.GetBytes(hit_position_z_ros);
                    foreach(var b in hit_position_x_ros_byte){
                        pointsMsg.Data.Add(b);
                    }
                    foreach(var b in hit_position_y_ros_byte){
                        pointsMsg.Data.Add(b);
                    }
                    foreach(var b in hit_position_z_ros_byte){
                        pointsMsg.Data.Add(b);
                    }
                    numPoints = numPoints + 1;
                }
            }
            currentScanAngle = currentScanAngle + horizontalResoluation;
            if(currentScanAngle>=360.0f){
                pointsMsg.Row_step = (uint)numPoints*pointsMsg.Point_step;
                pointsMsg.Width = (uint)numPoints;
                pointsPub.Publish(pointsMsg);
                pointsMsg = getEmptyCloud();
                currentScanAngle = 0.0f;
                numPoints = 0;
            }
        }
    }

    (bool,RaycastHit) Raycast(float scanAngle,float verticalAngles = 0.0f)
    {
        RaycastHit hit;
        var vector = Quaternion.AngleAxis(verticalAngles, Vector3.forward) * Quaternion.AngleAxis(scanAngle, Vector3.up) * transform.forward;
        Ray ray = new Ray(transform.position, vector);
        bool result = Physics.Raycast(ray, out hit, range, layerMask);
        rayBuffer.Add(ray);
        //Debug.DrawRay(ray.origin, ray.direction * range, Color.green, 5, false);
        return (result,hit);
    }

    void OnDrawGizmos()
    {
        if(!showGizmos)
        {
            return;
        }
        Gizmos.color = Color.red;
        foreach(var ray in rayBuffer){
            Gizmos.DrawRay (ray.origin, ray.direction * range);
        }
        rayBuffer.Clear();
    }

    [SerializeField] private string nodeName = "lidar_driver";
    [SerializeField] private string pointcloudTopic = "/points_raw";
    [SerializeField] private string frameId = "lidar";
    [SerializeField] private int numberOfLines = 16;
    [SerializeField] private List<float> verticalAngles;
    [SerializeField] private bool splitEquidistant = true;
    [SerializeField] private float upperAngle = 15.0f;
    [SerializeField] private float lowerAngle = -15.0f;
    [SerializeField,Range(0.1f,20.0f),TooltipAttribute("frequency of the lidar scan in hz")] private float frequency = 10.0f;
    [SerializeField,Range(0.1f,10.0f),TooltipAttribute("horizontal resolution of the lidar scan in degree")] private float horizontalResoluation = 1.0f;

    [SerializeField] private float range = 200.0f;

    [SerializeField]
    LayerMask layerMask = new LayerMask();

    [SerializeField]
    bool showGizmos = true;

    private List<Ray> rayBuffer = new List<Ray>();

    private float currentScanAngle = 0.0f;

    private float scanDeltaTime = 0.0f;

    private int numPoints = 0;

    private sensor_msgs.msg.PointCloud2 pointsMsg = new sensor_msgs.msg.PointCloud2();

    private IPublisher<sensor_msgs.msg.PointCloud2> pointsPub;
    private INode node;

    sensor_msgs.msg.PointCloud2 getEmptyCloud()
    {
        sensor_msgs.msg.PointCloud2 cloud = new sensor_msgs.msg.PointCloud2();
        if(BitConverter.IsLittleEndian){
            cloud.Is_bigendian = false;
        }
        else{
            cloud.Is_bigendian = true;
        }
        cloud.Header.Frame_id = frameId;
        cloud.Header.Stamp = TimeUtil.getStamp();
        cloud.Point_step = 12;
        cloud.Height = 1;
        cloud.Is_dense = true;
        cloud.Fields = getFields();
        return cloud;
    }
    private List<sensor_msgs.msg.PointField> getFields()
    {
        List<sensor_msgs.msg.PointField> ret = new List<sensor_msgs.msg.PointField>();
        sensor_msgs.msg.PointField x = new sensor_msgs.msg.PointField();
        x.Name = "x";
        x.Offset = 0;
        x.Datatype = sensor_msgs.msg.PointField.FLOAT32;
        x.Count = 1;
        sensor_msgs.msg.PointField y = new sensor_msgs.msg.PointField();
        y.Name = "y";
        y.Offset = 4;
        y.Datatype = sensor_msgs.msg.PointField.FLOAT32;
        y.Count = 1;
        sensor_msgs.msg.PointField z = new sensor_msgs.msg.PointField();
        z.Name = "z";
        z.Offset = 8;
        z.Datatype = sensor_msgs.msg.PointField.FLOAT32;
        z.Count = 1;
        sensor_msgs.msg.PointField i = new sensor_msgs.msg.PointField();
        i.Name = "intensity";
        i.Offset = 12;
        i.Datatype = sensor_msgs.msg.PointField.FLOAT32;
        i.Count = 1;
        sensor_msgs.msg.PointField r = new sensor_msgs.msg.PointField();
        r.Name = "ring";
        r.Offset = 14;
        r.Datatype = sensor_msgs.msg.PointField.UINT16;
        r.Count = 1;
        sensor_msgs.msg.PointField t = new sensor_msgs.msg.PointField();
        t.Name = "time";
        t.Offset = 18;
        t.Datatype = sensor_msgs.msg.PointField.FLOAT32;
        t.Count = 1;
        ret.Add(x);
        ret.Add(y);
        ret.Add(z);
        /*
        ret.Add(i);
        ret.Add(r);
        ret.Add(t);
        */
        return ret;
    }
}
