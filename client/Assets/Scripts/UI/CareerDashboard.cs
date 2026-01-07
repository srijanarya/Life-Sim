using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using LifeCraft.Data;

namespace LifeCraft.UI
{
    public class CareerDashboard : MonoBehaviour
    {
        [Header("UI Components")]
        public TextMeshProUGUI CareerName;
        public TextMeshProUGUI CareerType;
        public TextMeshProUGUI CurrentLevel;
        public TextMeshProUGUI YearsInCareer;
        public Slider ProgressSlider;
        public TextMeshProUGUI CurrentSalary;
        public TextMeshProUGUI MaxSalary;
        public TextMeshProUGUI NextPromotionYears;
        public GameObject PromotionBar;
        public Image PromotionFill;
        public TextMeshProUGUI PromotionProgress;
        public GameObject SkillsSection;
        public GameObject MilestonesSection;

        [Header("Settings")]
        public Color PrimaryColor = new Color(0.796f, 0.431f, 0.353f);
        public Color SecondaryColor = new Color(0.6f, 0.667f, 0.596f);
        public float UpdateAnimationDuration = 0.5f;

        private PlayerCareer currentCareer;
        private Career careerTemplate;

        private void Start()
        {
            UpdateCareerInfo();
        }

        public void UpdateCareerInfo()
        {
            var gameState = GameManager.Instance?.CurrentGameState;
            var profile = GameManager.Instance?.PlayerProfile;

            if (gameState == null || profile == null)
            {
                ShowEmptyState();
                return;
            }

            StartCoroutine(LoadCareerData(gameState));
        }

        private IEnumerator LoadCareerData(GameState gameState)
        {
            yield return ApiClient.Instance.Get<PlayerCareer>(
                $"/api/player/{gameState.PlayerId}/career/current",
                (careerData) =>
                {
                    currentCareer = careerData;
                    StartCoroutine(LoadCareerTemplate(careerData.CareerId));
                },
                (error) =>
                {
                    Debug.LogError($"Failed to load career data: {error}");
                    ShowEmptyState();
                }
            );
        }

        private IEnumerator LoadCareerTemplate(string careerId)
        {
            yield return ApiClient.Instance.Get<Career>(
                $"/api/careers/{careerId}",
                (career) =>
                {
                    careerTemplate = career;
                    UpdateUI();
                },
                (error) =>
                {
                    Debug.LogError($"Failed to load career template: {error}");
                }
            );
        }

        private void UpdateUI()
        {
            if (currentCareer == null || careerTemplate == null) return;

            StartCoroutine(AnimateCareerUpdate());
        }

        private IEnumerator AnimateCareerUpdate()
        {
            float elapsedTime = 0f;

            while (elapsedTime < UpdateAnimationDuration)
            {
                float t = elapsedTime / UpdateAnimationDuration;
                UpdateCareerUI(t);
                yield return null;
                elapsedTime += Time.deltaTime;
            }

            UpdateCareerUI(1f);
        }

        private void UpdateCareerUI(float t)
        {
            if (CareerName != null)
            {
                CareerName.text = careerTemplate.Name;
            }

            if (CareerType != null)
            {
                CareerType.text = FormatCareerType(careerTemplate.Type);
            }

            if (CurrentLevel != null)
            {
                CurrentLevel.text = $"Level {currentCareer.Level}";
            }

            if (YearsInCareer != null)
            {
                YearsInCareer.text = $"{currentCareer.YearsInCareer} years";
            }

            if (ProgressSlider != null)
            {
                float progress = (float)currentCareer.YearsInCareer / careerTemplate.PromotionYears;
                ProgressSlider.value = Mathf.Lerp(ProgressSlider.value, progress, t);
            }

            if (CurrentSalary != null)
            {
                string salary = FormatCurrency(currentCareer.CurrentSalary);
                CurrentSalary.text = salary;
            }

            if (MaxSalary != null)
            {
                string maxSalary = FormatCurrency(careerTemplate.MaxSalary);
                MaxSalary.text = maxSalary;
            }

            if (NextPromotionYears != null)
            {
                int yearsUntilPromotion = careerTemplate.PromotionYears - currentCareer.YearsInCareer;
                NextPromotionYears.text = yearsUntilPromotion <= 0 
                    ? "Eligible for promotion!" 
                    : $"{yearsUntilPromotion} years until promotion";
            }

            if (PromotionBar != null && PromotionFill != null)
            {
                float progress = Mathf.Clamp01((float)currentCareer.YearsInCareer / careerTemplate.PromotionYears);
                PromotionFill.fillAmount = Mathf.Lerp(PromotionFill.fillAmount, progress, t);
            }

            if (PromotionProgress != null)
            {
                PromotionProgress.text = $"{Mathf.RoundToInt(PromotionFill.fillAmount * 100)}%";
            }
        }

        private string FormatCareerType(string careerType)
        {
            return careerType.Replace('_', ' ');
        }

        private string FormatCurrency(int amount)
        {
            return string.Format("${0:N0}", amount);
        }

        private void ShowEmptyState()
        {
            if (CareerName != null) CareerName.text = "No Career";
            if (CurrentLevel != null) CurrentLevel.text = "-";
            if (YearsInCareer != null) YearsInCareer.text = "0 years";
            if (ProgressSlider != null) ProgressSlider.value = 0f;
            if (CurrentSalary != null) CurrentSalary.text = "$0";
        }
    }
}
