namespace BuildYourOwnVectorStore.Helpers;

public static class VectorMatch //Don't ask me how this code works :-P
{
    public static float MatchScore(ReadOnlyMemory<float> a, ReadOnlyMemory<float> b)
    {
        float cos = CosineSimilarity(a, b);
        return cos <= 0.0f ? 0.0f : cos;
    }

    private static float CosineSimilarity(ReadOnlyMemory<float> a, ReadOnlyMemory<float> b)
    {
        ReadOnlySpan<float> sa = a.Span;
        ReadOnlySpan<float> sb = b.Span;

        if (sa.Length != sb.Length)
        {
            throw new ArgumentException("Vectors must have the same dimension.");
        }

        double dot = 0.0;
        double normA = 0.0;
        double normB = 0.0;

        for (int i = 0; i < sa.Length; i++)
        {
            double ai = sa[i];
            double bi = sb[i];

            dot += ai * bi;
            normA += ai * ai;
            normB += bi * bi;
        }

        double denom = Math.Sqrt(normA) * Math.Sqrt(normB);
        if (denom == 0.0)
        {
            return 0.0f;
        }

        return (float)(dot / denom);
    }
}