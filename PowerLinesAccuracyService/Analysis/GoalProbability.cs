namespace PowerLinesAccuracyService.Analysis
{
    public class GoalProbability
    {
        public int Goals { get; private set; }
        public decimal Probability { get; private set; }

        public GoalProbability(int goals, decimal probability)
        {
            Goals = goals;
            Probability = probability;
        }
    }
}
