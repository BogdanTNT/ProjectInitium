using System.Collections.Generic;

namespace Initium.Data.Error
{
    using Elements;

    public class InitiumNodeErrorData
    {
        public InitiumErrorData ErrorData { get; set; }
        public List<InitiumNode> Nodes { get; set; }

        public InitiumNodeErrorData()
        {
            ErrorData = new InitiumErrorData();
            Nodes = new List<InitiumNode>();
        }
    }
}