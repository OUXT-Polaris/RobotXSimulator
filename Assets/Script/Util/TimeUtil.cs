using System;
using ROS2;

public static class TimeUtil
{
    public static builtin_interfaces.msg.Time getStamp(DateTime dateTime)
    {
        builtin_interfaces.msg.Time stamp = new builtin_interfaces.msg.Time();
        DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
        TimeSpan span = (dateTime - Epoch);
        double total_secs = span.TotalSeconds;
        stamp.Sec = (int)Math.Floor(total_secs);
        stamp.Nanosec = (uint)((total_secs - Math.Floor(total_secs))*Math.Pow(10,9));
        return stamp;
    }

    public static builtin_interfaces.msg.Time getStamp()
    {
        return getStamp(DateTime.Now);
    }
}