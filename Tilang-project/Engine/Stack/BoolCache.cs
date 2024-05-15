namespace Tilang_project.Engine.Stack
{
    public class BoolCache
    {
        private List<bool> previousProcess = new List<bool>();

        public int Length { get
            {
                return previousProcess.Count;
            } }
        public void Append(bool cache)
        {
            previousProcess.Add(cache);
        }

        public bool GetLatest()
        {
            if(previousProcess.Count == 0) return false;
            return previousProcess[previousProcess.Count - 1];
        }

        public void Clear() { previousProcess.Clear();  }
    }
}
