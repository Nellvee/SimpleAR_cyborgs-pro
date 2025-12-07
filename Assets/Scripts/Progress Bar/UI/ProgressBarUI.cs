using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Project.UI.ProgressBar
{
    /// <summary>
    /// Simple UI Progress bar that uses Scrollbar as fill image
    /// </summary>
    public class ProgressBarUI : MonoBehaviour
    {
        [SerializeField]
        private Scrollbar bar;
        [SerializeField]
        private TextMeshProUGUI barProgressText;

        private ProgressFloat progressFloat = new();
        private void Awake()
        {
            progressFloat.OnProgressChanged += OnProgressChanged;
            OnProgressChanged(progressFloat.Progress);
        }
        public void UpdateValue(float value)
        {
            progressFloat.SetProgress(value);
        }
        private void OnProgressChanged(float value)
        {
            if(bar != null)
            {
                bar.size = value;
            }
            if (barProgressText != null)
            {
                barProgressText.text = value.ToString("00.00%");
            }
        }
    }
}