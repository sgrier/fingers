using System;
using Leap;

public struct HandData {
  public Boolean isLeft;
  public Boolean isActive;
  public Leap.Vector pos;
  public float angle;
}

class LeapHandler
{
  private Fingers fingers;
  HandData leftHand;
  HandData rightHand;
  Leap.IController controller;
  
  public LeapHandler(Fingers parent) {
    fingers = parent;

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
    controller.SetPolicy(Leap.Controller.PolicyFlag.POLICY_ALLOW_PAUSE_RESUME);
    controller.SetPolicy(Leap.Controller.PolicyFlag.POLICY_BACKGROUND_FRAMES);
    controller.SetPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
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
      foreach (Finger finger in hand.Fingers) {
        if (finger.Type != Finger.FingerType.TYPE_INDEX) continue;

        Bone mcp = finger.Bone(Bone.BoneType.TYPE_METACARPAL);

        if (hand.IsLeft) {
          leftHand.pos = mcp.NextJoint;
          leftHand.isActive = true;
          leftHand.angle = mcp.NextJoint[2]; // Actual rotation is not reliable, use pos
        } else {
          rightHand.pos = mcp.NextJoint;
          rightHand.isActive = true;
          rightHand.angle = mcp.NextJoint[2]; // Actual rotation is not reliable, use pos
        }
      }
    }

    if (frame.Hands.Count != 0)
    {
      fingers.HandleHands(leftHand, rightHand);
    }
  }

  public void OnDeviceFailure(object sender, DeviceFailureEventArgs args)
  {
    Console.WriteLine("Device Error");
    Console.WriteLine("  PNP ID:" + args.DeviceSerialNumber);
    Console.WriteLine("  Failure message:" + args.ErrorMessage);
  }

  public void OnLogMessage(object sender, LogEventArgs args)
  {
    switch (args.severity)
    {
      case Leap.MessageSeverity.MESSAGE_CRITICAL:
        Console.WriteLine("Leap Message: [Critical]");
        break;
      case Leap.MessageSeverity.MESSAGE_WARNING:
        Console.WriteLine("Leap Message: [Warning]");
        break;
      case Leap.MessageSeverity.MESSAGE_INFORMATION:
        Console.WriteLine("Leap Message: [Info]");
        break;
      case Leap.MessageSeverity.MESSAGE_UNKNOWN:
        Console.WriteLine("Leap Message: [Unknown]");
        break;
    }
    //Console.WriteLine("[{0}] {1}", args.timestamp, args.message);
  }
}