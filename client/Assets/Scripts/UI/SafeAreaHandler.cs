using UnityEngine;

namespace LifeCraft.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaHandler : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastSafeArea = Rect.zero;
        private Vector2Int lastScreenSize = Vector2Int.zero;
        private ScreenOrientation lastOrientation = ScreenOrientation.AutoRotation;
        
        [SerializeField] private bool applyTop = true;
        [SerializeField] private bool applyBottom = true;
        [SerializeField] private bool applyLeft = true;
        [SerializeField] private bool applyRight = true;
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }
        
        private void Start()
        {
            ApplySafeArea();
        }
        
        private void Update()
        {
            if (SafeAreaChanged())
            {
                ApplySafeArea();
            }
        }
        
        private bool SafeAreaChanged()
        {
            return lastSafeArea != Screen.safeArea ||
                   lastScreenSize.x != Screen.width ||
                   lastScreenSize.y != Screen.height ||
                   lastOrientation != Screen.orientation;
        }
        
        private void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;
            
            lastSafeArea = safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            lastOrientation = Screen.orientation;
            
            if (Screen.width <= 0 || Screen.height <= 0) return;
            
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            
            if (!applyLeft) anchorMin.x = 0f;
            if (!applyBottom) anchorMin.y = 0f;
            if (!applyRight) anchorMax.x = 1f;
            if (!applyTop) anchorMax.y = 1f;
            
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
        
        public void ForceRefresh()
        {
            ApplySafeArea();
        }
    }
}
