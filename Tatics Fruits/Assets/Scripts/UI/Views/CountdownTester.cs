using UnityEngine;
using UI.Views;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UI.Views
{
    [RequireComponent(typeof(CountdownView))]
    public class CountdownTester : MonoBehaviour
    {
        [Header("Test Controls")]
        [Tooltip("Press this key in Play Mode to trigger countdown")]
        [SerializeField] private KeyCode testKey = KeyCode.Space;
        
        [Tooltip("Auto-play countdown when Play Mode starts")]
        [SerializeField] private bool autoPlayOnStart = false;
        
        [Tooltip("Delay before auto-play (seconds)")]
        [SerializeField] private float autoPlayDelay = 1f;

        private CountdownView _countdownView;
        private bool _isPlaying = false;

        private void Awake()
        {
            _countdownView = GetComponent<CountdownView>();
        }

        private void Start()
        {
            if (autoPlayOnStart)
            {
                Invoke(nameof(TriggerCountdown), autoPlayDelay);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(testKey) && !_isPlaying)
            {
                TriggerCountdown();
            }
        }

        public void TriggerCountdown()
        {
            if (_countdownView != null && !_isPlaying)
            {
                _isPlaying = true;
                StartCoroutine(PlayCountdownCoroutine());
            }
        }

        private System.Collections.IEnumerator PlayCountdownCoroutine()
        {
            yield return _countdownView.PlayCountdown();
            _isPlaying = false;
            Debug.Log("[CountdownTester] Countdown finished!");
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CountdownTester))]
public class CountdownTesterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "Test the countdown effect:\n\n" +
            "• In Play Mode: Press the Test Key (default: Space)\n" +
            "• Or enable 'Auto Play On Start'\n" +
            "• Or click the 'Test Countdown' button below",
            MessageType.Info);

        EditorGUILayout.Space(5);

        CountdownTester tester = (CountdownTester)target;

        GUI.enabled = Application.isPlaying;
        if (GUILayout.Button("🎬 Test Countdown", GUILayout.Height(30)))
        {
            tester.TriggerCountdown();
        }
        GUI.enabled = true;

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to test the countdown", MessageType.Warning);
        }
    }
}
#endif
