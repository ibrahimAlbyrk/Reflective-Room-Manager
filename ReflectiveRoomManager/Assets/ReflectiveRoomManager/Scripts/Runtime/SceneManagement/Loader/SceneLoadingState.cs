namespace REFLECTIVE.Runtime.SceneManagement.Loader
{
    public class SceneLoadingState
    {
        public bool IsCurrentlyLoading { get; private set; }
        
        public void StartLoading()
        {
            IsCurrentlyLoading = true;
        }

        public void FinishLoading()
        {
            IsCurrentlyLoading = false;
        }
    }
}