using BaseX;
using FrooxEngine;
using HarmonyLib;
using NeosModLoader;
using System;
using Valve.VR;

namespace PimaxSwordAsViveWand
{
    public class PimaxSwordAsViveWand : NeosMod
    {
        public override string Name => "PimaxSwordAsViveWand";
        public override string Author => "badhaloninja";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/badhaloninja/PimaxSwordAsViveWand";
        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("me.badhaloninja.PimaxSwordAsViveWand");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(SteamVRDriver), "RegisterController")]
        private class SteamVRDriver_RegisterController_Patch // ;-; 
        {
            public static bool Prefix(SteamVRDriver __instance, int index, ETrackedControllerRole role, string renderModel,
				ref TrackedObject ___LeftController, ref int ___leftIndex, ref Hand ___LeftHand, ref MappableTrackedObject ___BoundLeftHand, ref HapticPoint ___LeftHapticPoint,
				ref TrackedObject ___RightController, ref int ___rightIndex, ref Hand ___RightHand, ref MappableTrackedObject ___BoundRightHand, ref HapticPoint ___RightHapticPoint,

				ref bool ___DisableSkeletalModel, InputInterface ___inputInterface
				)
            {
				if (!(renderModel.Contains("aapvr") || renderModel.Contains("sword_handler"))) //renderModel.Contains("indexcontroller")
				{ //"sword_handler" 
					return true;
                }

                
				CVRSystem system = OpenVR.System;
				if (role == ETrackedControllerRole.LeftHand && ___LeftController != null)
				{
					if (___leftIndex == index)
					{
						return false;
					}
					Warn("LeftHand controller role is already mapped to: " + ___LeftController);
					if (system.GetControllerRoleForTrackedDeviceIndex((uint)___leftIndex) == ETrackedControllerRole.RightHand)
					{
						Msg("Remapping LeftController to Right");
						___RightController = ___LeftController;
						___RightController.CorrespondingBodyNode = BodyNode.RightController;
						((IStandardController)___RightController).Side = Chirality.Right;
						___rightIndex = ___leftIndex;
					}
					else
					{
						Msg("Remapping LeftController to new device: " + index);
						___LeftController.IsTracking = false;
						if (___LeftHand != null)
						{
							___LeftHand.IsTracking = false;
						}
						if (___BoundLeftHand != null)
						{
							___BoundLeftHand.IsTracking = false;
						}
					}
				}
				if (role == ETrackedControllerRole.RightHand && ___RightController != null)
				{
					if (___rightIndex == index)
					{
						return false;
					}
					Warn("RightHand controller role is already mapped to: " + ___RightController);
					if (system.GetControllerRoleForTrackedDeviceIndex((uint)___rightIndex) == ETrackedControllerRole.LeftHand)
					{
						Msg("Remapping RightController to Left");
						___LeftController = ___RightController;
						___LeftController.CorrespondingBodyNode = BodyNode.LeftController;
						((IStandardController)___LeftController).Side = Chirality.Left;
						___leftIndex = ___rightIndex;
					}
					else
					{
						Msg("Remapping LeftController to new device: " + index);
						___RightController.IsTracking = false;
						if (___RightHand != null)
						{
							___RightHand.IsTracking = false;
						}
						if (___BoundRightHand != null)
						{
							___BoundRightHand.IsTracking = false;
						}
					}
				}
				BodyNode bodyNode;
				BodyNode bodyNode2;
				Chirality chirality;
				bool flag;
				if (role == ETrackedControllerRole.LeftHand)
				{
					bodyNode = BodyNode.LeftController;
					bodyNode2 = BodyNode.LeftHand;
					chirality = Chirality.Left;
					flag = true;
					___leftIndex = index;
				}
				else
				{
					bodyNode = BodyNode.RightController;
					bodyNode2 = BodyNode.RightHand;
					chirality = Chirality.Right;
					flag = false;
					___rightIndex = index;
				}
				bool flag2 = false;
				IStandardController standardController;
				float3 zero;
				floatQ floatQ;
                
                flag2 = ___DisableSkeletalModel;
                SteamVR_Actions.Vive.Activate(SteamVR_Input_Sources.Any, 0, false);
                ViveController vive = ___inputInterface.CreateDevice<ViveController>("Vive Controller - " + role);
                vive.Side = chirality;
                vive.CorrespondingBodyNode = bodyNode;
                ViveController vive2 = vive;
                vive2.VibrationCallback = (Action<double>)Delegate.Combine(vive2.VibrationCallback, new Action<double>(delegate (double time)
                {
                    VibrateController(__instance, vive, time); ;
                }));
                standardController = vive;
                if (flag)
                {
                    zero = new float3(-0.02f, 0f, -0.16f);
                    floatQ = floatQ.Euler(140f, -90f, -90f);
                    SteamVR_Actions.Vive.left_hand.SetSkeletalTransformSpace(EVRSkeletalTransformSpace.Model);
                    SteamVR_Actions.Vive.left_hand.SetRangeOfMotion(EVRSkeletalMotionRange.WithoutController);
                }
                else
                {
                    zero = new float3(0.02f, 0f, -0.16f);
                    floatQ = floatQ.Euler(40f, -90f, -90f);
                    SteamVR_Actions.Vive.right_hand.SetSkeletalTransformSpace(EVRSkeletalTransformSpace.Model);
                    SteamVR_Actions.Vive.right_hand.SetRangeOfMotion(EVRSkeletalMotionRange.WithoutController);
                }
                floatQ floatQ2 = floatQ.Euler(90f, 90f, 90f);
                floatQ2 = floatQ2.Inverted;
                floatQ = (floatQ) * (floatQ2);




                Msg("Registering Controller: " + standardController);
                ___inputInterface.RegisterController(standardController);
                MappableTrackedObject mappableTrackedObject = null;
                if (flag2)
                {
                    MappableTrackedObject mappableTrackedObject2 = ___inputInterface.CreateDevice<MappableTrackedObject>(standardController.Name + " - Hand");
                    mappableTrackedObject2.Initialize(mappableTrackedObject2.Name, bodyNode2, MappingTarget.VR, zero, floatQ);
                    mappableTrackedObject2.Priority = -10;
                    mappableTrackedObject = mappableTrackedObject2;
                }
                Hand hand = new Hand(___inputInterface, flag ? Chirality.Left : Chirality.Right, 0);
                hand.TracksMetacarpals = true;
                SteamVRDriver.SetTracking(hand, false, true);
                Hand hand2 = hand;
                if (flag)
                {
                    ___LeftController = (TrackedObject)standardController;
                    ___LeftHand = hand2;
                    ___BoundLeftHand = mappableTrackedObject;
                    ___LeftHapticPoint = new HapticPoint(___inputInterface, 0.05f, new ControllerHapticPosition(Chirality.Left));
                    ___inputInterface.RegisterHapticPoint(___LeftHapticPoint);
                    return false;
                }
                ___RightController = (TrackedObject)standardController;
                ___RightHand = hand2;
                ___BoundRightHand = mappableTrackedObject;
                ___RightHapticPoint = new HapticPoint(___inputInterface, 0.05f, new ControllerHapticPosition(Chirality.Right));
                ___inputInterface.RegisterHapticPoint(___RightHapticPoint);
                return false;

            }

            [HarmonyReversePatch]
			[HarmonyPatch(typeof(SteamVRDriver), "VibrateController")]
			public static void VibrateController(SteamVRDriver instance, IStandardController controller, double time)
			{
				// its a stub so it has no initial content
				throw new NotImplementedException("It's a stub");
			}
		}
    }
}