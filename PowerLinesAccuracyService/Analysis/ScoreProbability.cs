namespace PowerLinesAccuracyService.Analysis
{
    public class ScoreProbability
    {
        public GoalProbability HomeGoalProbability { get; private set; }
        public GoalProbability AwayGoalProbability { get; private set; }
        public decimal Probability { get; private set; }

        public char Result
        {
            get
            {
                if(HomeGoalProbability.Goals > AwayGoalProbability.Goals)
                {
                    return 'H';
                }
                if(AwayGoalProbability.Goals > HomeGoalProbability.Goals)
                {
                    return 'A';
                }
                return 'D';
            }
        }

        public ScoreProbability(GoalProbability homeGoalProbability, GoalProbability awayGoalProbability)
        {
            HomeGoalProbability = homeGoalProbability;
            AwayGoalProbability = awayGoalProbability;
        }

        public void CalculateProbability()
        {
            Probability = HomeGoalProbability.Probability * AwayGoalProbability.Probability;
        }
    }
}
