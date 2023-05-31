using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QCHT.Core.Extensions;
using QCHT.Interactions.Hands;
using System;
using QCHT.Interactions.Core;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using Unity.VisualScripting;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEditor;
using UnityEngine.XR.ARFoundation;
using System.Runtime.CompilerServices;

namespace QCHT.Core.Hands
{
//[Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html", false)]
public class arhnadTrack : XRSubsystemLifeCycleManager<XRHandTrackingSubsystem,
        XRHandTrackingSubsystemDescriptor, XRHandTrackingSubsystem.Provider>
    {
        private static readonly Vector3 s_leftThumpMcp = new Vector3(0.0270322636f, 0.00485388096f, 0.00566395884f);
        private static readonly Vector3 s_rightThumpMcp = new Vector3(-0.0270322636f, 0.00485388096f, 0.00566395884f);

        [SerializeField]
        public HandType handType;

        public bool IsTracked { get; private set; }
        public Pose Pose { get; private set; }
        public XrHandedness HandType => (XrHandedness)handType;
        public HandPose HandPose { get; private set; }
        public XrHandGesture GestureId { get; private set; }
        public float GestureRatio { get; private set; }
        public float FlipRatio { get; private set; }
        
        private Pose[] _leftPoses = new Pose[(int)XrHandJoint.XR_HAND_JOINT_MAX];
        private Pose[] _rightPoses = new Pose[(int)XrHandJoint.XR_HAND_JOINT_MAX];
        private float[] _leftScales = new float[(int)XrHandJoint.XR_HAND_JOINT_MAX];
        private float[] _rightScales = new float[(int)XrHandJoint.XR_HAND_JOINT_MAX];

        //finger position information
        //left hand data
        public Pose _lThumbTip;
        public Pose _leftindexTip;
        public Pose _lPalm;
        public Pose _lPalmWorld;

        public bool _lPinched = false;
        public bool _lGriped = false;

        public float _lpinchDIst;
        public float l_menuAngle;
        public Boolean l_menuGestured;
        public Vector3 _lSpeed;
        
        //right hand data
        public Pose _rThumbTip;
        public Pose _rightindexTip;
        public Pose _rPalm;
        public Pose _rPalmWorld;
        public Pose _ballAttach;

        private bool _rPinched = false;
        public float _rpinchDIst;
        public float menuAngle;
        public Boolean menuGestured;
        public Vector3 _rSpeed;


        bool r_menuCalled;
        bool l_menuCalled = false;
        private XROrigin _xrOrigin;
        //reference data
        bool isLeft = true;


        //menuInfo
        [SerializeField] public GameObject l_menu;
        [SerializeField] private GameObject r_keyBoard;
        [SerializeField] private GameObject mainCamera;
        //finger tip colider info
        [SerializeField] private float _redius = 0.01f;
        [SerializeField] private LayerMask _buttonMask;
        [SerializeField] private LayerMask _menuMask;
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private LayerMask _ballMask;
        [SerializeField] private LayerMask _launchPadMask;
        [SerializeField] private LayerMask _keyboard;
        [SerializeField] private GameObject player;
        [SerializeField] public GameObject AR_Scene;
        [SerializeField] private GameObject origion;

        [SerializeField] private GameObject changeModeBtn;
        [SerializeField] private GameObject ballManager;
        [SerializeField] private GameObject debugsphere;
        private XRHandTrackingManager outsideTrackingdata;
        private changeModeButton _changeMode;
        private BallManager _ballManager;
        private ARPlaneManager planemanager;
        private readonly Collider[] handcolid = new Collider[10];
        public int numFound;
        public int groundFound;
        private int ballFound;
        private bool lindPressed = false;
        private bool rindPressed = false;
        buttonInterface temp;
        // debug prompt
        public String _interactedButton;
        // Start is called before the first frame update

        private float nextUpdate = 0.05f;

        public Vector3 _lRefpoint;
        public Vector3 _rRefpoint;

        private GameObject pickedBall;

        float previousUpdate;

        Pose scenePosition;
        //debug
        public int isReached = 0;
        void Start()
        {
            Debug.Log("123");
            planemanager = origion.GetComponent<ARPlaneManager>();
            _changeMode = changeModeBtn.GetComponent<changeModeButton>();
            _ballManager = ballManager.GetComponent<BallManager>();
            outsideTrackingdata = origion.GetComponent<XRHandTrackingManager>();
            l_menuCalled = false;
            lindPressed = false;
            rindPressed = false;
            pickedBall = null;
        }

        // Update is called once per frame
        void Update()
        {
            //initialize hands
            GetLHandData();
            GetRHandData();
            //update velocity every 0.1 second
            if (Time.time >= nextUpdate)
            {
                //Debug.Log(Time.time + ">=" + nextUpdate);
                // Change the next update (current second+1)
                nextUpdate = Time.time + 0.05f;
                previousUpdate = Time.time;
                // Call your fonction
                updatelocateion();
            }

            //visualiz the gloable position of right hand
            //call keyboard
            updateSpeed();
            //quick menu colide 
           /* if (_lPalm.rotation.eulerAngles.z < 130 &&
               _lPalm.rotation.eulerAngles.z > 100)*/
            //{
                //if facing self and pinched, activate quickmenu and move it above left hand
                if (_lPinched && !l_menu.activeSelf)
                {
/*                    isReached = 5;
*/                  l_menu.SetActive(true);
                    l_menu.transform.position = _lPalmWorld.position;
                    l_menu.transform.rotation = Quaternion.Euler(0f,mainCamera.transform.rotation.eulerAngles.y,0f);
                    l_menuCalled = true;
                }
            //}
            if (_lPinched)
            {
                debugsphere.transform.position = _rPalmWorld.position;
                l_menu.transform.position = _lPalmWorld.position;
                l_menu.transform.rotation = Quaternion.Euler(0f, mainCamera.transform.rotation.eulerAngles.y, 0f);
                l_menuCalled = true;
            }

            /*            if (l_menuCalled)
                        {
                            numFound = Physics.OverlapSphereNonAlloc(_rightindexTip.position, _redius, handcolid, _menuMask);
                            if (numFound > 0)
                            {
                                temp = handcolid[0].GetComponent<buttonInterface>();
                                temp.hovered();
                                if (temp != null && !rindPressed)
                                {

                                    rindPressed = true;
                                    _interactedButton = temp.buttonName();
                                }
                            }
                            else
                            {

                                if (rindPressed)
                                {
                                    temp.pressed();
                                }     
                                rindPressed = false;
                                temp.unhovered();
                            }

                        }*/
            //checking left hand finger tip colider for key board
            //if keyboard.activated;
            /*int keyfound = Physics.OverlapSphereNonAlloc(_leftindexTip.position, _redius, handcolid, _keyboard);
            if (keyfound > 0)
            {
                //isReached++;
                temp = handcolid[0].gameObject.GetComponent<buttonInterface>();
                temp.hovered();
                if (temp != null && !lindPressed)
                {

                    lindPressed = true;
                    _interactedButton = temp.buttonName();
                }
            }
            else
            {
                //isReached+=2;
                if (lindPressed)
                {
                    temp.pressed();
                }
                lindPressed = false;
                if (temp != null)
                {
                    temp.unhovered();
                    temp = null;
                }
            }
*/
            //launchPad colide
            /*numFound = Physics.OverlapSphereNonAlloc(_rightindexTip.position, _redius, handcolid, _launchPadMask);
			if (numFound > 0)
			{
				temp = handcolid[0].gameObject.GetComponent<buttonInterface>();
				temp.hovered();
				if (temp != null && !rindPressed)
				{
            		rindPressed = true;
					_interactedButton = temp.buttonName();
				}
			}
			else
			{  
                if (rindPressed)
                {
                    temp.pressed();
                }
				rindPressed = false;
				if (temp != null)
				{
					temp.unhovered();
					temp = null;
				}
			}*/


            //colide with ground to get hit pos info(globle)
            //eighgr hand touch ground to activate the scene.
            groundFound = Physics.OverlapSphereNonAlloc(_rPalmWorld.position, _redius*15, handcolid, _groundMask);

            if (groundFound > 0 && !AR_Scene.activeInHierarchy)
            {
                float groundHeight =  player.transform.position.y;
                //Vector3 scenepo = new Vector3(_rPalmWorld.position.x, _rPalmWorld.position.y - 0.5f, _rPalmWorld.position.z);
                float yangle = mainCamera.transform.rotation.eulerAngles.y;
                Quaternion scenerotation = Quaternion.Euler(0, yangle-90f, 0);
                AR_Scene.SetActive(true);
                AR_Scene.transform.position = _rPalmWorld.position;
				AR_Scene.transform.rotation = scenerotation;
                planemanager.enabled = false;
            }

            //change mode to hand throw, we can pickup balls
            
            if(_changeMode.gameMode == "HandThrow")
            {
				//colide with balls and pinch to pick up. 
				ballFound = Physics.OverlapSphereNonAlloc(_rightindexTip.position, _redius, handcolid, _ballMask);
                bool picked = false;
                if (ballFound > 0 && _rPinched)
				{
                    //CALL pick up ball function
                    pickedBall = handcolid[0].gameObject;
                    _ballManager.PickBall(handcolid[0].gameObject);
                    _ballManager.AttachToHand(handcolid[0].gameObject);
                    picked = true;
					
				}
                    if (picked && !_rPinched)
                    {
                        isReached = 123;
                        _ballManager.ThrowBall(pickedBall, _rSpeed * 5);
                    }
                
			}
            


        }

        
        private Transform OriginTransform
        {
            get
            {
#if UNITY_AR_FOUNDATION_LEGACY
                if (_arOrigin != null)
                    return _arOrigin.transform;
#endif
                return _xrOrigin ? _xrOrigin.transform : transform;
            }
        }

        private void GetLHandData()
        {
            ref var joints = ref isLeft ? ref _leftPoses : ref _rightPoses;
            ref var scales = ref isLeft ? ref _leftScales : ref _rightScales;
            var isTracked = false;
            var gestureId = 0;
            var gestureRatio = 0f;
            var flipRatio = 1f;

            //get left hand data;
            subsystem?.GetHandData(true, ref isTracked, ref joints, ref scales, ref gestureId, ref gestureRatio, ref flipRatio);
            _lThumbTip = outsideTrackingdata._leftPoses[4];
            _leftindexTip = outsideTrackingdata._leftPoses[9];
            _lPalm = outsideTrackingdata._leftPoses[0];
            _lPalmWorld = outsideTrackingdata._leftPoses[0];
            //toDisplay = devicePose;
            //get pinch dist info
            _lpinchDIst = Vector3.Distance(_lThumbTip.position, _leftindexTip.position);
            _lPinched = (XrHandGesture)gestureId == XrHandGesture.XR_HAND_PINCH;
            //get menu angle;
            float tempAngle = Vector3.Angle(_rThumbTip.up, _rightindexTip.up);
            l_menuAngle = math.abs(tempAngle);
            _lGriped = (XrHandGesture)gestureId == XrHandGesture.XR_HAND_GRAB;

            

        }

        private void GetRHandData()
        {
            ref var joints = ref isLeft ? ref _leftPoses : ref _rightPoses;
            ref var scales = ref isLeft ? ref _leftScales : ref _rightScales;
            var isTracked = false;
            var gestureId = 0;
            var gestureRatio = 0f;
            var flipRatio = 1f;

            //get right hand data;
            subsystem?.GetHandData(false, ref isTracked, ref joints, ref scales, ref gestureId, ref gestureRatio, ref flipRatio);
            _rThumbTip = outsideTrackingdata._rightPoses[4];
            _rightindexTip = outsideTrackingdata._rightPoses[9];
            _rPalm = outsideTrackingdata._rightPoses[0];
            _ballAttach = Unity.XR.CoreUtils.TransformExtensions.InverseTransformPose(OriginTransform, joints[(int)XrHandJoint.XR_HAND_JOINT_INDEX_DISTAL]);
            _rPalmWorld = outsideTrackingdata._rightPoses[0];
            //get angle
            float tempAngle = Vector3.Angle(_rThumbTip.up, _rightindexTip.up);
            menuAngle = math.abs(tempAngle);
            // get right hand data;
           
            _rpinchDIst = Vector3.Distance(_rThumbTip.position, _rightindexTip.position);
            _rPinched = (XrHandGesture)gestureId == XrHandGesture.XR_HAND_PINCH;

            if(Vector3.Distance(_rThumbTip.position, _rPalm.position) < 0.055f)
            {
                
            }
        }
        private void updatelocateion()
        {
            _rRefpoint = _rPalm.position;
            _lRefpoint = _lPalm.position;
        }
        public void updateSpeed()
        {
            float DT = Time.time - previousUpdate;
            _rSpeed = (_rPalm.position - _rRefpoint) / DT;
            _lSpeed = (_lPalm.position - _lRefpoint) / DT;
        }
    }
}
