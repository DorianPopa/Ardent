namespace GoogleARCore.ARObjectPlacer
{
    using GoogleARCore;
    using UnityEngine;
    using UnityEngine.EventSystems;

    using Dummiesman;

#if UNITY_EDITOR
    using Input = InstantPreviewInput;
#endif

    public class ARController : MonoBehaviour
    {
        private AppController appController = null;

        public Camera FirstPersonCamera;

        public GameObject GameObjectForVerticalPlane;
        public GameObject GameObjectForHorizontalPlane;
        public GameObject GameObjectForFeaturePoint;

        private const float ROTATION_FOR_VERTICAL_PLANE = 180.0f;

        private bool isQuitting = false;

        public void Awake()
        {
            appController = FindObjectOfType<AppController>();

            if(QualitySettings.vSyncCount == 0)
                Application.targetFrameRate = 60;

            var loadedObj = new OBJLoader().Load(appController.objPathToLoad);
            //loadedObj.GetComponent<Transform>().localScale = new Vector3(0.2f, 0.2f, 0.2f);

            GameObjectForHorizontalPlane = loadedObj;
        }

        public void Update()
        {
            UpdateApplicationLifecycle();

            Touch touch;
            if(Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            {
                return;
            }

            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }

            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon | TrackableHitFlags.FeaturePointWithSurfaceNormal;

            if(Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
            {

                if((hit.Trackable is DetectedPlane) && 
                    Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position, hit.Pose.rotation * Vector3.up) < 0)
                {
                    Debug.Log("There was a hit at the back of the current DetectedPlane");
                }
                else
                {

                    GameObject objectToBeInstantiated;
                    if(hit.Trackable is FeaturePoint)
                    {
                        objectToBeInstantiated = GameObjectForFeaturePoint;
                    }
                    else if(hit.Trackable is DetectedPlane)
                    {
                        DetectedPlane detectedPlane = hit.Trackable as DetectedPlane;
                        if(detectedPlane.PlaneType == DetectedPlaneType.Vertical)
                        {
                            objectToBeInstantiated = GameObjectForVerticalPlane;
                        }
                        else
                        {
                            objectToBeInstantiated = GameObjectForHorizontalPlane;
                        }
                    }
                    else
                    {
                        objectToBeInstantiated = GameObjectForHorizontalPlane;
                    }

                    GameObject gameObject = Instantiate(objectToBeInstantiated, hit.Pose.position, hit.Pose.rotation);

                    gameObject.transform.Rotate(0, ROTATION_FOR_VERTICAL_PLANE, 0, Space.Self);

                    Anchor anchor = hit.Trackable.CreateAnchor(hit.Pose);

                    gameObject.transform.parent = anchor.transform;
                }
            }
        }

        private void UpdateApplicationLifecycle()
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                appController.LoadScene("MainMenuScene");
            }

            if (Session.Status != SessionStatus.Tracking)
            {
                Screen.sleepTimeout = SleepTimeout.SystemSetting;
            }
            else
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            if (isQuitting) return;

            if(Session.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                ShowAndroidToastMessage("Camera permission is needed to run this application.");
                isQuitting = true;
                Invoke("QuitTheApplication", 0.5f);
            }
            else if (Session.Status.IsError())
            {
                ShowAndroidToastMessage("ARCore encountered a problem connecting. Please start the app again.");
                isQuitting = true;
                Invoke("QuitTheApplication", 0.5f);
            }
        }

        private void QuitTheApplication()
        {
            Application.Quit();
        }

        private void ShowAndroidToastMessage(string message)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if(unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject = 
                        toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                    toastObject.Call("show");
                }));
            }
        }
    }
}
