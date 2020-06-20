namespace GoogleARCore.ARObjectPlacer
{
    using GoogleARCore;
    using UnityEngine;
    using UnityEngine.EventSystems;

    using Dummiesman;
    using System.IO;
    using System.Text;

#if UNITY_EDITOR
    using Input = InstantPreviewInput;
#endif

    public class ARController : MonoBehaviour
    {
        private AppController appController = null;

        /// <summary>
        /// The first-person camera being used to render the passthrough camera image (AR
        /// background).
        /// </summary>
        public Camera FirstPersonCamera;

        /// <summary>
        /// A prefab to place when a raycast from a user touch hits a vertical plane.
        /// </summary>
        public GameObject GameObjectForVerticalPlane;

        /// <summary>
        /// A prefab to place when a raycast from a user touch hits a horizontal plane.
        /// </summary>
        public GameObject GameObjectForHorizontalPlane;

        /// <summary>
        /// A prefab to place when a raycast from a user touch hits a feature point.
        /// </summary>
        public GameObject GameObjectForFeaturePoint;

        /// <summary>
        /// The rotation in degrees need to apply to prefab when it is placed.
        /// </summary>
        private const float PREFAB_ROTATION = 180.0f;

        /// <summary>
        /// True if the app is in the process of quitting due to an ARCore connection error,
        /// otherwise false.
        /// </summary>
        private bool isQuitting = false;

        public void Awake()
        {
            appController = FindObjectOfType<AppController>();

            // Enable ARCore to target 60fps camera capture frame rate on supported devices.
            // Note, Application.targetFrameRate is ignored when QualitySettings.vSyncCount != 0.
            if(QualitySettings.vSyncCount == 0)
                Application.targetFrameRate = 60;

            var loadedObj = new OBJLoader().Load(appController.objPathToLoad);
            loadedObj.GetComponent<Transform>().localScale = new Vector3(0.2f, 0.2f, 0.2f);
            //Destroy(GameObject.Find(loadedObj.name));

            GameObjectForHorizontalPlane = loadedObj;
        }

        public void Update()
        {
            UpdateApplicationLifecycle();

            // If the user has not began a touch or has not touched the screen at all, 
            // there is no need to update anything
            Touch touch;
            if(Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            {
                return;
            }

            // Should not handle input if the user is pointing on UI elements.
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }

            // Raycast against the location the user touched to search for detected planes.
            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon | 
                TrackableHitFlags.FeaturePointWithSurfaceNormal;

            if(Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
            {
                // Check if the hit is from the back of the plane, if it is, there is 
                // no need to create an anchor
                if((hit.Trackable is DetectedPlane) && 
                    Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position, hit.Pose.rotation * Vector3.up) < 0)
                {
                    Debug.Log("There was a hit at the back of the current DetectedPlane");
                }
                else
                {
                    // Check the type and orientation of the hit object and choose the GameObject 
                    // that needs to be instantiated
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
                    // Fallback
                    else
                    {
                        objectToBeInstantiated = GameObjectForHorizontalPlane;
                    }

                    // Instantiate the object at the hit pose.
                    GameObject gameObject = Instantiate(objectToBeInstantiated, hit.Pose.position, hit.Pose.rotation);

                    // Compensate for the hit Pose rotation facing away from the raycast point (camera)
                    gameObject.transform.Rotate(0, PREFAB_ROTATION, 0, Space.Self);

                    // Create an anchor to allow ARCore to track the hitpoint as understanding of 
                    // the physical world evolves.
                    Anchor anchor = hit.Trackable.CreateAnchor(hit.Pose);

                    // Make the instantiated object a child of the anchor
                    gameObject.transform.parent = anchor.transform;
                }
            }
        }

        /// <summary>
        /// Check and update the application lifecycle.
        /// </summary>
        private void UpdateApplicationLifecycle()
        {
            // Exit the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                appController.LoadScene("MainMenuScene");
            }

            // Only allow the screen to sleep when not tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                Screen.sleepTimeout = SleepTimeout.SystemSetting;
            }
            else
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            // If the app is already in a quitting state, skip the next verifications
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

        /// <summary>
        /// Actually quit the application.
        /// </summary>
        private void QuitTheApplication()
        {
            Application.Quit();
        }

        /// <summary>
        /// Show an Android toast message.
        /// </summary>
        /// <param name="message">Message string to show in the toast.</param>
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
