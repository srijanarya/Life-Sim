using UnityEngine;

namespace LifeCraft.UI
{
    [CreateAssetMenu(fileName = "LifeCraftTheme", menuName = "LifeCraft/UI Theme", order = 1)]
    public class LifeCraftTheme : ScriptableObject
    {
        [Header("Background Colors")]
        public Color backgroundDark = new Color(0.118f, 0.094f, 0.086f, 1f);
        public Color backgroundMid = new Color(0.188f, 0.153f, 0.133f, 1f);
        public Color backgroundLight = new Color(0.976f, 0.961f, 0.937f, 1f);
        
        [Header("Accent Colors")]
        public Color primaryAccent = new Color(0.796f, 0.431f, 0.353f, 1f);
        public Color secondaryAccent = new Color(0.6f, 0.667f, 0.596f, 1f);
        public Color tertiaryAccent = new Color(0.867f, 0.788f, 0.694f, 1f);
        
        [Header("Text Colors")]
        public Color textPrimary = new Color(0.976f, 0.961f, 0.937f, 1f);
        public Color textSecondary = new Color(0.7f, 0.65f, 0.6f, 1f);
        public Color textMuted = new Color(0.5f, 0.45f, 0.4f, 1f);
        public Color textOnAccent = Color.white;
        
        [Header("Status Colors")]
        public Color success = new Color(0.486f, 0.702f, 0.478f, 1f);
        public Color warning = new Color(0.945f, 0.769f, 0.059f, 1f);
        public Color error = new Color(0.898f, 0.345f, 0.329f, 1f);
        public Color info = new Color(0.396f, 0.612f, 0.851f, 1f);
        
        [Header("Button Styles")]
        public Color buttonPrimary = new Color(0.796f, 0.431f, 0.353f, 1f);
        public Color buttonSecondary = new Color(0.976f, 0.961f, 0.937f, 0.15f);
        public Color buttonTertiary = new Color(0f, 0f, 0f, 0f);
        public Color buttonDisabled = new Color(0.4f, 0.4f, 0.4f, 0.5f);
        
        [Header("Card & Panel")]
        public Color cardBackground = new Color(0.15f, 0.12f, 0.11f, 0.95f);
        public Color cardBorder = new Color(1f, 1f, 1f, 0.1f);
        public Color panelOverlay = new Color(0f, 0f, 0f, 0.6f);
        
        [Header("Animation Timings")]
        public float fastDuration = 0.15f;
        public float normalDuration = 0.3f;
        public float slowDuration = 0.5f;
        public float staggerDelay = 0.08f;
        
        [Header("Spacing")]
        public float spacingTiny = 4f;
        public float spacingSmall = 8f;
        public float spacingMedium = 16f;
        public float spacingLarge = 24f;
        public float spacingXLarge = 40f;
        
        [Header("Touch Targets")]
        public float minTouchSize = 44f;
        public float buttonHeightPrimary = 56f;
        public float buttonHeightSecondary = 48f;
        
        [Header("Border Radius")]
        public float radiusSmall = 8f;
        public float radiusMedium = 12f;
        public float radiusLarge = 16f;
        public float radiusRound = 100f;
        
        public static LifeCraftTheme Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<LifeCraftTheme>("LifeCraftTheme");
                    
                    if (_instance == null)
                    {
                        _instance = CreateInstance<LifeCraftTheme>();
                    }
                }
                return _instance;
            }
        }
        private static LifeCraftTheme _instance;
        
        public Color GetStatusColor(StatusType status)
        {
            return status switch
            {
                StatusType.Success => success,
                StatusType.Warning => warning,
                StatusType.Error => error,
                StatusType.Info => info,
                _ => textPrimary
            };
        }
    }
    
    public enum StatusType
    {
        Success,
        Warning,
        Error,
        Info
    }
}
