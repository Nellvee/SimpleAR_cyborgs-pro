using System;
namespace Project.UI.ProgressBar
{
    /// <summary>
    /// Float Value with value changed event.
    /// </summary>
    public class ProgressFloat
    {
        public event Action<float> OnProgressChanged;

        private float progress;
        public float Progress => progress;
        public ProgressFloat()
        {

        }
        public ProgressFloat(float progress) : this()
        {
            this.progress = progress;
        }
        public void SetProgress(float progress)
        {
            this.progress = progress;
            OnProgressChanged?.Invoke(progress);
        }
    }
}