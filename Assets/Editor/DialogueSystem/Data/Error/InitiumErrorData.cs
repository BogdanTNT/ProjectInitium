using UnityEngine;

namespace Initium.Data.Error
{
    public class InitiumErrorData
    {
        public Color Color { get; set; }

        public InitiumErrorData()
        {
            GenerateRandomColor();
        }

        private void GenerateRandomColor()
        {
            Color = new Color32(
                (byte) Random.Range(65, 256),
                (byte) Random.Range(50, 176),
                (byte) Random.Range(50, 176),
                255
            );
        }
    }
}