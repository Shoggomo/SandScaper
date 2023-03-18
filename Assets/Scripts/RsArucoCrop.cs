using Intel.RealSense;
using UnityEngine;
using System;
using System.Collections.Generic;

[ProcessingBlockDataAttribute(typeof(RsArucoCrop))]
public class RsArucoCrop : RsProcessingBlock
{

    public int[] arucoIds;

    public Vector2 origin = new(220, 180);
    public Vector2Int size = new(420, 400);

    ushort[] depthData;

    // ushort[] colorData;


    Frame ApplyFilter(DepthFrame depth, VideoFrame color, FrameSource frameSource)
    {
        // TODO detect aruco markers and crop depth and color image

        var depthCount = depth.Width * depth.Height;
        // var colorCount = color.Width * color.Height;

        if (depthData == null || depthData.Length != depthCount)
            depthData = new ushort[depthCount];

        // if (colorData == null || colorData.Length != colorCount)
        //     colorData = new ushort[depthCount];

        depth.CopyTo(depthData);
        // color.CopyTo(colorData);

        for (int y = 0; y < depth.Height; y++)
        {
            for (int x = 0; x < depth.Width; x++)
            {
                if (x < origin.x || y < origin.y || x > origin.x + size.x || y > origin.y + size.y)
                {
                    // Debug.Log(x);
                    // Debug.Log(y);
                    // Debug.Log(x * depth.Width + y);
                    depthData[y * depth.Width + x] = 0;
                }
            }
        }



        var depthFrame = frameSource.AllocateVideoFrame<DepthFrame>(depth.Profile, depth, depth.BitsPerPixel, depth.Width, depth.Height, depth.Stride, Extension.DepthFrame);
        depthFrame.CopyFrom(depthData);

        // var videoFrame = frameSource.AllocateVideoFrame<VideoFrame>(color.Profile, color, color.BitsPerPixel, color.Width, color.Height, color.Stride, Extension.VideoFrame);
        // videoFrame.CopyFrom(videoData);

        return depthFrame;
    }

    public override Frame Process(Frame frame, FrameSource frameSource)
    {
        if (frame.IsComposite)
        {
            using var fs = FrameSet.FromFrame(frame);
            using var color = fs.ColorFrame;
            using var depth = fs.DepthFrame;
            var v = ApplyFilter(depth, color, frameSource);
            // return v;

            // find and remove the original depth frame
            var frames = new List<Frame>();
            foreach (var f in fs)
            {
                using (var p1 = f.Profile)
                    if (p1.Stream == Stream.Depth && p1.Format == Format.Z16)
                    {
                        f.Dispose();
                        continue;
                    }
                frames.Add(f);
            }
            frames.Add(v);

            var res = frameSource.AllocateCompositeFrame(frames);
            frames.ForEach(f => f.Dispose());
            using (res)
                return res.AsFrame();
        }

        return frame;
    }
}