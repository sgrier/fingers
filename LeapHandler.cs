using System;
using System.Numerics; // Vector
using Leap;

class LeapHandler
{
  // Angle of the leap relative to the eye forward vector in degrees; left handed rotation. You can
  // also use this to offset the cursor position (increasing a value will push the cursor in that
  // direction)
  // X+ : leap is rotated down
  // Y+ : leap rotated right
  // Z+ : leap is rotated counterclockwise
  private static Vector3 mountAngleOffset = new Vector3(10, 0, 0);

  // Position of the leap relative to the eye center in mm
  // X+ : leap is to the right of the eye
  // Y+ : leap is above the eye
  // Z+ : leap is forward of the eye
  //
  // Common values: (0, 62, 106) - pimax top-mounted leap
  //                (0, -45, 68) - pimax hand tracker
  private static Vector3 mountPositionOffset = new Vector3(0, -45, 68);

  private Fingers fingers;
  HandData leftHand;
  HandData rightHand;
  Leap.IController controller;
  Vector3 mountAngleOffsetRadians;

  public LeapHandler(Fingers parent)
  {
    fingers = parent;

    float deg2rad = ((float)Math.PI / 180);
    mountAngleOffsetRadians = new Vector3(mountAngleOffset.X * deg2rad, 
                                          mountAngleOffset.Y * deg2rad, 
                                          mountAngleOffset.Z * deg2rad);

    leftHand = new HandData() { isLeft = true };
    rightHand = new HandData();

    controller = new Leap.Controller();

    controller.FrameReady += OnFrame;
    controller.Device += OnConnect;
    controller.DeviceLost += OnDisconnect;
    controller.DeviceFailure += OnDeviceFailure;
    controller.LogMessage += OnLogMessage;
  }

  public void OnConnect(object sender, DeviceEventArgs args)
  {
    Console.WriteLine("Leap Connected");

    // Need to do this after we've connected
    controller.SetPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
    controller.SetPolicy(Leap.Controller.PolicyFlag.POLICY_BACKGROUND_FRAMES);

    controller.Config.Set<bool>("tracking_processing_auto_flip", true, delegate(bool success){
      if(success) Console.WriteLine("Auto flip enabled");
    });
  }

  public void OnDisconnect(object sender, DeviceEventArgs args)
  {
    Console.WriteLine("Leap Disconnected");
  }
  public void OnFrame(object sender, FrameEventArgs args)
  {
    // Get the most recent frame and report some basic information
    Frame frame = args.frame;

    leftHand.isActive = false;
    rightHand.isActive = false;

    foreach (Hand hand in frame.Hands)
    {
      ref HandData data = ref ((hand.IsLeft) ? ref leftHand : ref rightHand);

      Bone mcp = hand.Fingers.Find(x => x.Type == Finger.FingerType.TYPE_INDEX)
                             .Bone(Bone.BoneType.TYPE_METACARPAL);

      // Convert to HandData coordinate system
      // Can also consider using StabilizedPalmPosition
      data.pos = Util.RotatePosition(
        new Vector3(-mcp.NextJoint[0], -mcp.NextJoint[2], mcp.NextJoint[1]), mountAngleOffsetRadians);

      data.pos.X = data.pos.X + mountPositionOffset.X; // horizontal
      data.pos.Y = data.pos.Y + mountPositionOffset.Y; // upness
      data.pos.Z = data.pos.Z + mountPositionOffset.Z; // depth

      data.isActive = true;

      // Actual rotation is not reliable; use a combination of X/Y pos so users can drag
      // horizontally or vertically; this means up/right is "increase", down/left is "decrease"
      data.angle = mcp.NextJoint[2] + mcp.NextJoint[0];
    }

    if (frame.Hands.Count != 0)
      fingers.HandleHands(leftHand, rightHand);
  }

  public void OnDeviceFailure(object sender, DeviceFailureEventArgs args)
  {
    Console.WriteLine("Leap Error:");
    Console.WriteLine("  PNP ID:" + args.DeviceSerialNumber);
    Console.WriteLine("  Failure message:" + args.ErrorMessage);
  }

  public void OnLogMessage(object sender, LogEventArgs args)
  {
    if (args.message.Equals("LeapC PollConnection call was  eLeapRS_Timeout"))
    {
      Console.WriteLine("Leap Error: Could not connect");
      return;
    }

    switch (args.severity)
    {
      case Leap.MessageSeverity.MESSAGE_CRITICAL:
        Console.WriteLine("Leap Message: [Critical]: {0}", args.message);
        break;
      case Leap.MessageSeverity.MESSAGE_WARNING:
        Console.WriteLine("Leap Message: [Warning]: {0}", args.message);
        break;
      case Leap.MessageSeverity.MESSAGE_INFORMATION:
        Console.WriteLine("Leap Message: [Info]: {0}", args.message);
        break;
      case Leap.MessageSeverity.MESSAGE_UNKNOWN:
        Console.WriteLine("Leap Message: [Unknown]: {0}", args.message);
        break;
    }
  }
}