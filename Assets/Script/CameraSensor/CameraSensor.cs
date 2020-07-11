using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using ROS2;
using ROS2.Utils;

public class CameraSensor : MonoBehaviour
{
    [SerializeField] private Camera imageCamera;
    [SerializeField] private string opticalFrameId = "front_camera_optical";
    [SerializeField] private string frameId = "front_camera";
    [SerializeField] private string nodeName = "front_camera_driver";
    [SerializeField] private string imageTopic = "front_camera/image_raw";
    [SerializeField,Range(0, 1920)] private int resolutionWidth = 640;
    [SerializeField,Range(0, 1080)] private int resolutionHeight = 480;
    [SerializeField,Range(0, 100)] private int qualityLevel = 75;
    [SerializeField,Range(1, 30)] private float frequency = 30.0f; 

    private Texture2D texture2D;
    private Rect rect;
    private INode node;
    private IPublisher<sensor_msgs.msg.CompressedImage> imagePub;
    private sensor_msgs.msg.CompressedImage imageMsg = new sensor_msgs.msg.CompressedImage();
    private sensor_msgs.msg.CameraInfo cameraInfoMsg = new sensor_msgs.msg.CameraInfo();
    private Mutex mut = new Mutex();

    protected void Start()
    {
        texture2D = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.RGB24, false);
        rect = new Rect(0, 0, resolutionWidth, resolutionHeight);
        imageCamera.targetTexture = new RenderTexture(resolutionWidth, resolutionHeight, 24);
        Camera.onPostRender += UpdateImage;
        RCLdotnet.Init();
        node = RCLdotnet.CreateNode(nodeName);
        imagePub = node.CreatePublisher<sensor_msgs.msg.CompressedImage>(imageTopic);
        InvokeRepeating("PublishImage", 0.0f, 1.0f/frequency);
    }

    private void UpdateImage(Camera _camera)
    {
        if (texture2D != null && _camera == this.imageCamera)
            UpdateMessage();
    }

    private void UpdateMessage()
    {
        mut.WaitOne();
        try
        {
            texture2D.ReadPixels(rect, 0, 0);
            //imageMsg.Data = new List<byte>(texture2D.GetRawTextureData());
            imageMsg.Data = new List<byte>(texture2D.EncodeToJPG(qualityLevel));
            imageMsg.Format = "jpeg";
        }
        finally
        {
            mut.ReleaseMutex();
        }
    }

    private void PublishImage()
    {
        mut.WaitOne();
        try
        {
            var stamp = TimeUtil.getStamp();
            imageMsg.Header.Frame_id = opticalFrameId;
            imageMsg.Header.Stamp = stamp;
            imagePub.Publish(imageMsg);
        }
        finally
        {
            mut.ReleaseMutex();
        }
    }
}