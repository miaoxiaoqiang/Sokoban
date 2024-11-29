using Sokoban.Model;

namespace Sokoban.Core
{
    internal interface IStage
    {
        public Stage GetNextStage(Stage currentStage);

        public int GetIndex(Stage currentStage);
    }
}
