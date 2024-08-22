namespace KSM
{
    using UnityEngine;

    public class KSMPermanentData : ScriptableObject
    {
        [SerializeField] private string folderpath;
        [SerializeField] private int selector;
        public string Folderpath { get { return this.folderpath; } set { this.folderpath = value; } }
        public int Selector { get { return this.selector; } set { this.selector = value; } }
    }
}
