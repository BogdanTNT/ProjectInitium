using System.Collections.Generic;

namespace Initium.Data.Error
{
    using Elements;

    public class InitiumGroupErrorData
    {
        public InitiumErrorData ErrorData { get; set; }
        public List<InitiumGroup> Groups { get; set; }

        public InitiumGroupErrorData()
        {
            ErrorData = new InitiumErrorData();
            Groups = new List<InitiumGroup>();
        }
    }
}