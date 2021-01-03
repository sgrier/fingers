using System;
using System.Numerics; // Vector
using Leap;

class LeapHandler
{
  private Fingers fingers;
  HandData leftHand;
  HandData rightHand;
  Leap.IController controller;

  public LeapHandler(Fingers parent)
  {
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
      foreach (Finger finger in hand.Fingers)
      {
        if (finger.Type != Finger.FingerType.TYPE_INDEX) continue;

        Bone mcp = finger.Bone(Bone.BoneType.TYPE_METACARPAL);

        ref HandData data = ref ((hand.IsLeft) ? ref leftHand : ref rightHand);
        
        // Convert to HandData coordinate system
        
        data.pos = new Vector3(-mcp.NextJoint[0], -mcp.NextJoint[2], mcp.NextJoint[1]);
        data.isActive = true;
        // Actual rotation is not reliable; use a combination of X/Y pos so users can drag
        // horizontally or vertically; this means up/right is "increase", down/left is "decrease"
        data.angle = mcp.NextJoint[2] + mcp.NextJoint[0];
        //leftHand.isPinching = (hand.PinchStrength == 1); unreliable
      }
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
        Console.WriteLine("Leap Message: [Warning]");
        break;
      case Leap.MessageSeverity.MESSAGE_INFORMATION:
        Console.WriteLine("Leap Message: [Info]");
        break;
      case Leap.MessageSeverity.MESSAGE_UNKNOWN:
        Console.WriteLine("Leap Message: [Unknown]");
        break;
    }
  }
}